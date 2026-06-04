using System.Threading;
using System.Threading.Tasks;
using LineItem.Exceptions;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using LineItem.Services.Interfaces;

namespace LineItem.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserModel> CreateAsync(UserModel user, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.RetrieveByExternalIdAsync(user.ExternalId, cancellationToken);

        if (existingUser != null)
            throw new BadRequestException($"A user with External Id = {user.ExternalId} already exists.");

        var createdUser = await _userRepository.CreateAsync(user, cancellationToken);
        return createdUser;
    }

    public Task<UserModel?> RetrieveByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _userRepository.RetrieveByIdAsync(id, cancellationToken);
    }

    public Task<UserModel?> RetrieveByExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        return _userRepository.RetrieveByExternalIdAsync(externalId, cancellationToken);
    }

    public async Task<UserModel?> UpdateAsync(long id, UserModel user, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.RetrieveByExternalIdAsync(user.ExternalId, cancellationToken);

        if (existingUser != null)
            throw new BadRequestException($"A user with External Id = {user.ExternalId} already exists.");

        var updatedUser = await _userRepository.UpdateAsync(id, user, cancellationToken);
        return updatedUser;
    }

    public Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        return _userRepository.DeleteAsync(id, cancellationToken);
    }
}