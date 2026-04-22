namespace AuthService.Api.Services;

public interface IJwtTokenService
{
    string GenerateToken(string userId, string email, string[] roles);
}
