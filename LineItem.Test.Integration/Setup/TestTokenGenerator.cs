using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LineItem.Test.Integration.Setup;

/// <summary>
///     A custom token generator for generating mock JWT tokens when running integration tests locally.
///     Works in tandem with the TestAuthHandler.
/// </summary>
public static class TestTokenGenerator
{
    private const string SIGNING_KEY = "secret-key-for-integration-tests";
    private const string ISSUER = "test";
    private const string AUDIENCE = "test";

    public static string GetIssuer()
    {
        return ISSUER;
    }

    public static string GetAudience()
    {
        return AUDIENCE;
    }

    public static SymmetricSecurityKey GetSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SIGNING_KEY));
    }

    public static string Generate(string externalId, string displayName = "Integration Test User")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SIGNING_KEY));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, externalId),
            new Claim("https://line-item.app/name", displayName)
        };

        var token = new JwtSecurityToken(
            ISSUER,
            AUDIENCE,
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}