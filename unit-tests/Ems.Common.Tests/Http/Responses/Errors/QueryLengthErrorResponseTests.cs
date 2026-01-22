namespace Ems.Common.Tests.Http.Responses.Errors;

using Ems.Common.Http.Responses.Errors;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using Xunit;

public class QueryLengthErrorResponseTests
{
    [Fact]
    public void Constructor_ShouldSetErrorCode()
    {
        var response = new QueryLengthErrorResponse();
        response.ErrorCode.Should().Be(ErrorCodes.InvalidArgument);
    }

    [Fact]
    public void Constructor_ShouldSetMessage()
    {
        var response = new QueryLengthErrorResponse();
        response.Message.Should().Be("Search query cannot exceed 500 characters.");
    }

    [Fact]
    public void Constructor_ShouldSetStatusCode()
    {
        var response = new QueryLengthErrorResponse();
        response.Status.Should().Be(StatusCodes.Status400BadRequest);
    }
}
