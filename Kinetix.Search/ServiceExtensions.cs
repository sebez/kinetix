﻿using Kinetix.Search.MetaModel;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Search.Elastic
{
    /// <summary>
    /// Enregistre Kinetix.Search dans ASP.NET Core.
    /// </summary>
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSearch(this IServiceCollection services, string defaultDataSourceName)
        {
            return services
                .AddSingleton<DocumentDescriptor>()
                .AddSingleton(provider => new SearchManager(defaultDataSourceName, provider));
        }
    }
}
