using high_load_api.Models.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace high_load_api.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An exception occurred: {Message}", exception.Message);

            var response = new ResponsesEnvelope<object>
            {
                Status = "Error",
                Data = []
            };

            switch (exception)
            {
                // Missing or Invalid Resources
                case KeyNotFoundException keyNotFoundEx:
                    response.Code = StatusCodes.Status404NotFound;
                    response.Message = "Resource Not Found";
                    _logger.LogWarning("Resource not found: {ErrorMessage}", keyNotFoundEx.Message);
                    break;

                // Bad Input / Validation Errors
                case ArgumentNullException argNullEx:
                    response.Code = StatusCodes.Status400BadRequest;
                    response.Message = "Missing Argument";
                    _logger.LogWarning("Missing argument: {ErrorMessage}", argNullEx.Message);
                    break;

                case ArgumentException argEx:
                    response.Code = StatusCodes.Status400BadRequest;
                    response.Message = "Bad Request";
                    _logger.LogWarning("Bad argument: {ErrorMessage}", argEx.Message);
                    break;

                // Business Rule Violations
                case InvalidOperationException invalidOpEx:
                    // 422 is standard for semantic/business logic errors where the syntax is valid
                    response.Code = StatusCodes.Status422UnprocessableEntity;
                    response.Message = "Unprocessable Entity";
                    _logger.LogWarning("Unprocessable entity: {ErrorMessage}", invalidOpEx.Message);
                    break;

                // Access / Permission Issues
                case UnauthorizedAccessException:
                    response.Code = StatusCodes.Status403Forbidden;
                    response.Message = "Forbidden";
                    _logger.LogWarning("Unauthorized access attempt.");
                    break;

                // Database Race Conditions
                case DbUpdateConcurrencyException:
                    response.Code = StatusCodes.Status409Conflict;
                    response.Message = "Data Conflict";
                    _logger.LogError("Database concurrency conflict occurred.");
                    break;

                // PostgreSQL Specific Database Violations
                case DbUpdateException dbEx when dbEx.InnerException is PostgresException pgEx:
                    switch (pgEx.SqlState)
                    {
                        case "23505": // Unique constraint violation (e.g., duplicate email)
                            response.Code = StatusCodes.Status409Conflict;
                            response.Message = "Resource already exists";
                            break;

                        case "23503": // Foreign key violation (e.g., CartID doesn't exist)
                            response.Code = StatusCodes.Status400BadRequest;
                            response.Message = "Invalid Reference";
                            break;

                        default:
                            response.Code = StatusCodes.Status500InternalServerError;
                            response.Message = "Database Error";
                            _logger.LogError(pgEx, "PostgreSQL error occurred: {ErrorMessage}", pgEx.Message);
                            break;
                    }
                    break;

                // Client Disconnects
                case OperationCanceledException:
                    // 499 Client Closed Request is the Nginx/APISIX standard
                    response.Code = 499;
                    response.Message = "Request Canceled";
                    _logger.LogInformation("Request cancelled by client.");
                    break;

                // Unhandled Fatal Errors
                default:
                    response.Code = StatusCodes.Status500InternalServerError;
                    response.Message = "Internal Server Error";
                    _logger.LogError(exception, "Unhandled system exception: {ErrorMessage}", exception.Message);
                    break;
            }

            httpContext.Response.StatusCode = response.Code;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
