namespace AuthService.Api.Models;

using System.ComponentModel.DataAnnotations;

public class LoginDto
{
    [Required]
    [EmailAddress]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public required string Email { get; set; }

    [Required]
    [StringLength(500, ErrorMessage = "Password hash cannot exceed 500 characters.")]
    public required string PasswordHash { get; set; }
}
