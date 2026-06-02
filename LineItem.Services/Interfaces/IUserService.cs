using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Services.Interfaces;

public interface IUserService
{
    public Task<UserModel> CreateAsync(UserModel user, CancellationToken cancellationToken);

    public Task<UserModel?> RetrieveByIdAsync(long id, CancellationToken cancellationToken);

    public Task<UserModel?> UpdateAsync(
        long id,
        UserModel user,
        CancellationToken cancellationToken
    );

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
}