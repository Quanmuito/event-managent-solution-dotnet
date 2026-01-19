namespace Ems.Common.Http.ExceptionHandler;

using System.Collections.Generic;
using System.Diagnostics;
using Ems.Common.Http.Responses.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
        var errorResponse = CreateErrorResponse(exception, traceId);

        logger.LogError(
            exception,
            "Exception occurred. TraceId: {TraceId}, StatusCode: {StatusCode}, ErrorCode: {ErrorCode}",
            traceId,
            errorResponse.Status,
            errorResponse.ErrorCode);

        httpContext.Response.StatusCode = errorResponse.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true;
    }

    private static ErrorResponse CreateErrorResponse(Exception exception, string traceId)
    {
        return exception switch
        {
            KeyNotFoundException => new ErrorResponse(
                ErrorCodes.NotFound,
                exception.Message,
                StatusCodes.Status404NotFound)
            {
                TraceId = traceId
            },
            FormatException => new ErrorResponse(
                ErrorCodes.FormatError,
                exception.Message,
                StatusCodes.Status400BadRequest)
            {
                TraceId = traceId
            },
            ArgumentException => new ErrorResponse(
                ErrorCodes.InvalidArgument,
                exception.Message,
                StatusCodes.Status400BadRequest)
            {
                TraceId = traceId
            },
            InvalidOperationException => new ErrorResponse(
                ErrorCodes.InvalidOperation,
                exception.Message,
                StatusCodes.Status400BadRequest)
            {
                TraceId = traceId
            },
            _ => new ErrorResponse(
                ErrorCodes.InternalError,
                "An error occurred while processing the request.",
                StatusCodes.Status500InternalServerError)
            {
                TraceId = traceId
            }
        };
    }
}
