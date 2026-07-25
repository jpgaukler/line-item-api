using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LineItem.Api;
using LineItem.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LineItem.Test.Integration.Setup;

/// <summary>
///     Shared fixture for integration tests. Allows for use of WebApplicationFactory to spin up a web server when testing
///     locally, or the use of a live API server by passing in Environment Variables.
/// </summary>
public class ApiFixture : IAsyncLifetime
{
    /// <summary>
    ///     Set the AUTHO_TEST_TOKEN environment variable to run tests against a live API server.
    /// </summary>
    private readonly string? authToken = Environment.GetEnvironmentVariable("AUTH0_TEST_TOKEN");

    /// <summary>
    ///     Set the API_SERVER_URL environment variable to run tests against a live API server.
    /// </summary>
    private readonly string? serverUrl = Environment.GetEnvironmentVariable("API_SERVER_URL");

    private WebApplicationFactory<Program>? _webApplicationFactory;

    public long TestUserId { get; private set; }
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        if (UseWebApplicationFactory())
        {
            var token = TestTokenGenerator.Generate($"auth0|test-{Guid.NewGuid()}");

            // spin up in-process test server with test user via mocked JWT  
            _webApplicationFactory = new LineItemWebApplicationFactory();
            Client = _webApplicationFactory.CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(authToken))
                throw new InvalidOperationException("AUTH0_TEST_TOKEN must be set when using API_SERVER_URL.");

            // run tests against a live API server with test user via real JWT 
            Client = new HttpClient { BaseAddress = new Uri(serverUrl!) };
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        await InitializeTestUserAsync();
    }

    public async Task DisposeAsync()
    {
        await CleanupTestUserAsync();
        Client.Dispose();

        if (_webApplicationFactory is not null)
            await _webApplicationFactory.DisposeAsync();
    }

    private bool UseWebApplicationFactory()
    {
        return string.IsNullOrWhiteSpace(serverUrl);
    }

    private async Task InitializeTestUserAsync()
    {
        // hit /users/me to trigger user creation via middleware
        var response = await Client.GetAsync("v1/users/me");
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<UserModel>();
        TestUserId = user!.Id;
    }

    private async Task CleanupTestUserAsync()
    {
        var response = await Client.DeleteAsync($"v1/users/{TestUserId}");
        response.EnsureSuccessStatusCode();
    }
}