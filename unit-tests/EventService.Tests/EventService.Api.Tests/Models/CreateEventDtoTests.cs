namespace EventService.Api.Tests.Models;

using EventService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using Xunit;

public class CreateEventDtoTests
{
    [Fact]
    public void Validate_WithValidDto_ShouldReturnNoValidationErrors()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        var validationContext = ValidationTestHelper.CreateValidationContext(dto);

        var results = dto.Validate(validationContext);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithTimeStartAfterTimeEnd_ShouldReturnValidationError()
    {
        var dto = TestDataBuilder.CreateInvalidCreateEventDto();
        var validationContext = ValidationTestHelper.CreateValidationContext(dto);

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
        var validationContext = ValidationTestHelper.CreateValidationContext(dto);

        var results = dto.Validate(validationContext).ToList();

        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Be("TimeStart must be before TimeEnd.");
    }

    [Fact]
    public void Validate_WithTitleTooShort_ShouldReturnValidationError()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        dto.Title = "AB";
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "Title must be between 3 and 200 characters.");
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldReturnValidationError()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        dto.Title = new string('A', 201);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "Title must be between 3 and 200 characters.");
    }

    [Fact]
    public void Validate_WithHostedByTooShort_ShouldReturnValidationError()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        dto.HostedBy = "";
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "The HostedBy field is required." || r.ErrorMessage == "HostedBy must be between 1 and 100 characters.");
    }

    [Fact]
    public void Validate_WithHostedByTooLong_ShouldReturnValidationError()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        dto.HostedBy = new string('A', 101);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "HostedBy must be between 1 and 100 characters.");
    }

    [Fact]
    public void Validate_WithDetailsTooLong_ShouldReturnValidationError()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        dto.Details = new string('A', 2001);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "Details cannot exceed 2000 characters.");
    }

    [Fact]
    public void Validate_WithNullDetails_ShouldReturnNoValidationError()
    {
        var dto = TestDataBuilder.CreateValidCreateEventDto();
        dto.Details = null;
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }
}
