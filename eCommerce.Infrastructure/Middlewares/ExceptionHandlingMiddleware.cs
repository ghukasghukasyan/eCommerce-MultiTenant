using eCommerce.Application.Services.Interfaces.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Text.Json;

namespace eCommerce.Infrastructure.Middlewares
{
    public class ExceptionHandlingMiddleware(RequestDelegate _next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DbUpdateException ex)
            {
                var logger = context.RequestServices.GetRequiredService<IAppLogger<ExceptionHandlingMiddleware>>();
                context.Response.ContentType = "application/json";
                if (ex.InnerException is PostgresException innerException)
                {
                    logger.LogError(innerException, "SqlException");

                    switch (innerException.SqlState)
                    {
                        case "23505": // unique_violation
                            context.Response.StatusCode = StatusCodes.Status409Conflict;
                            await WriteJsonAsync(context, "A record with the same unique key already exists.");
                            break;
                        case "23502": // not_null_violation
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await WriteJsonAsync(context, "Cannot insert null.");
                            break;
                        case "23503": // foreign_key_violation
                            context.Response.StatusCode = StatusCodes.Status409Conflict;
                            await WriteJsonAsync(context, "Foreign key constraint violation.");
                            break;
                        default:
                            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                            await WriteJsonAsync(context, "An unexpected database error occurred.");
                            break;
                    }
                }
                else
                {
                    logger.LogError(ex, "Related EFCore Exception");
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await WriteJsonAsync(context, "An error occurred while processing your request.");
                }
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<IAppLogger<ExceptionHandlingMiddleware>>();
                context.Response.ContentType = "application/json";
                logger.LogError(ex, "Unhandled Exception");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await WriteJsonAsync(context, "An unexpected error occurred.");
            }
        }

        private static Task WriteJsonAsync(HttpContext context, string message)
            => context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
    }
}
