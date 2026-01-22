namespace BookingService.Api.Tests.Models;

using BookingService.Api.Models;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using Xunit;

public class CreateBookingDtoTests
{
    private static CreateBookingDto CreateDto(
        string? eventId = null,
        string? status = null,
        string? name = null,
        string? email = null,
        string? phone = null)
    {
        return new CreateBookingDto
        {
            EventId = eventId ?? "507f1f77bcf86cd799439012",
            Status = status ?? BookingStatus.Registered,
            Name = name ?? "John Doe",
            Email = email ?? "john.doe@example.com",
            Phone = phone ?? "1234567890"
        };
    }

    [Fact]
    public void Validate_WithValidDto_ShouldReturnNoValidationErrors()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(BookingStatus.Registered)]
    [InlineData(BookingStatus.QueueEnrolled)]
    public void Validate_WithValidStatus_ShouldReturnNoValidationErrors(string status)
    {
        var dto = CreateDto(status: status);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_WithInvalidStatusValue_ShouldReturnValidationError(string? status)
    {
        var dto = new CreateBookingDto
        {
            EventId = "507f1f77bcf86cd799439012",
            Status = BookingStatus.Registered,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890"
        };
        typeof(CreateBookingDto).GetProperty(nameof(CreateBookingDto.Status))!.SetValue(dto, status);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Status"));
    }

    [Theory]
    [InlineData(BookingStatus.Canceled)]
    [InlineData(BookingStatus.QueuePending)]
    public void Validate_WithInvalidStatus_ShouldReturnValidationError(string status)
    {
        var dto = CreateDto(status: status);
        var validationContext = ValidationTestHelper.CreateValidationContext(dto);
        var results = dto.Validate(validationContext).ToList();

        results.Should().HaveCount(1);
        results[0].MemberNames.Should().Contain("Status");
        results[0].ErrorMessage.Should().Contain($"Status must be either '{BookingStatus.Registered}' or '{BookingStatus.QueueEnrolled}'");
    }

    [Fact]
    public void Validate_WithNullEventId_ShouldReturnValidationError()
    {
        var dto = new CreateBookingDto
        {
            EventId = "temp",
            Status = BookingStatus.Registered,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890"
        };
        typeof(CreateBookingDto).GetProperty(nameof(CreateBookingDto.EventId))!.SetValue(dto, null);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("EventId"));
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldReturnValidationError()
    {
        var dto = CreateDto(name: string.Empty);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldReturnValidationError()
    {
        var dto = CreateDto(name: new string('A', 101));
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Name") && r.ErrorMessage!.Contains("between 1 and 100 characters"));
    }

    public static IEnumerable<object[]> ValidNameLengthData()
    {
        yield return new object[] { "A" };
        yield return new object[] { new string('A', 100) };
    }

    [Theory]
    [MemberData(nameof(ValidNameLengthData))]
    public void Validate_WithValidNameLength_ShouldPass(string name)
    {
        var dto = CreateDto(name: name);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }


    [Theory]
    [InlineData("notanemail")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    public void Validate_WithInvalidEmailFormat_ShouldReturnValidationError(string invalidEmail)
    {
        var dto = CreateDto(email: invalidEmail);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Email") && r.ErrorMessage!.Contains("valid email address"));
    }

    [Fact]
    public void Validate_WithValidEmail_ShouldPass()
    {
        var dto = CreateDto(email: "john.doe@example.com");
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNullEmail_ShouldReturnValidationError()
    {
        var dto = new CreateBookingDto
        {
            EventId = "507f1f77bcf86cd799439012",
            Status = BookingStatus.Registered,
            Name = "John Doe",
            Email = "temp@example.com",
            Phone = "1234567890"
        };
        typeof(CreateBookingDto).GetProperty(nameof(CreateBookingDto.Email))!.SetValue(dto, null);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void Validate_WithNullPhone_ShouldReturnValidationError()
    {
        var dto = new CreateBookingDto
        {
            EventId = "507f1f77bcf86cd799439012",
            Status = BookingStatus.Registered,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "1234567890"
        };
        typeof(CreateBookingDto).GetProperty(nameof(CreateBookingDto.Phone))!.SetValue(dto, null);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Phone"));
    }

    [Fact]
    public void Validate_WithPhoneTooLong_ShouldReturnValidationError()
    {
        var dto = CreateDto(phone: new string('1', 21));
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Phone") && r.ErrorMessage!.Contains("cannot exceed 20 characters"));
    }

    [Fact]
    public void Validate_WithPhoneExactly20Chars_ShouldPass()
    {
        var dto = CreateDto(phone: new string('1', 20));
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }
}
