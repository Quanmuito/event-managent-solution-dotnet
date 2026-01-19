namespace Ems.Common.Http.Responses.Errors;

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class ErrorResponse : ProblemDetails
{
    public string ErrorCode { get; set; } = ErrorCodes.InternalError;
    public string TraceId { get; set; } = Activity.Current?.TraceId.ToString() ?? string.Empty;

    public string Message
    {
        get => Title ?? string.Empty;
        set
        {
            Title = value;
            Detail = value;
        }
    }

    public Dictionary<string, string[]> Errors
    {
        get => Extensions.TryGetValue("errors", out var errors) && errors is Dictionary<string, string[]> dict
            ? dict
            : [];
        set => Extensions["errors"] = value;
    }

    public ErrorResponse()
    {
        Title = "An error occurred while processing the request.";
        Detail = "An error occurred while processing the request.";
    }

    public ErrorResponse(string message)
    {
        Title = message;
        Detail = message;
    }

    public ErrorResponse(string message, Dictionary<string, string[]> errors)
    {
        Title = message;
        Detail = message;
        Extensions["errors"] = errors;
    }

    public ErrorResponse(string errorCode, string message, int statusCode)
    {
        ErrorCode = errorCode;
        Title = message;
        Detail = message;
        Status = statusCode;
    }

    public ErrorResponse(string errorCode, string message, int statusCode, Dictionary<string, string[]> errors)
    {
        ErrorCode = errorCode;
        Title = message;
        Detail = message;
        Status = statusCode;
        Extensions["errors"] = errors;
    }
}