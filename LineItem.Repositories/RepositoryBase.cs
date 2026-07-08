using LineItem.Repositories.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LineItem.Repositories;

public class RepositoryBase
{
    private readonly string _connectionString;

    protected RepositoryBase(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    protected NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}