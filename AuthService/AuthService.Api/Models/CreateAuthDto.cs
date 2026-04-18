namespace AuthService.Api.Models;

using System.ComponentModel.DataAnnotations;

public class CreateAuthDto
{
    [Required]
    public required string UserId { get; set; }

    [Required]
    [StringLength(500, ErrorMessage = "Password hash cannot exceed 500 characters.")]
    public required string PasswordHash { get; set; }

    [Required]
    [StringLength(500, ErrorMessage = "Token cannot exceed 500 characters.")]
    public required string Token { get; set; }
}
