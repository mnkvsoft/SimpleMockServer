﻿namespace SimpleMockServer.Domain.TextPart.Functions.Functions.Transform.Format;

record FormatFunction(string Format) : ITransformFunction
{
    public object? Transform(object? input)
    {
        return input?.FormatOrThrow(Format);
    }
}
