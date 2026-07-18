using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LineItem.Test.Integration.Setup;

/// <summary>
///     A custom authentication handler used in integration tests to bypass external authentication providers
///     and simulate authentication with a test user.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string SCHEME_NAME = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    ) : base(options, logger, encoder)
    {
    }

    public static string GetSchemeName()
    {
        return SCHEME_NAME;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Context.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            return Task.FromResult(AuthenticateResult.NoResult());

        var token = authHeader["Bearer ".Length..];

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = TestTokenGenerator.GetIssuer(),
                ValidateAudience = true,
                ValidAudience = TestTokenGenerator.GetAudience(),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = TestTokenGenerator.GetSecurityKey(),
                ValidateLifetime = true
            };

            var principal = handler.ValidateToken(token, validationParameters, out _);
            var ticket = new AuthenticationTicket(principal, SCHEME_NAME);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid test token."));
        }
    }
}