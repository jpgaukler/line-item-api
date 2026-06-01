using Microsoft.Extensions.DependencyInjection;

namespace LineItem.Repositories.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseRepository<TInterface, TImplementation>(
        this IServiceCollection services,
        string connectionString)
        where TInterface : class
        where TImplementation : DatabaseRepository, TInterface
    {
        services.AddScoped<TInterface, TImplementation>(serviceProvider =>
            ActivatorUtilities.CreateInstance<TImplementation>(serviceProvider, connectionString));

        return services;
    }
}