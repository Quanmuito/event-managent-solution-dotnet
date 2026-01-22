namespace TestUtilities.Helpers;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public static class ControllerTestHelper
{
    public static void AssertInternalServerError(IActionResult result)
    {
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    public static T AssertOkResult<T>(IActionResult result)
    {
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
        return (T)okResult.Value!;
    }

    public static void AssertBadRequest(IActionResult result)
    {
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    public static void AssertCreatedAtAction(IActionResult result, string expectedActionName, object expectedRouteValue)
    {
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        createdAtResult!.ActionName.Should().Be(expectedActionName);
        if (expectedRouteValue is string stringValue)
        {
            createdAtResult.RouteValues!["id"].Should().Be(stringValue);
        }
        else
        {
            createdAtResult.RouteValues.Should().ContainKey("id");
        }
    }

    public static void AssertNoContent(IActionResult result)
    {
        result.Should().BeOfType<NoContentResult>();
    }

    public static async Task AssertExceptionThrown<TException>(Func<Task> act, string expectedMessage)
        where TException : Exception
    {
        var exception = await act.Should().ThrowAsync<TException>();
        exception.Which.Message.Should().Be(expectedMessage);
    }
}
