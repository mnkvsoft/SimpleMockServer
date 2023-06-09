﻿using Microsoft.Extensions.DependencyInjection;
using SimpleMockServer.Domain.Matching.Conditions;

namespace SimpleMockServer.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services.AddSingleton<IRequestStatisticStorage, RequestStatisticStorage>();
    }
}
