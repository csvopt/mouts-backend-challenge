using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                validationException.Errors.Select(error => (ValidationErrorDetail)error)),
            KeyNotFoundException => CreateError(
                StatusCodes.Status404NotFound,
                "Resource not found.",
                "ResourceNotFound",
                exception.Message),
            UnauthorizedAccessException => CreateError(
                StatusCodes.Status401Unauthorized,
                "Authentication failed.",
                "AuthenticationError",
                exception.Message),
            DomainException => CreateError(
                StatusCodes.Status409Conflict,
                "Business rule violation.",
                "BusinessRuleViolation",
                exception.Message),
            InvalidOperationException => CreateError(
                StatusCodes.Status409Conflict,
                "Operation conflict.",
                "OperationConflict",
                exception.Message),
            ArgumentException => CreateError(
                StatusCodes.Status400BadRequest,
                "Invalid request.",
                "InvalidArgument",
                exception.Message),
            _ => CreateError(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                "InternalServerError",
                "The server could not complete the request.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled error processing {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            _logger.LogWarning(exception, "Request failed with status {StatusCode}", statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }

    private static (int StatusCode, string Message, IEnumerable<ValidationErrorDetail> Errors) CreateError(
        int statusCode,
        string message,
        string error,
        string detail)
    {
        return (
            statusCode,
            message,
            [new ValidationErrorDetail { Error = error, Detail = detail }]);
    }
}
