namespace AuthService.Common.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class GuardAuthenticationService(IOptions<UserGuardOptions> options, IUserGuardAuthStore userGuardAuthStore)
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public async Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context)
    {
        var userGuardOptions = options.Value;
        var token = GetBearerToken(context.Request.Headers.Authorization);
        if (string.IsNullOrWhiteSpace(token))
            return null;

        ClaimsPrincipal principal;
        try
        {
            principal = _tokenHandler.ValidateToken(token, BuildValidationParameters(userGuardOptions), out _);
        }
        catch
        {
            return null;
        }

        var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var hasMatchingToken = await userGuardAuthStore.HasMatchingTokenAsync(userId, token, context.RequestAborted);
        if (!hasMatchingToken)
            return null;

        return principal;
    }

    public static bool ShouldGuardEndpoint<TGuardAttribute>(Endpoint? endpoint)
        where TGuardAttribute : Attribute
    {
        if (endpoint == null)
            return false;

        if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null)
            return false;

        return endpoint.Metadata.GetMetadata<TGuardAttribute>() != null;
    }

    public static bool HasAnyRequiredRole(ClaimsPrincipal principal, IReadOnlyCollection<string> requiredRoles)
    {
        foreach (var requiredRole in requiredRoles)
        {
            if (string.IsNullOrWhiteSpace(requiredRole))
                continue;

            foreach (var claim in principal.Claims)
            {
                if (claim.Type is not ClaimTypes.Role and not "role")
                    continue;

                if (string.Equals(claim.Value, requiredRole, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }

    private static TokenValidationParameters BuildValidationParameters(UserGuardOptions userGuardOptions)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = userGuardOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = userGuardOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(userGuardOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    private static string GetBearerToken(string? authorizationHeader)
    {
        const string bearerPrefix = "Bearer ";
        if (string.IsNullOrWhiteSpace(authorizationHeader))
            return string.Empty;

        if (authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            return authorizationHeader[bearerPrefix.Length..].Trim();

        return string.Empty;
    }
}
