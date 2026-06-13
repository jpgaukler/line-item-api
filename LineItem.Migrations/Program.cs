using System;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.VersionTableInfo;
using LineItem.Migrations.Metadata;
using LineItem.Repositories.Helpers;
using LineItem.Repositories.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace LineItem.Migrations;

public class Program
{
    public static void Main(string[] args)
    {
        // bootstrap logger for capturing issues during application startup
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

        try
        {
            var builder = Host.CreateApplicationBuilder(args);
            ConfigureServices(builder.Services, builder.Configuration);

            var services = builder.Build().Services;

            using var scope = services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            if (long.TryParse(builder.Configuration["downgradeToRevision"], out var version))
            {
                Log.Information("Starting database migration downgrade.");
                runner.MigrateDown(version);
                Log.Information("Database downgrade to version {Version} completed successfully.", version);
            }
            else
            {
                Log.Information("Starting database migration update.");
                runner.MigrateUp();
                Log.Information("Database upgrade completed successfully.");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application encountered a fatal unhandled exception");
            Environment.Exit(1); // Crucial for CLI to report a failure!
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabaseOptions(configuration)
            .AddScoped<IVersionTableMetaData, VersionTableMetadata>()
            .AddFluentMigratorCore()
            .ConfigureRunner(runner =>
                runner
                    .AddPostgres()
                    .WithGlobalCommandTimeout(TimeSpan.FromMinutes(30))
                    .ScanIn(typeof(Program).Assembly)
                    .For.Migrations()
            )
            .AddLogging(loggingBuilder => loggingBuilder.AddFluentMigratorConsole());

        // force connection string to resolve from IOptions after the ServiceProvider is built
        services.AddOptions<ProcessorOptions>()
            .PostConfigure<IOptions<DatabaseOptions>>((processorOptions, databaseOptions) =>
                processorOptions.ConnectionString = databaseOptions.Value.ConnectionString
            );
    }
}