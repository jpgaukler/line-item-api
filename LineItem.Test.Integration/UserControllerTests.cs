using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LineItem.Models;
using LineItem.Test.Integration.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace LineItem.Test.Integration;

[Collection("Integration")]
public class UserControllerTests : IntegrationTestBase
{
    public UserControllerTests(ApiFixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public async Task UserCRUD_WithValidUser_IsSuccessful()
    {
        UserModel? user = null;

        try
        {
            // CREATE
            var newUser = new UserModel
            {
                ExternalId = $"auth0|test-{Guid.NewGuid()}",
                DisplayName = "CRUD Test"
            };

            var response = await Client.PostAsJsonAsync("v1/users", newUser);
            user = await response.Content.ReadFromJsonAsync<UserModel>();
            LogResponse("CREATE", response.StatusCode, $"UserId={user!.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location.ToString().Should().Contain("v1/users/");
            user.Should().NotBeNull();
            user.Id.Should().BeGreaterThan(0);
            user.ExternalId.Should().Be(newUser.ExternalId);
            user.DisplayName.Should().Be(newUser.DisplayName);
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            user.UpdatedAt.Should().BeNull();

            // RETRIEVE
            var createdAt = user.CreatedAt;
            response = await Client.GetAsync(response.Headers.Location);
            user = await response.Content.ReadFromJsonAsync<UserModel>();
            LogResponse("RETRIEVE", response.StatusCode, $"UserId={user!.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            user.Should().NotBeNull();
            user.Id.Should().BeGreaterThan(0);
            user.ExternalId.Should().Be(newUser.ExternalId);
            user.DisplayName.Should().Be(newUser.DisplayName);
            user.CreatedAt.Should().Be(createdAt);
            user.UpdatedAt.Should().BeNull();

            // UPDATE
            var userId = user.Id;
            var updatedUser = new UserModel
            {
                ExternalId = $"auth0|test-updated-{Guid.NewGuid()}",
                DisplayName = "CRUD Test Updated"
            };

            response = await Client.PutAsJsonAsync($"v1/users/{userId}", updatedUser);
            user = await response.Content.ReadFromJsonAsync<UserModel>();
            LogResponse("UPDATE", response.StatusCode, $"UserId={userId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            user.Should().NotBeNull();
            user.Id.Should().Be(userId);
            user.ExternalId.Should().Be(updatedUser.ExternalId);
            user.DisplayName.Should().Be(updatedUser.DisplayName);
            user.CreatedAt.Should().Be(createdAt);
            user.UpdatedAt.Should().NotBeNull();
            user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // DELETE
            response = await Client.DeleteAsync($"v1/users/{userId}");
            LogResponse("DELETE", response.StatusCode, $"UserId={userId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            user = null;

            // RETRIEVE (verify deletion)
            response = await Client.GetAsync($"v1/users/{userId}");
            LogResponse("RETRIEVE (verify deletion)", response.StatusCode);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            if (user is not null)
                await Client.DeleteAsync($"v1/users/{user.Id}");
        }
    }

    [Fact]
    public async Task CreateUser_WithDuplicateExternalId_ReturnsBadRequest()
    {
        UserModel? user = null;

        try
        {
            // CREATE first user
            var newUser = new UserModel
            {
                ExternalId = $"auth0|test-duplicate-{Guid.NewGuid()}",
                DisplayName = "Duplicate Test"
            };

            var response = await Client.PostAsJsonAsync("v1/users", newUser);
            user = await response.Content.ReadFromJsonAsync<UserModel>();
            LogResponse("CREATE", response.StatusCode, $"UserId={user!.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            user.Should().NotBeNull();
            user.Id.Should().BeGreaterThan(0);

            // CREATE second user with same ExternalId
            var duplicateUser = new UserModel
            {
                ExternalId = newUser.ExternalId,
                DisplayName = "Duplicate Test 2"
            };

            response = await Client.PostAsJsonAsync("v1/users", duplicateUser);
            LogResponse("CREATE DUPLICATE", response.StatusCode);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            if (user is not null)
                await Client.DeleteAsync($"v1/users/{user.Id}");
        }
    }

    [Fact]
    public async Task UpdateUser_WithDuplicateExternalId_ReturnsBadRequest()
    {
        UserModel? user1 = null;
        UserModel? user2 = null;

        try
        {
            // CREATE first user
            var firstUser = new UserModel
            {
                ExternalId = $"auth0|test-first-{Guid.NewGuid()}",
                DisplayName = "First User"
            };

            var response = await Client.PostAsJsonAsync("v1/users", firstUser);
            user1 = await response.Content.ReadFromJsonAsync<UserModel>();
            LogResponse("CREATE USER 1", response.StatusCode, $"UserId={user1!.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            user1.Should().NotBeNull();
            user1.Id.Should().BeGreaterThan(0);

            // CREATE second user
            var secondUser = new UserModel
            {
                ExternalId = $"auth0|test-second-{Guid.NewGuid()}",
                DisplayName = "Second User"
            };

            response = await Client.PostAsJsonAsync("v1/users", secondUser);
            user2 = await response.Content.ReadFromJsonAsync<UserModel>();
            LogResponse("CREATE USER 2", response.StatusCode, $"UserId={user2!.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            user2.Should().NotBeNull();
            user2.Id.Should().BeGreaterThan(0);

            // UPDATE second user with first user's ExternalId
            var updateUser = new UserModel
            {
                ExternalId = user1.ExternalId,
                DisplayName = "Updated Second User"
            };

            response = await Client.PutAsJsonAsync($"v1/users/{user2.Id}", updateUser);
            LogResponse("UPDATE WITH DUPLICATE EXTERNAL ID", response.StatusCode, $"UserId={user2.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            if (user1 is not null)
                await Client.DeleteAsync($"v1/users/{user1.Id}");
            if (user2 is not null)
                await Client.DeleteAsync($"v1/users/{user2.Id}");
        }
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        const int invalidUserId = 0;
        var response = await Client.GetAsync($"v1/users/{invalidUserId}");
        LogResponse("GET WITH INVALID ID", response.StatusCode);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidId_ReturnsNotFound()
    {
        const int invalidUserId = 0;
        var updateUser = new UserModel
        {
            ExternalId = $"auth0|test-update-invalid-{Guid.NewGuid()}",
            DisplayName = "Update Invalid Test"
        };

        var response = await Client.PutAsJsonAsync($"v1/users/{invalidUserId}", updateUser);
        LogResponse("UPDATE WITH INVALID ID", response.StatusCode);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}