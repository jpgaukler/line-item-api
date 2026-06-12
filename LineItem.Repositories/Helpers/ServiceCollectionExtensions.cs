using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace LineItem.Repositories.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureDapperMapping(this IServiceCollection services)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        return services;
    }
}