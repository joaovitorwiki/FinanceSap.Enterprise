using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using FinanceSap.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceSap.Api.Extensions;

public static class ApiServiceExtensions
{
    // Políticas de Rate Limiting
    public const string AuthRateLimitPolicy = "auth-fixed-window";
    public const string GlobalRateLimitPolicy = "global-fixed-window";

    public static IServiceCollection AddApiSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddRateLimiter(options =>
        {
            // Resposta 429 padronizada com ProblemDetails (RFC 7807)
            options.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                ctx.HttpContext.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Title = "Muitas requisições",
                    Status = StatusCodes.Status429TooManyRequests,
                    Detail = "Você excedeu o limite de requisições. Tente novamente mais tarde.",
                    Instance = ctx.HttpContext.Request.Path,
                    Extensions = { ["traceId"] = ctx.HttpContext.TraceIdentifier }
                };

                await ctx.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(problemDetails),
                    ct
                );
            };

            // ── Environment-Based Rate Limiting ──────────────────────────────────────
            // Development/Testing: limites relaxados para não bloquear testes.
            // Production: limites estritos para proteção contra abuso.
            var isDevelopmentOrTesting = environment.IsDevelopment() ||
                                        environment.IsEnvironment("Testing");

            // ── Auth Policy (Brute-Force Protection) ────────────────────────────────
            // Endpoints de autenticação: 5 requisições por minuto por IP
            var authPermitLimit = isDevelopmentOrTesting ? 100 : 5;
            var authWindow = isDevelopmentOrTesting ? TimeSpan.FromSeconds(1) : TimeSpan.FromMinutes(1);

            options.AddPolicy(AuthRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = authPermitLimit,
                        Window = authWindow,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0 // Rejeita imediatamente ao atingir o limite
                    }
                )
            );

            // ── Global Policy (General Protection) ──────────────────────────────────
            // Endpoints de transações e consultas: 60 requisições por minuto por usuário autenticado/IP
            var globalPermitLimit = isDevelopmentOrTesting ? 1000 : 60;
            var globalWindow = isDevelopmentOrTesting ? TimeSpan.FromSeconds(1) : TimeSpan.FromMinutes(1);

            options.AddPolicy(GlobalRateLimitPolicy, httpContext =>
            {
                // Para usuários autenticados, usa o UserId como chave
                // Para usuários não autenticados, usa o IP
                var partitionKey = httpContext.User.Identity?.IsAuthenticated == true
                    ? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                    : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: partitionKey,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = globalPermitLimit,
                        Window = globalWindow,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0 // Rejeita imediatamente ao atingir o limite
                    }
                );
            });
        });

        return services;
    }

    /// <summary>
    /// Registers API-specific services.
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Auth Service
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
