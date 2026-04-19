namespace AuthService.Api.Models;

public class RegisterResultDto
{
    public required string Message { get; set; }
    public required string Token { get; set; }
}
