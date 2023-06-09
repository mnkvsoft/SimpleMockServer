﻿namespace SimpleMockServer.Domain.Configuration.Rules;

class Constants
{
    public static class SectionName
    {
        public const string Rule = "rule";
        public const string Variables = "variables";
        public const string Templates = "templates";
        public const string Condition = "condition";
        public const string Response = "response";
        public const string CallPrefix = "call";
    }

    public static class BlockName
    {
        public class Common
        {
            public const string Delay = "delay";
        }

        public class Rule
        {
            public const string Method = "method";
            public const string Path = "path";
            public const string Query = "query";
            public const string Headers = "headers";
            public const string Body = "body";
        }

        public class Response
        {
            public const string Code = "code";
            public const string Headers = "headers";
            public const string Body = "body";
            public const string Delay = Common.Delay;
        }
    }

    public static readonly HashSet<string> HttpMethods = new()
    {
        "GET",
        "PUT",
        "POST",
        "DELETE",
        "HEAD",
        "TRACE",
        "OPTIONS",
        "CONNECT"
    };
}
