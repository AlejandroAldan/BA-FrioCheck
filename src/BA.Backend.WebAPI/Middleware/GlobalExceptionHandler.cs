using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using BA.Backend.Application.Exceptions;

namespace BA.Backend.WebAPI.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (ex)
        {
            case UserNotFoundExeption userNotFound:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = userNotFound.Message;
                response.ErrorCode = "USER_NOT_FOUND";
                break;

            case InvalidCredentialException invalidCreds:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = invalidCreds.Message;
                response.ErrorCode = "INVALID_CREDENTIALS";
                break;

            case ValidationExeption validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Validation failed";
                response.ErrorCode = "VALIDATION_ERROR";
                response.Errors = validationEx.Errors;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "Internal server error";
                response.ErrorCode = "INTERNAL_SERVER_ERROR";
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
