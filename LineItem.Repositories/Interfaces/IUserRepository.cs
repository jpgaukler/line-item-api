using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Repositories.Interfaces;

public interface IUserRepository
{
    public Task<long> CreateAsync(UserModel user, CancellationToken cancellationToken);

    public Task<UserModel?> RetrieveByIdAsync(long id, CancellationToken cancellationToken);

    public Task<UserModel?> RetrieveByExternalIdAsync(string externalId, CancellationToken cancellationToken);

    public Task<bool> UpdateAsync(long id, UserModel user, CancellationToken cancellationToken);

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
}