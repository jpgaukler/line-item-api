using Microsoft.Data.SqlClient;

namespace LineItem.Repositories;

public class DatabaseRepository
{
    private readonly string _connectionString;

    public DatabaseRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
