namespace Ems.Common.Tests.Http.Responses.Errors;

using Ems.Common.Http.Responses.Errors;
using Ems.Common.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using Xunit;

public class ErrorResponseTests
{
    [Fact]
    public void Constructor_Default_ShouldSetDefaultValues()
    {
        var response = new ErrorResponse();

        response.ErrorCode.Should().Be(ErrorCodes.InternalError);
        response.Title.Should().Be("An error occurred while processing the request.");
        response.Detail.Should().Be("An error occurred while processing the request.");
    }

    [Fact]
    public void Constructor_Default_ShouldSetTraceIdFromActivity()
    {
        using var _ = ActivityTestHelper.CreateActivityWithTraceId();

        var response = new ErrorResponse();

        response.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_Default_ShouldSetEmptyTraceIdWhenNoActivity()
    {
        var response = new ErrorResponse();

        response.TraceId.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetTitleAndDetail()
    {
        var message = "Test error message";

        var response = new ErrorResponse(message);

        response.Title.Should().Be(message);
        response.Detail.Should().Be(message);
        response.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_ShouldSetAllProperties()
    {
        var message = "Validation failed";
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error 1", "Error 2" } },
            { "Field2", new[] { "Error 3" } }
        };

        var response = new ErrorResponse(message, errors);

        response.Title.Should().Be(message);
        response.Detail.Should().Be(message);
        response.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Constructor_WithErrorCodeMessageAndStatusCode_ShouldSetAllProperties()
    {
        var errorCode = ErrorCodes.ValidationError;
        var message = "Invalid input";
        var statusCode = StatusCodes.Status400BadRequest;

        var response = new ErrorResponse(errorCode, message, statusCode);

        response.ErrorCode.Should().Be(errorCode);
        response.Title.Should().Be(message);
        response.Detail.Should().Be(message);
        response.Status.Should().Be(statusCode);
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAllProperties()
    {
        var errorCode = ErrorCodes.ValidationError;
        var message = "Invalid input";
        var statusCode = StatusCodes.Status400BadRequest;
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error 1" } }
        };

        var response = new ErrorResponse(errorCode, message, statusCode, errors);

        response.ErrorCode.Should().Be(errorCode);
        response.Title.Should().Be(message);
        response.Detail.Should().Be(message);
        response.Status.Should().Be(statusCode);
        response.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Message_Getter_ShouldReturnTitle()
    {
        var response = new ErrorResponse("Test message");

        response.Message.Should().Be(response.Title);
    }

    [Fact]
    public void Message_Getter_ShouldReturnEmptyStringWhenTitleIsNull()
    {
        var response = new ErrorResponse();
        response.Title = null;

        response.Message.Should().BeEmpty();
    }

    [Fact]
    public void Message_Setter_ShouldUpdateTitleAndDetail()
    {
        var response = new ErrorResponse();
        var newMessage = "New error message";

        response.Message = newMessage;

        response.Title.Should().Be(newMessage);
        response.Detail.Should().Be(newMessage);
        response.Message.Should().Be(newMessage);
    }

    [Fact]
    public void Errors_Getter_ShouldReturnDictionaryFromExtensions()
    {
        var response = new ErrorResponse();
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error 1" } }
        };
        response.Extensions["errors"] = errors;

        response.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Errors_Getter_ShouldReturnEmptyDictionaryWhenNotPresent()
    {
        var response = new ErrorResponse();

        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Errors_Getter_ShouldReturnEmptyDictionaryWhenExtensionsValueIsNotDictionary()
    {
        var response = new ErrorResponse();
        response.Extensions["errors"] = "invalid";

        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Errors_Setter_ShouldStoreDictionaryInExtensions()
    {
        var response = new ErrorResponse();
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error 1" } }
        };

        response.Errors = errors;

        response.Extensions["errors"].Should().Be(errors);
        response.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void TraceId_Setter_ShouldUpdateValue()
    {
        var response = new ErrorResponse();
        var traceId = "test-trace-id";

        response.TraceId = traceId;

        response.TraceId.Should().Be(traceId);
    }
}
