namespace AuthService.Api.Tests.Models;

using AuthService.Api.Models;
using AuthService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class AuthDtoTests
{
    [Fact]
    public void Constructor_WithValidAuth_ShouldMapAllProperties()
    {
        var authEntity = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011");

        var result = new AuthDto(authEntity);

        result.Should().NotBeNull();
        result.Id.Should().Be(authEntity.Id);
        result.UserId.Should().Be(authEntity.UserId);
        result.Token.Should().Be(authEntity.Token);
        result.CreatedAt.Should().Be(authEntity.CreatedAt);
        result.UpdatedAt.Should().Be(authEntity.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithNullUpdatedAt_ShouldSetUpdatedAtToNull()
    {
        var authEntity = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011");
        authEntity.UpdatedAt = null;

        var result = new AuthDto(authEntity);

        result.UpdatedAt.Should().BeNull();
    }
}
