namespace Ems.Common.Http.Responses.Errors;

using Microsoft.AspNetCore.Http;

public class QueryLengthErrorResponse : ErrorResponse
{
    public QueryLengthErrorResponse() : base(ErrorCodes.InvalidArgument, "Search query cannot exceed 500 characters.", StatusCodes.Status400BadRequest)
    {
    }
}
