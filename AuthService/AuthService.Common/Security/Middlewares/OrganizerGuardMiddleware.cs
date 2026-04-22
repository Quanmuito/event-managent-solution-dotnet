namespace AuthService.Common.Security.Middlewares;

using AuthService.Common.Security.Attributes;
using AuthService.Common.Security;
using Microsoft.AspNetCore.Http;

public class OrganizerGuardMiddleware(GuardAuthenticationService guardAuthenticationService) : IMiddleware
{
    private static readonly string[] s_requiredRoles = ["organizer"];

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();
        if (!GuardAuthenticationService.ShouldGuardEndpoint<OrganizerGuardAttribute>(endpoint))
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

        if (!GuardAuthenticationService.HasAnyRequiredRole(principal, s_requiredRoles))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        context.User = principal;
        await next(context);
    }
}
