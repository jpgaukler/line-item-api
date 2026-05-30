using System;
using LineItem.Migrations.Metadata;
using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            var builder = new ConfigurationBuilder().AddCommandLine(args);

            var configuration = builder.Build();

            var connectionString = configuration["connectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("Connection string is null or undefined!");
            }

            var serviceProvider = new ServiceCollection()
                .AddScoped<IVersionTableMetaData, VersionTableMetadata>()
                .AddFluentMigratorCore()
                .ConfigureRunner(runner =>
                    runner
                        .AddPostgres()
                        .WithGlobalCommandTimeout(TimeSpan.FromMinutes(30))
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof(Program).Assembly)
                        .For.Migrations()
                )
                .AddLogging(configuration => configuration.AddFluentMigratorConsole())
                .BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            if (long.TryParse(configuration["downgradeToRevision"], out var version))
            {
                Log.Information("Starting database migration downgrade.");
                runner.MigrateDown(version);
                Log.Information($"Database downgrade to version {version} completed successfully.");
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
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
