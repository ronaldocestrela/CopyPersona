using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PersonaScript.Server.Middleware;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth-policy";
    public const string AiPolicy = "ai-generation-policy";
    public const string WebhooksPolicy = "webhooks-policy";

    public static IServiceCollection AddSecurityRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var authPermitLimit = configuration.GetValue("Security:RateLimiting:AuthPermitLimit", 10);
        var aiPermitLimit = configuration.GetValue("Security:RateLimiting:AiPermitLimit", 10);
        var webhooksPermitLimit = configuration.GetValue("Security:RateLimiting:WebhooksPermitLimit", 60);
        var windowSeconds = configuration.GetValue("Security:RateLimiting:WindowSeconds", 60);
        var windowTimeSpan = TimeSpan.FromSeconds(windowSeconds);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Headers.RetryAfter = windowSeconds.ToString();
                context.HttpContext.Response.ContentType = "application/json";

                var payload = new
                {
                    isSuccess = false,
                    error = new
                    {
                        code = "RateLimit.Exceeded",
                        message = "Muitas requisições. Por favor, aguarde antes de tentar novamente.",
                        type = 3
                    }
                };

                await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(payload), token);
            };

            // 1. Política de Autenticação (prevenção contra brute-force e credential stuffing por IP)
            options.AddPolicy(AuthPolicy, httpContext =>
            {
                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = authPermitLimit,
                        Window = windowTimeSpan,
                        QueueLimit = 0
                    });
            });

            // 2. Política de Geração de IA (particionada por Tenant/User ou IP)
            options.AddPolicy(AiPolicy, httpContext =>
            {
                var partitionKey = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                                   ?? httpContext.User.FindFirstValue("tenant_id")
                                   ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                   ?? "unknown_ai_client";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: partitionKey,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = aiPermitLimit,
                        Window = windowTimeSpan,
                        QueueLimit = 0
                    });
            });

            // 3. Política de Webhooks (resiliência para Stripe com proteção contra flood)
            options.AddPolicy(WebhooksPolicy, httpContext =>
            {
                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_webhook_ip";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = webhooksPermitLimit,
                        Window = windowTimeSpan,
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }
}
