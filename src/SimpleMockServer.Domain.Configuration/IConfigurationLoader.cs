﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SimpleMockServer.Configuration;
using SimpleMockServer.Domain.Configuration.DataModel;
using SimpleMockServer.Domain.Configuration.Rules;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.Configuration.Templating;
using SimpleMockServer.Domain.Configuration.Variables;
using SimpleMockServer.Domain.DataModel;
using SimpleMockServer.Domain.TextPart.CSharp;
using SimpleMockServer.Domain.TextPart.Variables;

namespace SimpleMockServer.Domain.Configuration;

public interface IConfigurationLoader
{
    Task<ConfigurationState> GetState();
    void ProvokeLoad();
}

class ConfigurationLoader : IDisposable, IRulesProvider, IDataProvider, IConfigurationLoader
{
    private readonly string _path;
    private readonly ILogger _logger;

    private readonly PhysicalFileProvider _fileProvider;
    private IChangeToken? _fileChangeToken;

    private readonly GlobalVariablesParser _globalVariablesParser;
    private readonly DataLoader _dataLoader;

    private Task<LoadResult> _loadTask;
    private ConfigurationState? _providerState;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ConfigurationLoader(
        IServiceScopeFactory serviceScopeFactory,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        GlobalVariablesParser globalVariablesParser,
        DataLoader dataLoader)
    {
        _globalVariablesParser = globalVariablesParser;
        _dataLoader = dataLoader;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = loggerFactory.CreateLogger(GetType());
        _path = configuration.GetRulesPath();

        _loadTask = Load(_path);

        _logger.LogInformation($"Rules path for watching: {_path}");

        _fileProvider = new PhysicalFileProvider(_path);
        _fileProvider.UsePollingFileWatcher = true;
        _fileProvider.UseActivePolling = true;

        WatchForFileChanges();
    }

    record LoadResult(IReadOnlyCollection<Rule> Rules, Dictionary<DataName, Data> Datas);

    public void ProvokeLoad()
    {
        // loading was init in constructor
        // invoke only for create instanse
    }

    private async Task<LoadResult> Load(string path)
    {
        var datas = await _dataLoader.Load(path);

        var templates = await TemplatesLoader.Load(path);


        var context = new ParsingContext(
            new VariableSet(),
            templates,
            RootPath: path,
            CurrentPath: path);

        var variables = await _globalVariablesParser.Load(context, path);

        var scope = _serviceScopeFactory.CreateScope();
        var provider = scope.ServiceProvider;

        var rulesLoader = provider.GetRequiredService<RulesLoader>();
        var rules = await rulesLoader.LoadRules(path, context with { Variables = variables });
        var csharpFactory = provider.GetRequiredService<IGeneratingCSharpFactory>();

        var stat = csharpFactory.CompilationStatistic;
        _logger.LogInformation($"Dynamic csharp compilation statistic: " + Environment.NewLine +
                               $"Total time: {(int)stat.TotalTime.TotalMilliseconds} ms. " + Environment.NewLine +
                               $"Assembly load time: {(int)stat.TotalLoadAssemblyTime.TotalMilliseconds} ms. " + Environment.NewLine +
                               $"Count load assemblies: {stat.CountLoadAssemblies}. " + Environment.NewLine +
                               $"Compilation time: {(int)stat.TotalCompilationTime.TotalMilliseconds} ms. " + Environment.NewLine +
                               $"Max compilation time: {(int)stat.MaxCompilationTime.TotalMilliseconds} ms. " + Environment.NewLine +
                               $"Average compilation time: {(int)(stat.TotalCompilationTime.TotalMilliseconds / stat.CountLoadAssemblies)} ms.");

        return new LoadResult(rules, datas);
    }


    async Task<IReadOnlyCollection<Rule>> IRulesProvider.GetRules()
    {
        var (rules, _) = await _loadTask;
        return rules;
    }

    Data IDataProvider.GetData(DataName name)
    {
        var (_, datas) = _loadTask.Result;
        return datas[name];
    }

    private void WatchForFileChanges()
    {
        _fileChangeToken = _fileProvider.Watch("**/*.*");
        _fileChangeToken.RegisterChangeCallback(Notify, default);
    }

    private void Notify(object? state)
    {
        OnChange();
        WatchForFileChanges();
    }

    private void OnChange()
    {
        _logger.LogInformation($"Change was detected in {_path}");
        _providerState = null;
        _loadTask = Load(_path);
    }

    public async Task<ConfigurationState> GetState()
    {
        if (_providerState != null)
            return _providerState;

        try
        {
            await _loadTask;
            _providerState = new ConfigurationState.Ok();
        }
        catch (Exception e)
        {
            _providerState = new ConfigurationState.Error(e);
        }

        return _providerState;
    }

    public void Dispose()
    {
        _fileProvider.Dispose();
    }
}
