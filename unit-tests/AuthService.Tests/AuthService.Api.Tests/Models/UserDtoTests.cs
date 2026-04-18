namespace AuthService.Api.Tests.Models;

using AuthService.Api.Models;
using AuthService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class UserDtoTests
{
    [Fact]
    public void Constructor_WithValidUser_ShouldMapAllProperties()
    {
        var userEntity = TestDataBuilder.CreateUser("507f1f77bcf86cd799439011");

        var result = new UserDto(userEntity);

        result.Should().NotBeNull();
        result.Id.Should().Be(userEntity.Id);
        result.Email.Should().Be(userEntity.Email);
        result.Phone.Should().Be(userEntity.Phone);
        result.IsVerified.Should().Be(userEntity.IsVerified);
        result.IsDeleted.Should().Be(userEntity.IsDeleted);
        result.DeletedAt.Should().Be(userEntity.DeletedAt);
        result.DeletedBy.Should().Be(userEntity.DeletedBy);
        result.DeletedReason.Should().Be(userEntity.DeletedReason);
        result.CreatedAt.Should().Be(userEntity.CreatedAt);
        result.UpdatedAt.Should().Be(userEntity.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithNullUpdatedAt_ShouldSetUpdatedAtToNull()
    {
        var userEntity = TestDataBuilder.CreateUser("507f1f77bcf86cd799439011");
        userEntity.UpdatedAt = null;

        var result = new UserDto(userEntity);

        result.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullPhone_ShouldSetPhoneToNull()
    {
        var userEntity = TestDataBuilder.CreateUser("507f1f77bcf86cd799439011");
        userEntity.Phone = null;

        var result = new UserDto(userEntity);

        result.Phone.Should().BeNull();
    }
}
