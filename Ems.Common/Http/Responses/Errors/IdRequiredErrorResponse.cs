namespace Ems.Common.Http.Responses.Errors;

using Microsoft.AspNetCore.Http;

public class IdRequiredErrorResponse : ErrorResponse
{
    public IdRequiredErrorResponse() : base(ErrorCodes.IdRequired, "ID is required.", StatusCodes.Status400BadRequest)
    {
    }
}
