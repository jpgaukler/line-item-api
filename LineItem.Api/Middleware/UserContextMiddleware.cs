using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace LineItem.Api.Middleware;

public class UserContextMiddleware
{
    private readonly IMemoryCache _cache;
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var externalId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(externalId))
            {
                var cacheKey = $"{nameof(UserContextMiddleware)}|ExternalId|{externalId}";

                var userId = await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromHours(1);
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4);

                    var user = await userRepository.RetrieveByExternalIdAsync(externalId, context.RequestAborted);

                    if (user is null)
                    {
                        var newUser = new UserModel
                        {
                            ExternalId = externalId,
                            DisplayName = context.User.FindFirst(ClaimTypes.Email)?.Value ?? ""
                        };
                        user = await userRepository.CreateAsync(newUser, context.RequestAborted);
                    }

                    return user.Id;
                });

                context.Items["UserId"] = userId;
            }
        }

        await _next(context);
    }
}