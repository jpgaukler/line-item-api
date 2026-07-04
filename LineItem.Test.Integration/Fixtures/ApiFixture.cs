using System;
using System.Net.Http;
using LineItem.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LineItem.Test.Integration.Fixtures;

/// <summary>
///     Shared fixture for integration tests. Spins up a web server for the API, so you do not have to run the API locally
///     to run the tests.
/// </summary>
public class ApiFixture : WebApplicationFactory<Program>
{
    public ApiFixture()
    {
        // Set the API_SERVER_URL environment variable to run tests against a live API server.
        var serverUrl = Environment.GetEnvironmentVariable("API_SERVER_URL");

        Client = string.IsNullOrWhiteSpace(serverUrl)
            ? CreateClient() // use WebApplicationFactory to create a test server and client
            : new HttpClient { BaseAddress = new Uri(serverUrl) }; // run tests against a live API
    }

    public HttpClient Client { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("local");
    }
}

/// <summary>
///     Registers a collection of tests that share the ApiFixture.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<ApiFixture>
{
}