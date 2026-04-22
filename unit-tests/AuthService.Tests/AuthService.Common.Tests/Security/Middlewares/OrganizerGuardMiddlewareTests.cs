namespace AuthService.Common.Tests.Security.Middlewares;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Common.Security;
using AuthService.Common.Security.Attributes;
using AuthService.Common.Security.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

public class OrganizerGuardMiddlewareTests
{
    private const string Secret = "replace-with-at-least-32-characters-secret-key";
    private const string Issuer = "AuthService";
    private const string Audience = "EmsClients";
    private const string UserId = "507f1f77bcf86cd799439011";

    [Fact]
    public async Task InvokeAsync_WhenEndpointIsNotGuarded_ShouldBypassValidation()
    {
        var middleware = CreateMiddleware(new Mock<IUserGuardAuthStore>().Object);
        var context = new DefaultHttpContext();
        SetEndpointMetadata(context);
        var nextCalled = false;

        await middleware.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithMissingAuthorizationHeader_ShouldReturnUnauthorized()
    {
        var middleware = CreateMiddleware(new Mock<IUserGuardAuthStore>().Object);
        var context = new DefaultHttpContext();
        SetEndpointMetadata(context, new OrganizerGuardAttribute());
        var nextCalled = false;

        await middleware.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithAuthenticatedNonOrganizer_ShouldReturnForbidden()
    {
        var token = CreateToken(Secret, DateTime.UtcNow.AddMinutes(5), ["user"]);
        var store = new Mock<IUserGuardAuthStore>();
        store.Setup(x => x.HasMatchingTokenAsync(UserId, token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var middleware = CreateMiddleware(store.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {token}";
        SetEndpointMetadata(context, new OrganizerGuardAttribute());
        var nextCalled = false;

        await middleware.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithAuthenticatedOrganizer_ShouldSetUserAndContinue()
    {
        var token = CreateToken(Secret, DateTime.UtcNow.AddMinutes(5), ["user", "organizer"]);
        var store = new Mock<IUserGuardAuthStore>();
        store.Setup(x => x.HasMatchingTokenAsync(UserId, token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var middleware = CreateMiddleware(store.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {token}";
        SetEndpointMetadata(context, new OrganizerGuardAttribute());
        var nextCalled = false;

        await middleware.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        nextCalled.Should().BeTrue();
        var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        userId.Should().Be(UserId);
    }

    private static OrganizerGuardMiddleware CreateMiddleware(IUserGuardAuthStore userGuardAuthStore)
    {
        var options = Options.Create(new UserGuardOptions
        {
            Secret = Secret,
            Issuer = Issuer,
            Audience = Audience
        });
        var guardAuthenticationService = new GuardAuthenticationService(options, userGuardAuthStore);
        return new OrganizerGuardMiddleware(guardAuthenticationService);
    }

    private static void SetEndpointMetadata(DefaultHttpContext context, params object[] metadata)
    {
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(metadata),
            "test-endpoint"
        );
        context.SetEndpoint(endpoint);
    }

    private static string CreateToken(string secret, DateTime expiresAtUtc, string[] roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, UserId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            SecurityAlgorithms.HmacSha256
        );
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
