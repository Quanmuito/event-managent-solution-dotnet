namespace AuthService.Common.Security.Middlewares;

using AuthService.Common.Security.Attributes;
using AuthService.Common.Security;
using Microsoft.AspNetCore.Http;

public class UserGuardMiddleware(GuardAuthenticationService guardAuthenticationService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();
        if (!GuardAuthenticationService.ShouldGuardEndpoint<UserGuardAttribute>(endpoint))
        {
            await next(context);
            return;
        }

        var principal = await guardAuthenticationService.AuthenticateAsync(context);
        if (principal == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        context.User = principal;
        await next(context);
    }
}
