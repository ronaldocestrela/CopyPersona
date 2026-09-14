using System.Diagnostics;

namespace PersonaScript.Server.Middleware;

public sealed class PerformanceTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceTimingMiddleware> _logger;
    private const double SlaThresholdMs = 500.0;

    public PerformanceTimingMiddleware(RequestDelegate next, ILogger<PerformanceTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        context.Response.OnStarting(() =>
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
            context.Response.Headers["Server-Timing"] = $"app;dur={elapsedMs:F1}";

            if (elapsedMs > SlaThresholdMs)
            {
                _logger.LogWarning(
                    "[Performance SLA Warning] Request {Method} {Path} took {ElapsedMs:F1}ms (exceeded {ThresholdMs}ms threshold). StatusCode: {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    elapsedMs,
                    SlaThresholdMs,
                    context.Response.StatusCode);
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
