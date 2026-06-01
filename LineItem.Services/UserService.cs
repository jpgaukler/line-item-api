using System.Threading;
using System.Threading.Tasks;
using LineItem.Exceptions;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using LineItem.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace LineItem.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<long> CreateAsync(UserModel user, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.RetrieveByExternalIdAsync(user.ExternalId, cancellationToken);

        if (existingUser != null)
            throw new BadRequestException($"A user with External Id = {user.ExternalId} already exists.");

        var id = await _userRepository.CreateAsync(user, cancellationToken);
        return id;
    }

    public Task<UserModel?> RetrieveByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _userRepository.RetrieveByIdAsync(id, cancellationToken);
    }

    public Task<bool> UpdateAsync(UserModel user, CancellationToken cancellationToken)
    {
        var result = _userRepository.UpdateAsync(user, cancellationToken);
        return result;
    }

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var result = _userRepository.DeleteAsync(id, cancellationToken);
        return result;
    }
}