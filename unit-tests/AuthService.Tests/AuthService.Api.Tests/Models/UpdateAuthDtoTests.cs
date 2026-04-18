namespace AuthService.Api.Tests.Models;

using AuthService.Api.Models;
using FluentAssertions;
using Xunit;

public class UpdateAuthDtoTests
{
    [Fact]
    public void UpdateAuthDto_WithNullFields_ShouldBeValid()
    {
        var dto = new UpdateAuthDto();

        dto.PasswordHash.Should().BeNull();
        dto.Token.Should().BeNull();
    }

    [Fact]
    public void UpdateAuthDto_WithTokenAndPasswordHash_ShouldBeValid()
    {
        var dto = new UpdateAuthDto
        {
            PasswordHash = "updated-hashed-password",
            Token = "updated-token"
        };

        dto.PasswordHash.Should().Be("updated-hashed-password");
        dto.Token.Should().Be("updated-token");
    }
}
