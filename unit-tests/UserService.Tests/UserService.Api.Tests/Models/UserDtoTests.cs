namespace UserService.Api.Tests.Models;

using FluentAssertions;
using UserService.Api.Models;
using UserService.Tests.Helpers;
using Xunit;

public class UserDtoTests
{
    [Fact]
    public void Constructor_WithValidUser_ShouldMapAllProperties()
    {
        var userEntity = TestDataBuilder.CreateDefaultUser();

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
        var userEntity = TestDataBuilder.CreateDefaultUser();
        userEntity.UpdatedAt = null;

        var result = new UserDto(userEntity);

        result.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullPhone_ShouldSetPhoneToNull()
    {
        var userEntity = TestDataBuilder.CreateDefaultUser();
        userEntity.Phone = null;

        var result = new UserDto(userEntity);

        result.Phone.Should().BeNull();
    }
}
