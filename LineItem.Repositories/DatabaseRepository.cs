using System.Collections.Generic;
using LineItem.Repositories.Helpers;
using Npgsql;

namespace LineItem.Repositories;

public class DatabaseRepository
{
    private readonly string _connectionString;

    protected DatabaseRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    protected static NpgsqlCommand CreateCommand(
        string query,
        IEnumerable<KeyValuePair<string, object>> parameters,
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction = null
    )
    {
        var command = new NpgsqlCommand(query, connection, transaction);
        command.AddParameters(parameters);
        return command;
    }
}