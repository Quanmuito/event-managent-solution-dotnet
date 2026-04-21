namespace UserService.Tests.Helpers;

using UserService.Api.Models;
using UserService.Data.Models;
using UserService.Data.Utils;

public static class TestDataBuilder
{
    public static CreateUserDto CreateValidCreateUserDto()
    {
        return new CreateUserDto
        {
            Email = "test@example.com",
            Phone = "+1234567890",
            Roles = [UserRoles.USER],
            IsVerified = false
        };
    }

    public static UpdateUserDto CreateValidUpdateUserDto()
    {
        return new UpdateUserDto
        {
            Email = "updated@example.com",
            Phone = "+9876543210",
            Roles = [UserRoles.USER, UserRoles.ORGANIZER],
            IsVerified = true
        };
    }

    public static User CreateUser(string id, string email, string? phone)
    {
        return new User
        {
            Id = id,
            Email = email,
            Phone = phone,
            Roles = [UserRoles.USER],
            IsVerified = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    public static User CreateDefaultUser()
    {
        return CreateUser("507f1f77bcf86cd799439011", "test@example.com", "+1234567890");
    }

    public static List<User> CreateUserList(int count)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            users.Add(CreateUser($"507f1f77bcf86cd79943901{i}", $"user{i}@example.com", $"+1234567890{i}"));
        }
        return users;
    }
}
