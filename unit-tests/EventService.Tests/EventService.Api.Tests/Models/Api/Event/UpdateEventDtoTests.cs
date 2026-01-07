namespace EventService.Api.Tests.Models.Api.Event;
using EventService.Api.Models.Api.Event;
using EventService.Tests.Helpers;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xunit;

public class UpdateEventDtoTests
{
    [Fact]
    public void Validate_WithValidDto_ShouldReturnNoValidationErrors()
    {
        var dto = TestDataBuilder.CreateValidUpdateEventDto();
        var validationContext = new ValidationContext(dto);

        var results = dto.Validate(validationContext);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithTimeStartAfterTimeEnd_ShouldReturnValidationError()
    {
        var dto = TestDataBuilder.CreateInvalidUpdateEventDto();
        var validationContext = new ValidationContext(dto);

        var results = dto.Validate(validationContext).ToList();

        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Be("TimeStart must be before TimeEnd.");
        results[0].MemberNames.Should().Contain("TimeStart");
        results[0].MemberNames.Should().Contain("TimeEnd");
    }

    [Fact]
    public void Validate_WithOnlyTimeStart_ShouldReturnNoValidationErrors()
    {
        var dto = new UpdateEventDto
        {
            TimeStart = DateTime.UtcNow.AddDays(1)
        };
        var validationContext = new ValidationContext(dto);

        var results = dto.Validate(validationContext);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithOnlyTimeEnd_ShouldReturnNoValidationErrors()
    {
        var dto = new UpdateEventDto
        {
            TimeEnd = DateTime.UtcNow.AddDays(2)
        };
        var validationContext = new ValidationContext(dto);

        var results = dto.Validate(validationContext);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithPartialUpdate_ShouldReturnNoValidationErrors()
    {
        var dto = new UpdateEventDto
        {
            Title = "Updated Title"
        };
        var validationContext = new ValidationContext(dto);

        var results = dto.Validate(validationContext);

        results.Should().BeEmpty();
    }
}
