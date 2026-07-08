using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Repositories.Interfaces;

public interface IUserRepository
{
    /// <summary>
    ///     Creates a new user.
    /// </summary>
    /// <returns>The created user.</returns>
    public Task<UserModel> CreateAsync(UserModel user, CancellationToken cancellationToken);

    /// <summary>
    ///     Retrieve a single user by id.
    /// </summary>
    public Task<UserModel?> RetrieveByIdAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    ///     Retrieves a user by external id linked to the identity provider.
    /// </summary>
    /// <returns>The user or null if no match is found.</returns>
    public Task<UserModel?> RetrieveByExternalIdAsync(string externalId, CancellationToken cancellationToken);

    /// <summary>
    ///     Updates a user record.
    /// </summary>
    /// <returns>The updated user or null if no matching user is found.</returns>
    public Task<UserModel?> UpdateAsync(long id, UserModel user, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a user record by id.
    /// </summary>
    public Task DeleteAsync(long id, CancellationToken cancellationToken);
}