using Microsoft.Extensions.DependencyInjection;
using NexusMail.Application.Abstractions.Search;
using NexusMail.Search.Providers.Postgres;
using NexusMail.Search.Services;

namespace NexusMail.Search;

public static class DependencyInjection
{
    public static IServiceCollection AddSearchInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ISearchEngine, PgvectorHybridSearchEngine>();
        services.AddSingleton<ISearchRankingService, SearchRankingService>();
        
        return services;
    }
}
