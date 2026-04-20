namespace AuthService.Common.Security;

using Microsoft.AspNetCore.Builder;

public static class UserGuardApplicationBuilderExtensions
{
    public static IApplicationBuilder UseUserGuard(this IApplicationBuilder app)
    {
        app.UseMiddleware<UserGuardMiddleware>();
        return app;
    }
}
