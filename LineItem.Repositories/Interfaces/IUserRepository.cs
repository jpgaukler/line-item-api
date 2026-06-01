using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Repositories.Interfaces;

public interface IUserRepository
{
    public Task<long> CreateAsync(UserModel user, CancellationToken cancellationToken);

    public Task<UserModel?> GetByIdAsync(int id, CancellationToken cancellationToken);

    public Task<bool> UpdateAsync(UserModel user, CancellationToken cancellationToken);

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
