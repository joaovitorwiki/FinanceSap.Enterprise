using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceSap.Api.Extensions;

public static class ApiServiceExtensions
{
    // Constante compartilhada entre DI e controller — elimina strings mágicas duplicadas.
    public const string CustomersRateLimitPolicy = "customers-fixed-window";

    public static IServiceCollection AddApiSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddRateLimiter(options =>
        {
            // Resposta 429 genérica — não revela limites internos ao atacante.
            // Não inclui Retry-After para não auxiliar timing de ataques automatizados.
            options.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.StatusCode  = StatusCodes.Status429TooManyRequests;
                ctx.HttpContext.Response.ContentType = "application/json";

                await ctx.HttpContext.Response.WriteAsync(
                    """{"message":"Muitas requisições. Tente novamente em instantes."}""",
                    ct
                );
            };

            // ── Environment-Based Rate Limiting ──────────────────────────────────────
            // Development/Testing: limites relaxados para não bloquear testes.
            // Production: limites estritos para proteção contra abuso.
            var isDevelopmentOrTesting = environment.IsDevelopment() || 
                                        environment.IsEnvironment("Testing");

            var section     = configuration.GetSection("RateLimiting:Customers");
            var permitLimit = isDevelopmentOrTesting 
                ? 1000  // Limite alto em dev/test — não bloqueia testes de integração
                : section.GetValue<int>("PermitLimit", 10);
            
            var window = isDevelopmentOrTesting
                ? TimeSpan.FromSeconds(1)  // Janela curta em dev/test
                : TimeSpan.FromSeconds(section.GetValue<int>("WindowSeconds", 60));

            options.AddPolicy(CustomersRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    // Chave de partição: IP real do cliente.
                    // Em produção atrás de proxy reverso, habilite ForwardedHeaders
                    // e use X-Forwarded-For após validar que o proxy é confiável.
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit          = permitLimit,
                        Window               = window,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        // QueueLimit = 0: rejeita imediatamente ao atingir o limite.
                        // Fila aberta daria vantagem ao atacante mantendo conexões abertas.
                        QueueLimit           = 0
                    }
                )
            );
        });

        return services;
    }
}
