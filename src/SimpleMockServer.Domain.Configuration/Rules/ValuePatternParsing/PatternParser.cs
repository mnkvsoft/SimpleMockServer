﻿using System.Text;

namespace SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;

using static Consts.ExecutedBlock;

internal static class PatternParser
{
    public static IReadOnlyCollection<PatternPart> Parse(string pattern)
    {
        var parts = new List<PatternPart>();
        var sb = new StringBuilder();

        var iterator = pattern.GetEnumerator();
        var writeCallChain = false;

        while (iterator.MoveNext())
        {
            if (iterator.Current != BeginChar)
            {
                sb.Append(iterator.Current);
            }
            else
            {
                iterator.MoveNext();
                if (iterator.Current == BeginChar)
                {
                    parts.Add(new PatternPart.Static(sb.ToString()));
                    sb.Clear();

                    writeCallChain = true;
                    var callChain = new StringBuilder();
                    while (iterator.MoveNext())
                    {
                        if (iterator.Current == EndChar)
                        {
                            iterator.MoveNext();
                            if (iterator.Current == EndChar)
                            {
                                writeCallChain = false;
                                break;
                            }
                            else
                            {
                                callChain.Append(EndChar);
                                callChain.Append(iterator.Current);
                            }
                        }
                        else
                            callChain.Append(iterator.Current);
                    }

                    parts.Add(new PatternPart.Dynamic(callChain.ToString().Trim()));
                }
                else
                {
                    sb.Append(BeginChar);
                    sb.Append(iterator.Current);
                }
            }
        }

        if (writeCallChain)
            throw new Exception($"Open block {{{{ not close in pattern '{pattern}'");

        parts.Add(new PatternPart.Static(sb.ToString()));
        return parts;
    }
}
