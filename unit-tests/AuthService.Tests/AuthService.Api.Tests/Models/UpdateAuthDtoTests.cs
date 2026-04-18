namespace AuthService.Api.Tests.Models;

using AuthService.Api.Models;
using FluentAssertions;
using Xunit;

public class UpdateAuthDtoTests
{
    [Fact]
    public void UpdateAuthDto_WithNullToken_ShouldBeValid()
    {
        var dto = new UpdateAuthDto();

        dto.Token.Should().BeNull();
    }

    [Fact]
    public void UpdateAuthDto_WithToken_ShouldBeValid()
    {
        var dto = new UpdateAuthDto
        {
            Token = "updated-token"
        };

        dto.Token.Should().Be("updated-token");
    }
}
