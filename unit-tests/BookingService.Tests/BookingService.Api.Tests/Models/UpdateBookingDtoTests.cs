namespace BookingService.Api.Tests.Models;

using BookingService.Api.Models;
using BookingService.Api.Tests.Helpers;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class UpdateBookingDtoTests
{
    [Fact]
    public void Validate_WithValidStatus_ShouldReturnNoValidationErrors()
    {
        var dto = TestDataBuilder.CreateValidUpdateBookingDto();
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(BookingStatus.Registered)]
    [InlineData(BookingStatus.Canceled)]
    [InlineData(BookingStatus.QueueEnrolled)]
    [InlineData(BookingStatus.QueuePending)]
    public void Validate_WithValidStatusValues_ShouldReturnNoValidationErrors(string status)
    {
        var dto = new UpdateBookingDto { Status = status };
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNullStatus_ShouldReturnNoValidationErrors()
    {
        var dto = TestDataBuilder.CreateUpdateBookingDtoWithNullStatus();
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithAllNullFields_ShouldReturnNoValidationErrors()
    {
        var dto = new UpdateBookingDto();
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldReturnValidationError()
    {
        var dto = new UpdateBookingDto { Status = "invalid_status" };
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Status"));
    }

    [Fact]
    public void Validate_WithEmptyStatus_ShouldReturnValidationError()
    {
        var dto = new UpdateBookingDto { Status = string.Empty };
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Status"));
    }
}
