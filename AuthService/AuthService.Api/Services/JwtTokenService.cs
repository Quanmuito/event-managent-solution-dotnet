namespace AuthService.Api.Services;

using AuthService.Api.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenService(IOptions<JwtSettings> jwtOptions) : IJwtTokenService
{
    public string GenerateToken(string userId, string email, string[] roles)
    {
        var jwtSettings = jwtOptions.Value;
        ValidateJwtSettings(jwtSettings);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        foreach (var role in roles)
        {
            if (string.IsNullOrWhiteSpace(role))
                continue;

            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void ValidateJwtSettings(JwtSettings jwtSettings)
    {
        if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
            throw new InvalidOperationException("Jwt secret is required.");

        if (jwtSettings.Secret.Length < 32)
            throw new InvalidOperationException("Jwt secret must be at least 32 characters.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
            throw new InvalidOperationException("Jwt issuer is required.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
            throw new InvalidOperationException("Jwt audience is required.");

        if (jwtSettings.ExpiryMinutes <= 0)
            throw new InvalidOperationException("Jwt expiry minutes must be greater than zero.");
    }
}
