namespace AuthService.Tests.Helpers;

using AuthService.Api.Models;
using AuthService.Data.Models;
using UserService.Data.Models;

public static class TestDataBuilder
{
    public static User CreateUser(string id, string email, string? phone)
    {
        return new User
        {
            Id = id,
            Email = email,
            Phone = phone,
            IsVerified = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    public static CreateAuthDto CreateValidCreateAuthDto(string? userId = null)
    {
        return new CreateAuthDto
        {
            UserId = userId ?? "507f1f77bcf86cd799439011",
            PasswordHash = "hashed-password-12345",
            Token = "test-token-12345"
        };
    }

    public static UpdateAuthDto CreateValidUpdateAuthDto()
    {
        return new UpdateAuthDto
        {
            PasswordHash = "updated-hashed-password-67890",
            Token = "updated-token-67890"
        };
    }

    public static Auth CreateAuth(string? id = null, string? userId = null, string? token = "test-token-12345")
    {
        return new Auth
        {
            Id = id ?? "507f1f77bcf86cd799439011",
            UserId = userId ?? "507f1f77bcf86cd799439011",
            PasswordHash = "hashed-password-12345",
            Token = token ?? "test-token-12345",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    public static List<Auth> CreateAuthList(int count = 3, string? userId = null)
    {
        var auths = new List<Auth>();
        var baseUserId = userId ?? "507f1f77bcf86cd799439011";
        for (int i = 0; i < count; i++)
        {
            auths.Add(CreateAuth($"507f1f77bcf86cd79943901{i}", baseUserId, $"token-{i}"));
        }
        return auths;
    }
}
