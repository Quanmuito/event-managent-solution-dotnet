namespace EventService.Api.Tests.Models.Api.Event;
using EventService.Api.Models.Api.Event;
using EventService.Tests.Helpers;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xunit;

public class CreateEventDtoTests
{
    [Fact]
    public void Validate_WithValidDto_ShouldReturnNoValidationErrors()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        var validationContext = new ValidationContext(dto);

        var results = dto.Validate(validationContext);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithTimeStartAfterTimeEnd_ShouldReturnValidationError()
    {
        var dto = TestDataBuilder.CreateInvalidCreateEventDto();
        var validationContext = new ValidationContext(dto);

        var results = dto.Validate(validationContext).ToList();

        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Be("TimeStart must be before TimeEnd.");
        results[0].MemberNames.Should().Contain("TimeStart");
        results[0].MemberNames.Should().Contain("TimeEnd");
    }

    [Fact]
    public void Validate_WithTimeStartEqualToTimeEnd_ShouldReturnValidationError()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        dto.TimeStart = DateTime.UtcNow;
        dto.TimeEnd = dto.TimeStart;
        var validationContext = new ValidationContext(dto);

        var results = dto.Validate(validationContext).ToList();

        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Be("TimeStart must be before TimeEnd.");
    }
}
