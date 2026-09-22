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

            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path
            };

            switch (exception)
            {
                // Missing or Invalid Resources
                case KeyNotFoundException:
                    problemDetails.Status = StatusCodes.Status404NotFound;
                    problemDetails.Title = "Resource Not Found";
                    problemDetails.Detail = "The requested resource could not be found.";
                    break;

                // Bad Input / Validation Errors
                case ArgumentNullException argNullEx:
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Missing Argument";
                    problemDetails.Detail = argNullEx.Message;
                    break;

                case ArgumentException argEx:
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Bad Request";
                    problemDetails.Detail = argEx.Message;
                    break;

                // Business Rule Violations
                case InvalidOperationException invalidOpEx:
                    // 422 is standard for semantic/business logic errors where the syntax is valid
                    problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                    problemDetails.Title = "Unprocessable Entity";
                    problemDetails.Detail = invalidOpEx.Message;
                    break;

                // Access / Permission Issues
                case UnauthorizedAccessException:
                    problemDetails.Status = StatusCodes.Status403Forbidden;
                    problemDetails.Title = "Forbidden";
                    problemDetails.Detail = "You do not have permission to perform this action.";
                    break;

                // Database Race Conditions
                case DbUpdateConcurrencyException:
                    problemDetails.Status = StatusCodes.Status409Conflict;
                    problemDetails.Title = "Data Conflict";
                    problemDetails.Detail = "The resource was modified by another process. Please reload and try again.";
                    break;

                // PostgreSQL Specific Database Violations
                case DbUpdateException dbEx when dbEx.InnerException is PostgresException pgEx:
                    switch (pgEx.SqlState)
                    {
                        case "23505": // Unique constraint violation (e.g., duplicate email)
                            problemDetails.Status = StatusCodes.Status409Conflict;
                            problemDetails.Title = "Resource already exists";
                            problemDetails.Detail = "A record with this unique identifier already exists.";
                            break;

                        case "23503": // Foreign key violation (e.g., CartID doesn't exist)
                            problemDetails.Status = StatusCodes.Status400BadRequest;
                            problemDetails.Title = "Invalid Reference";
                            problemDetails.Detail = "The operation references a record that does not exist.";
                            break;

                        default:
                            problemDetails.Status = StatusCodes.Status500InternalServerError;
                            problemDetails.Title = "Database Error";
                            problemDetails.Detail = "An unexpected database error occurred.";
                            break;
                    }
                    break;

                // Client Disconnects
                case OperationCanceledException:
                    // 499 Client Closed Request is the Nginx/APISIX standard
                    problemDetails.Status = 499;
                    problemDetails.Title = "Request Canceled";
                    problemDetails.Detail = "The request was canceled by the client.";
                    break;

                // Unhandled Fatal Errors
                default:
                    problemDetails.Status = StatusCodes.Status500InternalServerError;
                    problemDetails.Title = "Internal Server Error";
                    problemDetails.Detail = "An unexpected error occurred processing your request.";
                    break;
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
