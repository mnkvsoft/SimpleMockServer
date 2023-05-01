﻿using SimpleMockServer.Domain.TextPart.Variables;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Rules;

public interface IExternalCallerRegistrator
{
    string Name { get; }

    IReadOnlyCollection<string> GetSectionKnowsBlocks();

    IExternalCaller Create(FileSection section, IReadOnlyCollection<Variable> variables);
}