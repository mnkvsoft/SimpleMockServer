﻿using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Rules;

public interface IExternalCallerRegistrator
{
    string Name { get; }

    IReadOnlyCollection<string> GetSectionKnowsBlocks();

    Task<IExternalCaller> Create(FileSection section, IParsingContext parsingContext);
}
