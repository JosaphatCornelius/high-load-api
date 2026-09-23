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
            var response = new ResponsesEnvelope<object>
            {
                Status = "Error",
                Data = []
            };

            var traceId = httpContext.TraceIdentifier;
            var path = httpContext.Request.Path;
            var method = httpContext.Request.Method;

            switch (exception)
            {
                // Missing or Invalid Resources
                case KeyNotFoundException keyNotFoundEx:
                    response.Code = StatusCodes.Status404NotFound;
                    response.Message = "Resource Not Found";
                    _logger.LogWarning("404 | Trace: {TraceId} | {Method} {Path} | {ErrorMessage}", traceId, method, path, keyNotFoundEx.Message);
                    break;

                // Bad Input / Validation Errors
                case ArgumentNullException argNullEx:
                    response.Code = StatusCodes.Status400BadRequest;
                    response.Message = "Missing Argument";
                    _logger.LogWarning("400 | Trace: {TraceId} | {Method} {Path} | {ErrorMessage}", traceId, method, path, argNullEx.Message);
                    break;

                case ArgumentException argEx:
                    response.Code = StatusCodes.Status400BadRequest;
                    response.Message = "Bad Request";
                    _logger.LogWarning("400 | Trace: {TraceId} | {Method} {Path} | {ErrorMessage}", traceId, method, path, argEx.Message);
                    break;

                // Business Rule Violations
                case InvalidOperationException invalidOpEx:
                    response.Code = StatusCodes.Status422UnprocessableEntity;
                    response.Message = "Unprocessable Entity";
                    _logger.LogWarning("422 | Trace: {TraceId} | {Method} {Path} | {ErrorMessage}", traceId, method, path, invalidOpEx.Message);
                    break;

                // Access / Permission Issues
                case UnauthorizedAccessException:
                    response.Code = StatusCodes.Status403Forbidden;
                    response.Message = "Forbidden";
                    _logger.LogWarning("403 | Trace: {TraceId} | {Method} {Path} | Unauthorized access attempt.", traceId, method, path);
                    break;

                // Database Race Conditions
                case DbUpdateConcurrencyException:
                    response.Code = StatusCodes.Status409Conflict;
                    response.Message = "Data Conflict";
                    _logger.LogWarning("409 | Trace: {TraceId} | {Method} {Path} | Database concurrency conflict.", traceId, method, path);
                    break;

                // PostgreSQL Specific Database Violations
                case DbUpdateException dbEx when dbEx.InnerException is PostgresException pgEx:
                    switch (pgEx.SqlState)
                    {
                        case "23505": // Unique constraint violation
                            response.Code = StatusCodes.Status409Conflict;
                            response.Message = "Resource already exists";
                            _logger.LogWarning("409 | Trace: {TraceId} | {Method} {Path} | Unique constraint violation.", traceId, method, path);
                            break;

                        case "23503": // Foreign key violation
                            response.Code = StatusCodes.Status400BadRequest;
                            response.Message = "Invalid Reference";
                            _logger.LogWarning("400 | Trace: {TraceId} | {Method} {Path} | Foreign key violation.", traceId, method, path);
                            break;

                        default:
                            response.Code = StatusCodes.Status500InternalServerError;
                            response.Message = "Database Error";
                            _logger.LogError(pgEx, "500 | Trace: {TraceId} | {Method} {Path} | Postgres Error: {ErrorMessage}", traceId, method, path, pgEx.Message);
                            break;
                    }
                    break;

                // Client Disconnects
                case OperationCanceledException:
                    response.Code = 499;
                    response.Message = "Request Canceled";
                    _logger.LogInformation("499 | Trace: {TraceId} | {Method} {Path} | Client closed connection.", traceId, method, path);
                    break;

                // Unhandled Fatal Errors
                default:
                    response.Code = StatusCodes.Status500InternalServerError;
                    response.Message = "Internal Server Error";
                    _logger.LogError(exception, "500 | Trace: {TraceId} | {Method} {Path} | Unhandled system exception.", traceId, method, path);
                    break;
            }

            httpContext.Response.Headers.Append("X-Trace-Id", traceId);
            httpContext.Response.StatusCode = response.Code;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
