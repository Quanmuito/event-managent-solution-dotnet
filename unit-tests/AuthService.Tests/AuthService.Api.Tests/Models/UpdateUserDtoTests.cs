namespace AuthService.Api.Tests.Models;

using AuthService.Api.Models;
using AuthService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class UpdateUserDtoTests
{
    [Fact]
    public void UpdateUserDto_WithAllNullProperties_ShouldBeValid()
    {
        var dto = new UpdateUserDto();

        dto.Email.Should().BeNull();
        dto.Phone.Should().BeNull();
        dto.IsVerified.Should().BeNull();
    }

    [Fact]
    public void UpdateUserDto_WithPartialProperties_ShouldBeValid()
    {
        var dto = new UpdateUserDto
        {
            Email = "updated@example.com"
        };

        dto.Email.Should().Be("updated@example.com");
        dto.Phone.Should().BeNull();
        dto.IsVerified.Should().BeNull();
    }
}
