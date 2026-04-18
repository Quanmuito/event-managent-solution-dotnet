namespace AuthService.Api.Models;

using System.ComponentModel.DataAnnotations;

public class UpdateAuthDto
{
    [StringLength(500, ErrorMessage = "Password hash cannot exceed 500 characters.")]
    public string? PasswordHash { get; set; }

    [StringLength(500, ErrorMessage = "Token cannot exceed 500 characters.")]
    public string? Token { get; set; }
}
