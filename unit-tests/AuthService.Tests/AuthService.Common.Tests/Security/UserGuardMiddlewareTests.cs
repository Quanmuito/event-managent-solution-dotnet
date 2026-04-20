namespace AuthService.Common.Tests.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Common.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

public class UserGuardMiddlewareTests
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
        context.Request.Path = "/v1/events";
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
    public async Task InvokeAsync_WhenEndpointHasAllowAnonymous_ShouldBypassValidation()
    {
        var middleware = CreateMiddleware(new Mock<IUserGuardAuthStore>().Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/auths/login";
        SetEndpointMetadata(context, new UserGuardAttribute(), new AllowAnonymousAttribute());
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
        context.Request.Path = "/v1/events";
        SetEndpointMetadata(context, new UserGuardAttribute());
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
    public async Task InvokeAsync_WithMalformedBearerToken_ShouldReturnUnauthorized()
    {
        var middleware = CreateMiddleware(new Mock<IUserGuardAuthStore>().Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/events";
        context.Request.Headers.Authorization = "Bearer not-a-jwt-token";
        SetEndpointMetadata(context, new UserGuardAttribute());
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
    public async Task InvokeAsync_WithExpiredToken_ShouldReturnUnauthorized()
    {
        var token = CreateToken(Secret, DateTime.UtcNow.AddMinutes(-1));
        var middleware = CreateMiddleware(new Mock<IUserGuardAuthStore>().Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/events";
        context.Request.Headers.Authorization = $"Bearer {token}";
        SetEndpointMetadata(context, new UserGuardAttribute());
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
    public async Task InvokeAsync_WithWrongSignature_ShouldReturnUnauthorized()
    {
        var token = CreateToken("another-secret-key-with-32-characters", DateTime.UtcNow.AddMinutes(5));
        var middleware = CreateMiddleware(new Mock<IUserGuardAuthStore>().Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/events";
        context.Request.Headers.Authorization = $"Bearer {token}";
        SetEndpointMetadata(context, new UserGuardAttribute());
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
    public async Task InvokeAsync_WithTokenNotMatchingStore_ShouldReturnUnauthorized()
    {
        var token = CreateToken(Secret, DateTime.UtcNow.AddMinutes(5));
        var store = new Mock<IUserGuardAuthStore>();
        store.Setup(x => x.HasMatchingTokenAsync(UserId, token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var middleware = CreateMiddleware(store.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/events";
        context.Request.Headers.Authorization = $"Bearer {token}";
        SetEndpointMetadata(context, new UserGuardAttribute());
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
    public async Task InvokeAsync_WithValidTokenAndStoreMatch_ShouldSetUserAndContinue()
    {
        var token = CreateToken(Secret, DateTime.UtcNow.AddMinutes(5));
        var store = new Mock<IUserGuardAuthStore>();
        store.Setup(x => x.HasMatchingTokenAsync(UserId, token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var middleware = CreateMiddleware(store.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/events";
        context.Request.Headers.Authorization = $"Bearer {token}";
        SetEndpointMetadata(context, new UserGuardAttribute());
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

    private static UserGuardMiddleware CreateMiddleware(IUserGuardAuthStore userGuardAuthStore)
    {
        var options = Options.Create(new UserGuardOptions
        {
            Secret = Secret,
            Issuer = Issuer,
            Audience = Audience
        });
        return new UserGuardMiddleware(options, userGuardAuthStore);
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

    private static string CreateToken(string secret, DateTime expiresAtUtc)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, UserId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

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
