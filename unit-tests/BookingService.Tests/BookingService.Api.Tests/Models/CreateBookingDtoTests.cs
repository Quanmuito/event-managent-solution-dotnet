namespace BookingService.Api.Tests.Models;

using BookingService.Api.Models;
using BookingService.Api.Tests.Helpers;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using FluentAssertions;
using Xunit;

public class CreateBookingDtoTests
{
    [Fact]
    public void Validate_WithValidStatus_ShouldReturnNoValidationErrors()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(BookingStatus.Registered)]
    [InlineData(BookingStatus.QueueEnrolled)]
    public void Validate_WithValidStatusValues_ShouldReturnNoValidationErrors(string status)
    {
        var dto = new CreateBookingDto
        {
            EventId = "507f1f77bcf86cd799439012",
            Status = status,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890"
        };
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldReturnValidationError()
    {
        var dto = TestDataBuilder.CreateInvalidCreateBookingDto();
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Status"));
    }

    [Fact]
    public void Validate_WithNullStatus_ShouldReturnValidationError()
    {
        var dto = new CreateBookingDto
        {
            EventId = "507f1f77bcf86cd799439012",
            Status = null!,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890"
        };
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Status"));
    }

    [Fact]
    public void Validate_WithEmptyStatus_ShouldReturnValidationError()
    {
        var dto = new CreateBookingDto
        {
            EventId = "507f1f77bcf86cd799439012",
            Status = string.Empty,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890"
        };
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Status"));
    }

    [Fact]
    public void Validate_WithCanceledStatus_ShouldReturnValidationError()
    {
        var dto = new CreateBookingDto
        {
            EventId = "507f1f77bcf86cd799439012",
            Status = BookingStatus.Canceled,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890"
        };
        var validationContext = ValidationTestHelper.CreateValidationContext(dto);

        var results = dto.Validate(validationContext).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("Status");
        results[0].ErrorMessage.Should().Contain($"Status must be either '{BookingStatus.Registered}' or '{BookingStatus.QueueEnrolled}'");
    }

    [Fact]
    public void Validate_WithQueuePendingStatus_ShouldReturnValidationError()
    {
        var dto = new CreateBookingDto
        {
            EventId = "507f1f77bcf86cd799439012",
            Status = BookingStatus.QueuePending,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890"
        };
        var validationContext = ValidationTestHelper.CreateValidationContext(dto);

        var results = dto.Validate(validationContext).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("Status");
        results[0].ErrorMessage.Should().Contain($"Status must be either '{BookingStatus.Registered}' or '{BookingStatus.QueueEnrolled}'");
    }
}
