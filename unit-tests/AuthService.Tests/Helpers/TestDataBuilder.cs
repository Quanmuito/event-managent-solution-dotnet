namespace AuthService.Tests.Helpers;

using AuthService.Api.Models;
using AuthService.Data.Models;

public static class TestDataBuilder
{
    public static CreateUserDto CreateValidCreateUserDto()
    {
        return new CreateUserDto
        {
            Email = "test@example.com",
            Phone = "+1234567890",
            IsVerified = false
        };
    }

    public static UpdateUserDto CreateValidUpdateUserDto()
    {
        return new UpdateUserDto
        {
            Email = "updated@example.com",
            Phone = "+9876543210",
            IsVerified = true
        };
    }

    public static User CreateUser(string? id = null, string? email = "test@example.com", string? phone = "+1234567890")
    {
        return new User
        {
            Id = id ?? "507f1f77bcf86cd799439011",
            Email = email,
            Phone = phone,
            IsVerified = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    public static List<User> CreateUserList(int count = 3)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            users.Add(CreateUser($"507f1f77bcf86cd79943901{i}", $"user{i}@example.com", $"+1234567890{i}"));
        }
        return users;
    }

    public static CreateAuthDto CreateValidCreateAuthDto(string? userId = null)
    {
        return new CreateAuthDto
        {
            UserId = userId ?? "507f1f77bcf86cd799439011",
            Token = "test-token-12345"
        };
    }

    public static UpdateAuthDto CreateValidUpdateAuthDto()
    {
        return new UpdateAuthDto
        {
            Token = "updated-token-67890"
        };
    }

    public static Auth CreateAuth(string? id = null, string? userId = null, string? token = "test-token-12345")
    {
        return new Auth
        {
            Id = id ?? "507f1f77bcf86cd799439011",
            UserId = userId ?? "507f1f77bcf86cd799439011",
            Token = token,
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
