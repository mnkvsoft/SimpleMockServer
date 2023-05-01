﻿using SimpleMockServer.Common.Extensions;
using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.Domain.TextPart.Variables;
using SimpleMockServer.Domain.TextPart.Variables.Request;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Rules.Parsers;

class VariablesParser
{
    private readonly ITextPartsParser _textGeneratorFactory;

    public VariablesParser(ITextPartsParser textGeneratorFactory)
    {
        _textGeneratorFactory = textGeneratorFactory;
    }

    public VariableSet<RequestVariable> Parse(FileSection variablesSection)
    {
        var set = new VariableSet<RequestVariable>();

        foreach (var line in variablesSection.LinesWithoutBlock)
        {
            (var name, var pattern) = line.SplitToTwoParts("=").Trim();

            if (string.IsNullOrEmpty(name))
                throw new Exception($"RequestVariable name not defined. Line: {line}");

            if (string.IsNullOrEmpty(pattern))
                throw new Exception($"RequestVariable '{name}' not initialized. Line: {line}");

            var generator = _textGeneratorFactory.Parse(pattern, set);
            set.Add(new RequestVariable(name, generator));
        }

        return set;
    }
}
