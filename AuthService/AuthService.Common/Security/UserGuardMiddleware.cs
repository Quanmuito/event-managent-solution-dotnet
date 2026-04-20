namespace AuthService.Common.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class UserGuardMiddleware(IOptions<UserGuardOptions> options, IUserGuardAuthStore userGuardAuthStore) : IMiddleware
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();
        if (!ShouldGuardEndpoint(endpoint))
        {
            await next(context);
            return;
        }

        var userGuardOptions = options.Value;
        var token = GetBearerToken(context.Request.Headers.Authorization);
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        ClaimsPrincipal principal;
        try
        {
            principal = _tokenHandler.ValidateToken(token, BuildValidationParameters(userGuardOptions), out _);
        }
        catch
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var hasMatchingToken = await userGuardAuthStore.HasMatchingTokenAsync(userId, token, context.RequestAborted);
        if (!hasMatchingToken)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        context.User = principal;
        await next(context);
    }

    private static bool ShouldGuardEndpoint(Endpoint? endpoint)
    {
        if (endpoint == null)
            return false;

        if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null)
            return false;

        return endpoint.Metadata.GetMetadata<UserGuardAttribute>() != null;
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
