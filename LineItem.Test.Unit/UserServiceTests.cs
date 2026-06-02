using System;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using LineItem.Services;
using LineItem.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LineItem.Test.Unit;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;

    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        var logger = new Mock<ILogger<UserService>>();

        _userService = new UserService(_userRepository.Object, logger.Object);
    }

    [Fact]
    public async Task CreateExample_WithInvalidName_ThrowsValidationException()
    {
        var example = new UserModel
        {
            Id = 1,
            ExternalId = "test|id",
            DisplayName = "Test",
            CreatedAt = DateTime.Now,
            UpdatedAt = null
        };

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            _userService.CreateAsync(example, CancellationToken.None)
        );

        Assert.Contains(ex.Message, "my message");
    }
}