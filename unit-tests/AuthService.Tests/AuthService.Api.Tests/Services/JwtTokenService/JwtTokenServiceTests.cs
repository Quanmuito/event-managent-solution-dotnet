namespace AuthService.Api.Tests.Services.JwtTokenService;

using System.IdentityModel.Tokens.Jwt;
using AuthService.Api.Services;
using AuthService.Api.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

public class JwtTokenServiceTests
{
    [Fact]
    public void GenerateToken_WithValidSettings_ShouldContainSubAndEmailClaims()
    {
        var service = CreateService(CreateValidSettings());

        var token = service.GenerateToken("user-123", "user@example.com");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be("user-123");
        jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().Be("user@example.com");
        jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_WithValidSettings_ShouldUseConfiguredIssuerAudienceAndExpiry()
    {
        var service = CreateService(CreateValidSettings());

        var before = DateTime.UtcNow;
        var token = service.GenerateToken("user-123", "user@example.com");
        var after = DateTime.UtcNow;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Issuer.Should().Be("AuthService");
        jwt.Audiences.Should().ContainSingle().Which.Should().Be("EmsClients");
        jwt.ValidTo.Should().BeAfter(before.AddMinutes(14));
        jwt.ValidTo.Should().BeBefore(after.AddMinutes(16));
    }

    [Fact]
    public void GenerateToken_WithShortSecret_ShouldThrowInvalidOperationException()
    {
        var settings = CreateValidSettings();
        settings.Secret = "short-secret";
        var service = CreateService(settings);

        var act = () => service.GenerateToken("user-123", "user@example.com");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Jwt secret must be at least 32 characters.");
    }

    private static AuthService.Api.Services.JwtTokenService CreateService(JwtSettings settings)
    {
        return new AuthService.Api.Services.JwtTokenService(Options.Create(settings));
    }

    private static JwtSettings CreateValidSettings()
    {
        return new JwtSettings
        {
            Secret = "replace-with-at-least-32-characters-secret-key",
            Issuer = "AuthService",
            Audience = "EmsClients",
            ExpiryMinutes = 15
        };
    }
}
