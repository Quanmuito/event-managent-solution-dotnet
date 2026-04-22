namespace AuthService.Common.Security.Extensions;

using AuthService.Common.Security.Middlewares;
using Microsoft.AspNetCore.Builder;

public static class UserGuardApplicationBuilderExtensions
{
    public static IApplicationBuilder UseUserGuard(this IApplicationBuilder app)
    {
        app.UseMiddleware<UserGuardMiddleware>();
        app.UseMiddleware<OrganizerGuardMiddleware>();
        return app;
    }
}
