namespace Ems.Common.Tests.Http.Responses.Errors;

using Ems.Common.Http.Responses.Errors;
using FluentAssertions;
using Xunit;

public class ErrorCodesTests
{
    [Theory]
    [InlineData(nameof(ErrorCodes.ValidationError), "VALIDATION_ERROR")]
    [InlineData(nameof(ErrorCodes.IdRequired), "ID_REQUIRED")]
    [InlineData(nameof(ErrorCodes.NotFound), "NOT_FOUND")]
    [InlineData(nameof(ErrorCodes.InvalidOperation), "INVALID_OPERATION")]
    [InlineData(nameof(ErrorCodes.InvalidArgument), "INVALID_ARGUMENT")]
    [InlineData(nameof(ErrorCodes.FormatError), "FORMAT_ERROR")]
    [InlineData(nameof(ErrorCodes.InternalError), "INTERNAL_ERROR")]
    public void ErrorCode_ShouldHaveCorrectValue(string propertyName, string expectedValue)
    {
        var field = typeof(ErrorCodes).GetField(propertyName);
        field.Should().NotBeNull();
        var actualValue = field!.GetValue(null)?.ToString();

        actualValue.Should().Be(expectedValue);
    }
}
