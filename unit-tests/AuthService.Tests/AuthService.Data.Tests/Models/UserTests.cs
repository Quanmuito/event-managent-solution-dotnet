namespace AuthService.Data.Tests.Models;

using AuthService.Api.Models;
using AuthService.Data.Models;
using AuthService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class UserTests
{
    private static User CreateUserFromDto(CreateUserDto dto)
    {
        return new User
        {
            Email = dto.Email,
            Phone = dto.Phone,
            IsVerified = dto.IsVerified,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    [Fact]
    public void GetEntityFromDto_ShouldMapAllPropertiesCorrectly()
    {
        var dto = TestDataBuilder.CreateValidCreateUserDto();
        var beforeCreation = DateTime.UtcNow;

        var result = CreateUserFromDto(dto);

        var afterCreation = DateTime.UtcNow;

        result.Should().NotBeNull();
        result.Email.Should().Be(dto.Email);
        result.Phone.Should().Be(dto.Phone);
        result.IsVerified.Should().Be(dto.IsVerified);
        result.IsDeleted.Should().BeFalse();
        result.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        result.CreatedAt.Should().BeOnOrBefore(afterCreation);
        result.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void GetEntityFromDto_ShouldHandleNullPhone()
    {
        var dto = TestDataBuilder.CreateValidCreateUserDto();
        dto.Phone = null;

        var result = CreateUserFromDto(dto);

        result.Should().NotBeNull();
        result.Phone.Should().BeNull();
    }

    [Fact]
    public void GetEntityFromDto_ShouldSetCreatedAtToUtcNow()
    {
        var dto = TestDataBuilder.CreateValidCreateUserDto();
        var beforeCreation = DateTime.UtcNow;

        var result = CreateUserFromDto(dto);

        var afterCreation = DateTime.UtcNow;

        result.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        result.CreatedAt.Should().BeOnOrBefore(afterCreation);
        result.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }
}
