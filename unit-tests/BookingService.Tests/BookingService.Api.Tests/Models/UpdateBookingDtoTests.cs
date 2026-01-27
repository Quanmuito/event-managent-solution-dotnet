namespace BookingService.Api.Tests.Models;

using BookingService.Api.Models;
using BookingService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using Xunit;

public class UpdateBookingDtoTests
{
    private static UpdateBookingDto CreateDto(
        string? name = null,
        string? email = null,
        string? phone = null)
    {
        return new UpdateBookingDto
        {
            Name = name,
            Email = email,
            Phone = phone
        };
    }

    [Fact]
    public void Validate_WithValidDto_ShouldReturnNoValidationErrors()
    {
        var dto = TestDataBuilder.CreateValidUpdateBookingDto();
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

    [Theory]
    [InlineData("john.doe@example.com")]
    [InlineData(null)]
    public void Validate_WithValidEmail_ShouldPass(string? email)
    {
        var dto = CreateDto(email: email);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldReturnValidationError()
    {
        var dto = CreateDto(name: string.Empty);
        var (isValid, results) = ValidationTestHelper.ValidateObject(dto);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Name") && r.ErrorMessage!.Contains("between 1 and 100 characters"));
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
