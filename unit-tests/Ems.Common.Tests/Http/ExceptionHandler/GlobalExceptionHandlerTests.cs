namespace Ems.Common.Tests.Http.ExceptionHandler;

using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Ems.Common.Http.ExceptionHandler;
using Ems.Common.Http.Responses.Errors;
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

    [Fact]
    public async Task TryHandleAsync_WithKeyNotFoundException_ShouldReturn404()
    {
        var httpContext = CreateHttpContext();
        var exception = new KeyNotFoundException("Not found");

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var response = await GetResponseBody<ErrorResponse>(httpContext);
        response.ErrorCode.Should().Be(ErrorCodes.NotFound);
        response.Message.Should().Be("Not found");
    }

    [Fact]
    public async Task TryHandleAsync_WithFormatException_ShouldReturn400()
    {
        var httpContext = CreateHttpContext();
        var exception = new FormatException("Invalid format");

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var response = await GetResponseBody<ErrorResponse>(httpContext);
        response.ErrorCode.Should().Be(ErrorCodes.FormatError);
        response.Message.Should().Be("Invalid format");
    }

    [Fact]
    public async Task TryHandleAsync_WithArgumentException_ShouldReturn400()
    {
        var httpContext = CreateHttpContext();
        var exception = new ArgumentException("Invalid argument");

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var response = await GetResponseBody<ErrorResponse>(httpContext);
        response.ErrorCode.Should().Be(ErrorCodes.InvalidArgument);
        response.Message.Should().Be("Invalid argument");
    }

    [Fact]
    public async Task TryHandleAsync_WithInvalidOperationException_ShouldReturn400()
    {
        var httpContext = CreateHttpContext();
        var exception = new InvalidOperationException("Invalid operation");

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var response = await GetResponseBody<ErrorResponse>(httpContext);
        response.ErrorCode.Should().Be(ErrorCodes.InvalidOperation);
        response.Message.Should().Be("Invalid operation");
    }

    [Fact]
    public async Task TryHandleAsync_WithUnhandledException_ShouldReturn500()
    {
        var httpContext = CreateHttpContext();
        var exception = new Exception("Unexpected error");

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var response = await GetResponseBody<ErrorResponse>(httpContext);
        response.ErrorCode.Should().Be(ErrorCodes.InternalError);
        response.Message.Should().Be("An error occurred while processing the request.");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldLogError()
    {
        var httpContext = CreateHttpContext();
        var exception = new Exception("Test error");

        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldSetContentTypeToJson()
    {
        var httpContext = CreateHttpContext();
        var exception = new Exception("Test error");

        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        httpContext.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldIncludeTraceId()
    {
        var httpContext = CreateHttpContext();
        var exception = new Exception("Test error");

        using var activitySource = new ActivitySource("Test");
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = activitySource.StartActivity("TestActivity");
        activity?.SetIdFormat(ActivityIdFormat.W3C);
        activity?.Start();

        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        var response = await GetResponseBody<ErrorResponse>(httpContext);
        response.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturnTrue()
    {
        var httpContext = CreateHttpContext();
        var exception = new Exception("Test error");

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    private static async Task<T> GetResponseBody<T>(HttpContext httpContext)
    {
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(httpContext.Response.Body);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
