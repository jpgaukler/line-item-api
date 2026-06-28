using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using LineItem.Repositories.Options;
using Microsoft.Extensions.Options;

namespace LineItem.Repositories;

public class UserRepository : DatabaseRepository, IUserRepository
{
    public UserRepository(IOptions<DatabaseOptions> options) : base(options)
    {
    }

    public async Task<UserModel> CreateAsync(UserModel user, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.app_user_insert(@external_id, @display_name);",
            new
            {
                external_id = user.ExternalId,
                display_name = user.DisplayName
            },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleAsync<UserModel>(command);
        return result;
    }

    public async Task<UserModel?> RetrieveByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.app_user_retrieve_by_id(@id);",
            new { id },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleOrDefaultAsync<UserModel>(command);
        return result;
    }

    public async Task<UserModel?> RetrieveByExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.app_user_retrieve_by_external_id(@external_id);",
            new { external_id = externalId },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleOrDefaultAsync<UserModel>(command);
        return result;
    }

    public async Task<UserModel?> UpdateAsync(long id, UserModel user, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.app_user_update(@id, @external_id, @display_name);",
            new
            {
                id,
                external_id = user.ExternalId,
                display_name = user.DisplayName
            },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleOrDefaultAsync<UserModel>(command);
        return result;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT lineitem.app_user_delete(@id);",
            new { id },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
}