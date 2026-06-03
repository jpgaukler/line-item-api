using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LineItem.Api.Middleware;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace LineItem.Test.Unit;

public class UserContextMiddlewareTests
{
    private readonly Mock<RequestDelegate> _next;
    private readonly Mock<IUserRepository> _userRepository;

    public UserContextMiddlewareTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _next = new Mock<RequestDelegate>();
    }

    [Fact]
    public async Task InvokeAsync_WhenAuthenticatedUserExists_SetsUserId()
    {
        // Arrange
        var existingUser = new UserModel
        {
            Id = 1,
            ExternalId = "auth0|123456",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _userRepository
            .Setup(x => x.RetrieveByExternalIdAsync(existingUser.ExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, existingUser.ExternalId),
                new Claim(ClaimTypes.Name, existingUser.DisplayName)
            ], "TestAuth"))
        };

        var middleware = new UserContextMiddleware(_next.Object);

        // Act
        await middleware.InvokeAsync(context, _userRepository.Object);

        // Assert
        _userRepository.Verify(x =>
            x.RetrieveByExternalIdAsync(existingUser.ExternalId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x =>
            x.CreateAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Never);
        _next.Verify(x => x(context), Times.Once);
        context.Items.Should().ContainKey("UserId");
        context.Items["UserId"].Should().Be(existingUser.Id);
    }

    [Fact]
    public async Task InvokeAsync_WhenAuthenticatedUserDoesNotExist_CreatesUserAndSetsUserId()
    {
        // Arrange
        var newUser = new UserModel
        {
            Id = 1,
            ExternalId = "auth0|789012",
            DisplayName = "New User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _userRepository
            .Setup(x => x.RetrieveByExternalIdAsync(newUser.ExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);

        _userRepository
            .Setup(x => x.CreateAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUser);

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, newUser.ExternalId),
                new Claim(ClaimTypes.Name, newUser.DisplayName)
            ], "TestAuth"))
        };

        var middleware = new UserContextMiddleware(_next.Object);

        // Act
        await middleware.InvokeAsync(context, _userRepository.Object);

        // Assert
        _userRepository.Verify(x =>
            x.RetrieveByExternalIdAsync(newUser.ExternalId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x =>
            x.CreateAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Once);
        _next.Verify(x => x(context), Times.Once);
        context.Items.Should().ContainKey("UserId");
        context.Items["UserId"].Should().Be(newUser.Id);
    }
}