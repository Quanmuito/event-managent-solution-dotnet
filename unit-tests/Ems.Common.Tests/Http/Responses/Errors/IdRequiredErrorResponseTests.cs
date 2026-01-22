namespace Ems.Common.Tests.Http.Responses.Errors;

using Ems.Common.Http.Responses.Errors;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using Xunit;

public class IdRequiredErrorResponseTests
{
    [Fact]
    public void Constructor_ShouldSetErrorCode()
    {
        var response = new IdRequiredErrorResponse();
        response.ErrorCode.Should().Be(ErrorCodes.IdRequired);
    }

    [Fact]
    public void Constructor_ShouldSetMessage()
    {
        var response = new IdRequiredErrorResponse();
        response.Message.Should().Be("ID is required.");
    }

    [Fact]
    public void Constructor_ShouldSetStatusCode()
    {
        var response = new IdRequiredErrorResponse();
        response.Status.Should().Be(StatusCodes.Status400BadRequest);
    }
}
