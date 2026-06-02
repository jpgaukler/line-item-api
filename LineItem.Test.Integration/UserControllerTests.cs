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
            BaseAddress = new Uri("https://localhost:7165/")
        };
    }

    [Fact]
    public async Task UserModel_CRUDWorkflow_ShouldComplete()
    {
        // CREATE
        var newUser = new UserModel
        {
            ExternalId = $"auth0|test-{Guid.NewGuid()}",
            DisplayName = "CRUD Test"
        };

        var response = await _client.PostAsJsonAsync("api/v1/users", newUser);
        var user = await response.Content.ReadFromJsonAsync<UserModel>();

        _output.WriteLine($"CREATE - User created:\n{JsonSerializer.Serialize(user, JsonOptions)}");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.ToString().Should().Contain("/api/v1/users/");
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

        response = await _client.PutAsJsonAsync($"api/v1/users/{userId}", updatedUser);

        _output.WriteLine($"UPDATE - User updated with:\n{JsonSerializer.Serialize(updatedUser, JsonOptions)}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // RETRIEVE (verify update)
        response = await _client.GetAsync($"api/v1/users/{userId}");
        user = await response.Content.ReadFromJsonAsync<UserModel>();

        _output.WriteLine($"RETRIEVE (after update) - User retrieved:\n{JsonSerializer.Serialize(user, JsonOptions)}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        user.Should().NotBeNull();
        user.Id.Should().Be(userId);
        user.ExternalId.Should().Be(updatedUser.ExternalId);
        user.DisplayName.Should().Be(updatedUser.DisplayName);
        user.CreatedAt.Should().Be(createdAt);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // DELETE
        response = await _client.DeleteAsync($"api/v1/users/{userId}");

        _output.WriteLine($"DELETE - User deleted: UserId={userId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // RETRIEVE (verify deletion)
        response = await _client.GetAsync($"api/v1/users/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}