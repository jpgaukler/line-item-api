using Dapper;
using LineItem.Repositories.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LineItem.Repositories.Helpers;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureDapperMapping()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            return services;
        }

        public IServiceCollection AddDatabaseOptions(IConfiguration configuration)
        {
            services.AddOptions<DatabaseOptions>()
                .Bind(configuration.GetSection(nameof(DatabaseOptions)))
                .Validate(options =>
                        !string.IsNullOrWhiteSpace(options.Host) &&
                        !string.IsNullOrWhiteSpace(options.Port) &&
                        !string.IsNullOrWhiteSpace(options.Database) &&
                        !string.IsNullOrWhiteSpace(options.Username) &&
                        !string.IsNullOrWhiteSpace(options.Password),
                    $"{nameof(DatabaseOptions)} is invalid. Host, Port, Database, Username, and Password are required."
                )
                .ValidateOnStart();

            return services;
        }
    }
}