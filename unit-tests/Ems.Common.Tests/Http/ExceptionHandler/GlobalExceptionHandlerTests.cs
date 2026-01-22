namespace Ems.Common.Tests.Http.ExceptionHandler;

using Ems.Common.Http.ExceptionHandler;
using Ems.Common.Http.Responses.Errors;
using Ems.Common.Tests.Helpers;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _mockLogger;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_mockLogger.Object);
    }

    [Theory]
    [MemberData(nameof(ExceptionHandlingTestCases))]
    public async Task TryHandleAsync_WithException_ShouldReturnExpectedResponse(
        Exception exception,
        int expectedStatusCode,
        string expectedErrorCode,
        string expectedMessage)
    {
        var httpContext = HttpContextTestHelper.CreateHttpContext();

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(expectedStatusCode);
        var response = await HttpContextTestHelper.GetResponseBody<ErrorResponse>(httpContext);
        response.ErrorCode.Should().Be(expectedErrorCode);
        response.Message.Should().Be(expectedMessage);
    }

    public static IEnumerable<object[]> ExceptionHandlingTestCases()
    {
        yield return new object[]
        {
            new KeyNotFoundException("Not found"),
            StatusCodes.Status404NotFound,
            ErrorCodes.NotFound,
            "Not found"
        };
        yield return new object[]
        {
            new FormatException("Invalid format"),
            StatusCodes.Status400BadRequest,
            ErrorCodes.FormatError,
            "Invalid format"
        };
        yield return new object[]
        {
            new ArgumentException("Invalid argument"),
            StatusCodes.Status400BadRequest,
            ErrorCodes.InvalidArgument,
            "Invalid argument"
        };
        yield return new object[]
        {
            new InvalidOperationException("Invalid operation"),
            StatusCodes.Status400BadRequest,
            ErrorCodes.InvalidOperation,
            "Invalid operation"
        };
        yield return new object[]
        {
            new Exception("Unexpected error"),
            StatusCodes.Status500InternalServerError,
            ErrorCodes.InternalError,
            "An error occurred while processing the request."
        };
    }

    [Fact]
    public async Task TryHandleAsync_ShouldLogError()
    {
        var httpContext = HttpContextTestHelper.CreateHttpContext();
        var exception = new Exception("Test error");

        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        LoggerTestHelper.VerifyErrorLogged(_mockLogger);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldSetContentTypeToJson()
    {
        var httpContext = HttpContextTestHelper.CreateHttpContext();
        var exception = new Exception("Test error");

        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        httpContext.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldIncludeTraceId()
    {
        var httpContext = HttpContextTestHelper.CreateHttpContext();
        var exception = new Exception("Test error");

        using var _ = ActivityTestHelper.CreateActivityWithTraceId();

        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        var response = await HttpContextTestHelper.GetResponseBody<ErrorResponse>(httpContext);
        response.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturnTrue()
    {
        var httpContext = HttpContextTestHelper.CreateHttpContext();
        var exception = new Exception("Test error");

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
    }
}
