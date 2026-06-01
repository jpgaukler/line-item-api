using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;
using LineItem.Repositories.Helpers;
using LineItem.Repositories.Interfaces;
using Npgsql;

namespace LineItem.Repositories;

public class UserRepository : DatabaseRepository, IUserRepository
{
    public UserRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<long> CreateAsync(UserModel user, CancellationToken cancellationToken)
    {
        const string query = "SELECT lineitem.app_user_insert(@external_id, @display_name);";
        var parameters = new Dictionary<string, object>
        {
            { "@external_id", user.ExternalId },
            { "@display_name", user.DisplayName }
        };

        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateCommand(query, parameters, connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);

        if (result == null || result == DBNull.Value)
            throw new Exception("User creation failed; no Id returned.");

        return Convert.ToInt64(result);
    }

    public async Task<UserModel?> RetrieveByIdAsync(int id, CancellationToken cancellationToken)
    {
        const string query = "SELECT * FROM app_user_retrieve_by_id(@id);";
        var parameters = new Dictionary<string, object>
        {
            { "@id", id }
        };

        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateCommand(query, parameters, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        UserModel? user = null;

        if (await reader.ReadAsync(cancellationToken))
            user = ParseUser(reader);

        return user;
    }


    public async Task<bool> UpdateAsync(UserModel user, CancellationToken cancellationToken)
    {
        const string query = "SELECT lineitem.app_user_update(@external_id, @display_name);";
        var parameters = new Dictionary<string, object>
        {
            { "@external_id", user.ExternalId },
            { "@display_name", user.DisplayName }
        };

        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateCommand(query, parameters, connection);
        var result = (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
        return result;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        const string query = "SELECT lineitem.app_user_delete(@id);";
        var parameters = new Dictionary<string, object>
        {
            { "@id", id }
        };

        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateCommand(query, parameters, connection);
        var result = (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
        return result;
    }

    private static UserModel ParseUser(NpgsqlDataReader r)
    {
        return new UserModel
        {
            Id = r.Long("id"),
            ExternalId = r.String("external_id"),
            DisplayName = r.String("display_name"),
            CreatedAt = r.DateTime("created_at"),
            UpdatedAt = r.DateTime("updated_at")
        };
    }
}