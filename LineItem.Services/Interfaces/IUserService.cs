using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Services.Interfaces;

public interface IUserService
{
    public Task<long> CreateAsync(UserModel user, CancellationToken cancellationToken);

    public Task<UserModel?> RetrieveByIdAsync(long id, CancellationToken cancellationToken);

    public Task<bool> UpdateAsync(
        UserModel user,
        CancellationToken cancellationToken
    );

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
}