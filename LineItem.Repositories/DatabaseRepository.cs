using LineItem.Repositories.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LineItem.Repositories;

public class DatabaseRepository
{
    private readonly string _connectionString;

    protected DatabaseRepository(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    protected NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}