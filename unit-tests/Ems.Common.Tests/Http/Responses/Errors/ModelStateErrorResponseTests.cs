namespace Ems.Common.Tests.Http.Responses.Errors;

using Ems.Common.Http.Responses.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using FluentAssertions;
using Xunit;

public class ModelStateErrorResponseTests
{
    [Fact]
    public void Constructor_Default_ShouldCreateInstance()
    {
        var response = new ModelStateErrorResponse();

        response.Should().NotBeNull();
        response.Should().BeAssignableTo<ErrorResponse>();
    }

    [Fact]
    public void Constructor_WithModelStateDictionary_ShouldSetErrorCode()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error message");

        var response = new ModelStateErrorResponse(modelState);

        response.ErrorCode.Should().Be(ErrorCodes.ValidationError);
    }

    [Fact]
    public void Constructor_WithModelStateDictionary_ShouldSetMessage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error message");

        var response = new ModelStateErrorResponse(modelState);

        response.Message.Should().Be("Invalid request data.");
    }

    [Fact]
    public void Constructor_WithModelStateDictionary_ShouldSetStatusCode()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error message");

        var response = new ModelStateErrorResponse(modelState);

        response.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void Constructor_WithModelStateDictionary_ShouldIncludeOnlyEntriesWithErrors()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error 1");
        modelState.AddModelError("Field2", "Error 2");
        modelState.SetModelValue("Field3", new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult("value"), "value");

        var response = new ModelStateErrorResponse(modelState);

        response.Errors.Should().ContainKey("Field1");
        response.Errors.Should().ContainKey("Field2");
        response.Errors.Should().NotContainKey("Field3");
    }

    [Fact]
    public void Constructor_WithModelStateDictionary_ShouldExtractErrorMessages()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error message 1");

        var response = new ModelStateErrorResponse(modelState);

        response.Errors["Field1"].Should().Contain("Error message 1");
    }

    [Fact]
    public void Constructor_WithModelStateDictionary_ShouldGroupMultipleErrorsByKey()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error 1");
        modelState.AddModelError("Field1", "Error 2");

        var response = new ModelStateErrorResponse(modelState);

        response.Errors["Field1"].Should().HaveCount(2);
        response.Errors["Field1"].Should().Contain("Error 1");
        response.Errors["Field1"].Should().Contain("Error 2");
    }

    [Fact]
    public void Constructor_WithEmptyModelStateDictionary_ShouldCreateEmptyErrors()
    {
        var modelState = new ModelStateDictionary();

        var response = new ModelStateErrorResponse(modelState);

        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithModelStateDictionary_ShouldHandleNullModelStateEntry()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error 1");

        var response = new ModelStateErrorResponse(modelState);

        response.Errors.Should().ContainKey("Field1");
        response.Errors.Should().NotContainKey("Field2");
    }
}
