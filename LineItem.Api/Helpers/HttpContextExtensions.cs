using System;
using Microsoft.AspNetCore.Http;

namespace LineItem.Api.Helpers;

public static class HttpContextExtensions
{
    public static long GetUserId(this HttpContext context)
    {
        return context.Items["UserId"] is long userId
            ? userId
            : throw new InvalidOperationException("User context is not available.");
    }
}