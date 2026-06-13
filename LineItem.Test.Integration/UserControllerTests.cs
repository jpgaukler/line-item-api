using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LineItem.Models;
using Xunit;
using Xunit.Abstractions;

namespace LineItem.Test.Integration;

public class UserControllerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public UserControllerTests(ITestOutputHelper output)
    {
        _output = output;
        _client = new HttpClient
        {
            // BaseAddress = new Uri("https://localhost:7165/")
            BaseAddress = new Uri("https://dev.api.line-item.app")
        };
    }

    [Fact]
    public async Task UserCRUD_WithValidUser_IsSuccessful()
    {
        // CREATE
        var newUser = new UserModel
        {
            ExternalId = $"auth0|test-{Guid.NewGuid()}",
            DisplayName = "CRUD Test"
        };

        var response = await _client.PostAsJsonAsync("v1/users", newUser);
        var user = await response.Content.ReadFromJsonAsync<UserModel>();

        _output.WriteLine($"CREATE - User created:\n{JsonSerializer.Serialize(user, JsonOptions)}");

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
        response = await _client.GetAsync(response.Headers.Location);
        user = await response.Content.ReadFromJsonAsync<UserModel>();

        _output.WriteLine($"RETRIEVE - User retrieved:\n{JsonSerializer.Serialize(user, JsonOptions)}");

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

        response = await _client.PutAsJsonAsync($"v1/users/{userId}", updatedUser);
        user = await response.Content.ReadFromJsonAsync<UserModel>();

        _output.WriteLine($"UPDATE - User updated:\n{JsonSerializer.Serialize(user, JsonOptions)}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        user.Should().NotBeNull();
        user.Id.Should().Be(userId);
        user.ExternalId.Should().Be(updatedUser.ExternalId);
        user.DisplayName.Should().Be(updatedUser.DisplayName);
        user.CreatedAt.Should().Be(createdAt);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // DELETE
        response = await _client.DeleteAsync($"v1/users/{userId}");

        _output.WriteLine($"DELETE - User deleted: UserId={userId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // RETRIEVE (verify deletion)
        response = await _client.GetAsync($"v1/users/{userId}");

        _output.WriteLine($"RETRIEVE (verify deletion) - Response: {response.StatusCode}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task CreateUser_WithDuplicateExternalId_ReturnsBadRequest()
    {
        // CREATE first user
        var newUser = new UserModel
        {
            ExternalId = $"auth0|test-duplicate-{Guid.NewGuid()}",
            DisplayName = "Duplicate Test"
        };

        var response = await _client.PostAsJsonAsync("v1/users", newUser);
        var user = await response.Content.ReadFromJsonAsync<UserModel>();

        _output.WriteLine($"CREATE - First user created:\n{JsonSerializer.Serialize(user, JsonOptions)}");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        user.Should().NotBeNull();
        user.Id.Should().BeGreaterThan(0);

        // CREATE second user with same ExternalId
        var duplicateUser = new UserModel
        {
            ExternalId = newUser.ExternalId,
            DisplayName = "Duplicate Test 2"
        };

        response = await _client.PostAsJsonAsync("v1/users", duplicateUser);

        _output.WriteLine($"CREATE - Attempted duplicate user creation with ExternalId: {duplicateUser.ExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Cleanup
        await _client.DeleteAsync($"v1/users/{user.Id}");
    }

    [Fact]
    public async Task UpdateUser_WithDuplicateExternalId_ReturnsBadRequest()
    {
        // CREATE first user
        var firstUser = new UserModel
        {
            ExternalId = $"auth0|test-first-{Guid.NewGuid()}",
            DisplayName = "First User"
        };

        var response = await _client.PostAsJsonAsync("v1/users", firstUser);
        var user1 = await response.Content.ReadFromJsonAsync<UserModel>();

        _output.WriteLine($"CREATE - First user created:\n{JsonSerializer.Serialize(user1, JsonOptions)}");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        user1.Should().NotBeNull();
        user1.Id.Should().BeGreaterThan(0);

        // CREATE second user
        var secondUser = new UserModel
        {
            ExternalId = $"auth0|test-second-{Guid.NewGuid()}",
            DisplayName = "Second User"
        };

        response = await _client.PostAsJsonAsync("v1/users", secondUser);
        var user2 = await response.Content.ReadFromJsonAsync<UserModel>();

        _output.WriteLine($"CREATE - Second user created:\n{JsonSerializer.Serialize(user2, JsonOptions)}");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        user2.Should().NotBeNull();
        user2.Id.Should().BeGreaterThan(0);

        // UPDATE second user with first user's ExternalId
        var updateUser = new UserModel
        {
            ExternalId = user1.ExternalId,
            DisplayName = "Updated Second User"
        };

        response = await _client.PutAsJsonAsync($"v1/users/{user2.Id}", updateUser);

        _output.WriteLine(
            $"UPDATE - Attempted to update user {user2.Id} with duplicate ExternalId: {updateUser.ExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Cleanup
        await _client.DeleteAsync($"v1/users/{user1.Id}");
        await _client.DeleteAsync($"v1/users/{user2.Id}");
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        const int invalidUserId = 0;

        var response = await _client.GetAsync($"v1/users/{invalidUserId}");

        _output.WriteLine($"GET - Attempted to retrieve user with invalid ID: {invalidUserId}");

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

        var response = await _client.PutAsJsonAsync($"v1/users/{invalidUserId}", updateUser);

        _output.WriteLine($"UPDATE - Attempted to update user with invalid ID: {invalidUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}