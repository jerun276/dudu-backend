using Microsoft.AspNetCore.Mvc;
using CupidLearn.Application.Exceptions;

namespace CupidLearn.Api.Infrastructure;

public static class GlobalExceptionHandlingExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error;

                var traceId = context.TraceIdentifier;

                var (statusCode, title, detail) = exception switch
                {
                    AppException appEx => (appEx.StatusCode, appEx.Title, appEx.Message),
                    UnauthorizedAccessException ex => (StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message),
                    InvalidOperationException ex => (StatusCodes.Status400BadRequest, "Bad Request", ex.Message),
                    ArgumentException ex => (StatusCodes.Status400BadRequest, "Bad Request", ex.Message),
                    _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

                var payload = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail,
                    Instance = context.Request.Path
                };

                payload.Extensions["traceId"] = traceId;

                await context.Response.WriteAsJsonAsync(payload);
            });
        });

        return app;
    }
}
