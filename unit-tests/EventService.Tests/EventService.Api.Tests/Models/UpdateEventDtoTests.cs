namespace EventService.Api.Tests.Models;

using EventService.Api.Models;
using EventService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using Xunit;

public class UpdateEventDtoTests
{
    private static UpdateEventDto CreateDto(
        string? title = null,
        string? hostedBy = null,
        string? details = null,
        DateTime? timeStart = null,
        DateTime? timeEnd = null)
    {
        return new UpdateEventDto
        {
            Title = title,
            HostedBy = hostedBy,
            Details = details,
            TimeStart = timeStart,
            TimeEnd = timeEnd
        };
    }

    [Fact]
    public void Validate_WithValidDto_ShouldReturnNoValidationErrors()
    {
        var dto = TestDataBuilder.CreateValidUpdateEventDto();
        var validationContext = ValidationTestHelper.CreateValidationContext(dto);

        var results = dto.Validate(validationContext);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithTimeStartAfterTimeEnd_ShouldReturnValidationError()
    {
        var dto = TestDataBuilder.CreateInvalidUpdateEventDto();
        var validationContext = ValidationTestHelper.CreateValidationContext(dto);

        var results = dto.Validate(validationContext).ToList();

        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Be("TimeStart must be before TimeEnd.");
        results[0].MemberNames.Should().Contain("TimeStart");
        results[0].MemberNames.Should().Contain("TimeEnd");
    }

    [Theory]
    [InlineData("TimeStart")]
    [InlineData("TimeEnd")]
    [InlineData("Title")]
    public void Validate_WithPartialUpdate_ShouldReturnNoValidationErrors(string property)
    {
        var dto = property switch
        {
            "TimeStart" => CreateDto(timeStart: DateTime.UtcNow.AddDays(1)),
            "TimeEnd" => CreateDto(timeEnd: DateTime.UtcNow.AddDays(2)),
            "Title" => CreateDto(title: "Updated Title"),
            _ => new UpdateEventDto()
        };
        var validationContext = ValidationTestHelper.CreateValidationContext(dto);

        var results = dto.Validate(validationContext);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithTitleTooShort_ShouldReturnValidationError()
    {
        var dto = CreateDto(title: "AB");
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "Title must be between 3 and 200 characters.");
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldReturnValidationError()
    {
        var dto = CreateDto(title: new string('A', 201));
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "Title must be between 3 and 200 characters.");
    }

    [Fact]
    public void Validate_WithNullTitle_ShouldReturnNoValidationError()
    {
        var dto = CreateDto(title: null);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithHostedByTooShort_ShouldReturnValidationError()
    {
        var dto = CreateDto(hostedBy: "");
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "HostedBy must be between 1 and 100 characters.");
    }

    [Fact]
    public void Validate_WithHostedByTooLong_ShouldReturnValidationError()
    {
        var dto = CreateDto(hostedBy: new string('A', 101));
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "HostedBy must be between 1 and 100 characters.");
    }

    [Fact]
    public void Validate_WithDetailsTooLong_ShouldReturnValidationError()
    {
        var dto = CreateDto(details: new string('A', 2001));
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "Details cannot exceed 2000 characters.");
    }
}
