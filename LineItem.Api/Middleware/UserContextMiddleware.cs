using System.Security.Claims;
using System.Threading.Tasks;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LineItem.Api.Middleware;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var externalId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(externalId))
            {
                var user = await userRepository.RetrieveByExternalIdAsync(externalId, context.RequestAborted);

                if (user is null)
                {
                    var email = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "";

                    user = new UserModel
                    {
                        ExternalId = externalId,
                        DisplayName = email
                    };

                    user = await userRepository.CreateAsync(user, context.RequestAborted);
                }

                context.Items["UserId"] = user.Id;
            }
        }

        await _next(context);
    }
}