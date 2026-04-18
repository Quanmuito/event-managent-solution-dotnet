namespace AuthService.Api.Models;

using AuthService.Data.Models;

public class AuthDto(Auth auth)
{
    public string Id { get; set; } = auth.Id!;
    public string UserId { get; set; } = auth.UserId;
    public string PasswordHash { get; set; } = auth.PasswordHash;
    public string Token { get; set; } = auth.Token;
    public DateTime CreatedAt { get; set; } = auth.CreatedAt;
    public DateTime? UpdatedAt { get; set; } = auth.UpdatedAt;
}
