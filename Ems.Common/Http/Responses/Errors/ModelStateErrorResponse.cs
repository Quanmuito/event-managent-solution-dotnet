namespace Ems.Common.Http.Responses.Errors;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class ModelStateErrorResponse : ErrorResponse
{
    public ModelStateErrorResponse()
    {
    }

    public ModelStateErrorResponse(ModelStateDictionary modelState)
        : base(
            ErrorCodes.ValidationError,
            "Invalid request data.",
            StatusCodes.Status400BadRequest,
            modelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                )
            )
    {
    }
}
