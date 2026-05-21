using System.Diagnostics;

namespace ProductCatalog.Api.Middleware;

/// <summary>
/// Custom middleware that logs request duration and method/endpoint info.
/// Built from scratch — no framework helpers.
/// </summary>
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogInformation(
            "[Request {TraceId}] {Method} {Path} started",
            traceId, method, path
        );

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            var level = statusCode >= 500 ? LogLevel.Error
                : statusCode >= 400 ? LogLevel.Warning
                : LogLevel.Information;

            _logger.Log(level,
                "[Request {TraceId}] {Method} {Path} completed — {StatusCode} in {ElapsedMs}ms",
                traceId, method, path, statusCode, elapsedMs
            );
        }
    }
}
