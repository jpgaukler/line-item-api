using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Repositories.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LineItem.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public DatabaseHealthCheck(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1;", connection);
            await command.ExecuteScalarAsync(cancellationToken);

            stopwatch.Stop();

            var data = new Dictionary<string, object>
            {
                { "duration", stopwatch.Elapsed.ToString("c") }
            };

            return HealthCheckResult.Healthy("Database connection successful.", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed!", ex);
        }
    }
}