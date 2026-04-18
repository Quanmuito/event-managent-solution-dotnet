namespace AuthService.Data.Tests.Models;

using AuthService.Api.Models;
using AuthService.Data.Models;
using AuthService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class AuthTests
{
    private static Auth CreateAuthFromDto(CreateAuthDto dto)
    {
        return new Auth
        {
            UserId = dto.UserId,
            Token = dto.Token,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    [Fact]
    public void GetEntityFromDto_ShouldMapAllPropertiesCorrectly()
    {
        var dto = TestDataBuilder.CreateValidCreateAuthDto();
        var beforeCreation = DateTime.UtcNow;

        var result = CreateAuthFromDto(dto);

        var afterCreation = DateTime.UtcNow;

        result.Should().NotBeNull();
        result.UserId.Should().Be(dto.UserId);
        result.Token.Should().Be(dto.Token);
        result.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        result.CreatedAt.Should().BeOnOrBefore(afterCreation);
        result.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void GetEntityFromDto_ShouldSetCreatedAtToUtcNow()
    {
        var dto = TestDataBuilder.CreateValidCreateAuthDto();
        var beforeCreation = DateTime.UtcNow;

        var result = CreateAuthFromDto(dto);

        var afterCreation = DateTime.UtcNow;

        result.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        result.CreatedAt.Should().BeOnOrBefore(afterCreation);
        result.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }
}
