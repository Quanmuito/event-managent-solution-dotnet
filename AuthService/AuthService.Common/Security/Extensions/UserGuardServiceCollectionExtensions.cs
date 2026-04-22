namespace AuthService.Common.Security.Extensions;

using System.Text;
using AuthService.Common.Security;
using AuthService.Common.Security.Middlewares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class UserGuardServiceCollectionExtensions
{
    public static IServiceCollection AddUserGuard(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization();
        services.Configure<UserGuardOptions>(configuration.GetSection(UserGuardOptions.SectionName));
        services.AddSingleton<IValidateOptions<UserGuardOptions>, UserGuardOptionsValidator>();
        services.AddTransient<GuardAuthenticationService>();
        services.AddTransient<UserGuardMiddleware>();
        services.AddTransient<OrganizerGuardMiddleware>();
        return services;
    }
}

public class UserGuardOptionsValidator : IValidateOptions<UserGuardOptions>
{
    public ValidateOptionsResult Validate(string? name, UserGuardOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Secret))
            return ValidateOptionsResult.Fail("Jwt secret is required.");

        if (Encoding.UTF8.GetByteCount(options.Secret) < 32)
            return ValidateOptionsResult.Fail("Jwt secret must be at least 32 bytes.");

        if (string.IsNullOrWhiteSpace(options.Issuer))
            return ValidateOptionsResult.Fail("Jwt issuer is required.");

        if (string.IsNullOrWhiteSpace(options.Audience))
            return ValidateOptionsResult.Fail("Jwt audience is required.");

        return ValidateOptionsResult.Success;
    }
}
