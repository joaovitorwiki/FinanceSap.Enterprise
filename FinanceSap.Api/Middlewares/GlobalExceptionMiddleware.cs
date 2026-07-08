using System.Text.Json;

namespace FinanceSap.Api.Middlewares;

// Error Shielding Middleware — OWASP A05 (Security Misconfiguration).
// Regra absoluta: nenhum detalhe interno chega ao cliente.
// O log interno recebe a exceção completa para auditoria e diagnóstico.
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    // Payload fixo — imutável, sem interpolação de dados da exceção.
    // Impede qualquer vazamento acidental por refatoração futura.
    private static readonly byte[] SafeErrorPayload = JsonSerializer.SerializeToUtf8Bytes(
        new { message = "Ocorreu um erro inesperado." }
    );

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // Log estruturado completo para auditoria interna.
            // O correlationId permite rastrear o erro sem expô-lo ao cliente.
            var correlationId = context.TraceIdentifier;
            // Sanitiza Path e Method para evitar Log Injection (CWE-117).
            var safePath   = context.Request.Path.Value?.Replace("\n", "").Replace("\r", "") ?? "unknown";
            var safeMethod = context.Request.Method.Replace("\n", "").Replace("\r", "");
            logger.LogError(
                ex,
                "Exceção não tratada. CorrelationId={CorrelationId} Path={Path} Method={Method}",
                correlationId,
                safePath,
                safeMethod
            );

            await WriteErrorAsync(context);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context)
    {
        // Garante que headers não foram enviados antes de tentar escrever a resposta.
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode  = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.Body.WriteAsync(SafeErrorPayload);
    }
}
