namespace LineItem.Api.Middleware;

// public class UserContextMiddleware
// {
//     private readonly RequestDelegate _next;
//
//     public UserContextMiddleware(RequestDelegate next)
//     {
//         _next = next;
//     }
//
//     public async Task InvokeAsync(HttpContext context, UserRepository userRepository)
//     {
//         if (context.User.Identity?.IsAuthenticated == true)
//         {
//             var auth0Id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//
//             if (!string.IsNullOrEmpty(auth0Id))
//             {
//                 // 2. Check if the user already exists in your DB
//                 var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);
//
//                 if (user == null)
//                 {
//                     // 3. Optional: Grab email or name claims from the token to populate the new record
//                     var email = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "";
//
//                     user = new UserModel { ExternalId = auth0Id, DisplayName = email };
//
//                     // 4. Save the new user record
//                     dbContext.Users.Add(user);
//                     await dbContext.SaveChangesAsync();
//                 }
//
//                 // 5. Attach the internal Database Primary Key to HttpContext Items
//                 // This makes it easy to grab the integer/guid ID in your controllers
//                 context.Items["InternalUserId"] = user.Id;
//             }
//         }
//
//         await _next(context);
//     }
// }