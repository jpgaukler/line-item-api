using LineItem.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LineItem.Test.Integration.Setup;

/// <summary>
///     Custom web application factory for running integration tests locally. Overrides authentication setup to use a
///     custom authentication handler with mocked JWT tokens.
/// </summary>
public class LineItemWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("local");
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.GetSchemeName();
                    options.DefaultChallengeScheme = TestAuthHandler.GetSchemeName();
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.GetSchemeName(),
                    options => { });
        });
    }
}