namespace UserService.Api.Models;

using UserService.Data.Models;

public class UserDto(User user)
{
    public string Id { get; set; } = user.Id!;
    public string Email { get; set; } = user.Email;
    public string? Phone { get; set; } = user.Phone;
    public string[] Roles { get; set; } = user.Roles;
    public bool IsVerified { get; set; } = user.IsVerified;
    public bool IsDeleted { get; set; } = user.IsDeleted;
    public DateTime? DeletedAt { get; set; } = user.DeletedAt;
    public string? DeletedBy { get; set; } = user.DeletedBy;
    public string? DeletedReason { get; set; } = user.DeletedReason;
    public DateTime CreatedAt { get; set; } = user.CreatedAt;
    public DateTime? UpdatedAt { get; set; } = user.UpdatedAt;
}
