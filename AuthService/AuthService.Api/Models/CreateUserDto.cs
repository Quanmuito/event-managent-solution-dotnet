namespace AuthService.Api.Models;

using System.ComponentModel.DataAnnotations;

public class CreateUserDto
{
    [Required]
    [EmailAddress]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public required string Email { get; set; }

    [Phone]
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
    public string? Phone { get; set; }

    public bool IsVerified { get; set; } = false;
}
