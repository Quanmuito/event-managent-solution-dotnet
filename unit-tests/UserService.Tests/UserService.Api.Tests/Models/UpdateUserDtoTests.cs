namespace UserService.Api.Tests.Models;

using FluentAssertions;
using UserService.Api.Models;
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
