namespace EventService.Api.Tests.Models;

using EventService.Api.Models;
using EventService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using Xunit;

public class UpdateEventDtoTests
{
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

    [Fact]
    public void Validate_WithOnlyTimeStart_ShouldReturnNoValidationErrors()
    {
        var dto = new UpdateEventDto
        {
            TimeStart = DateTime.UtcNow.AddDays(1)
        };
        var validationContext = ValidationTestHelper.CreateValidationContext(dto);

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
        var validationContext = ValidationTestHelper.CreateValidationContext(dto);

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
        var validationContext = ValidationTestHelper.CreateValidationContext(dto);

        var results = dto.Validate(validationContext);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithTitleTooShort_ShouldReturnValidationError()
    {
        var dto = new UpdateEventDto
        {
            Title = "AB"
        };
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "Title must be between 3 and 200 characters.");
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldReturnValidationError()
    {
        var dto = new UpdateEventDto
        {
            Title = new string('A', 201)
        };
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "Title must be between 3 and 200 characters.");
    }

    [Fact]
    public void Validate_WithHostedByTooShort_ShouldReturnValidationError()
    {
        var dto = new UpdateEventDto
        {
            HostedBy = ""
        };
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "HostedBy must be between 1 and 100 characters.");
    }

    [Fact]
    public void Validate_WithHostedByTooLong_ShouldReturnValidationError()
    {
        var dto = new UpdateEventDto
        {
            HostedBy = new string('A', 101)
        };
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "HostedBy must be between 1 and 100 characters.");
    }

    [Fact]
    public void Validate_WithDetailsTooLong_ShouldReturnValidationError()
    {
        var dto = new UpdateEventDto
        {
            Details = new string('A', 2001)
        };
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage == "Details cannot exceed 2000 characters.");
    }

    [Fact]
    public void Validate_WithNullTitle_ShouldReturnNoValidationError()
    {
        var dto = new UpdateEventDto
        {
            Title = null
        };
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }
}
