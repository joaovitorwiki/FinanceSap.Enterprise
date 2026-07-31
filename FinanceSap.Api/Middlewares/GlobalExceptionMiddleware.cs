using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSap.Api.Middlewares;

// Error Shielding Middleware — OWASP A05 (Security Misconfiguration).
// Regra absoluta: nenhum detalhe interno chega ao cliente.
// O log interno recebe a exceção completa para auditoria e diagnóstico.
public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
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

            await WriteProblemDetailsAsync(context, ex);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception ex)
    {
        // Garante que headers não foram enviados antes de tentar escrever a resposta.
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Title = "Ocorreu um erro interno no servidor.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Um erro inesperado ocorreu. Contate o suporte.",
            Instance = context.Request.Path,
            Extensions = { ["traceId"] = context.TraceIdentifier ?? "unknown" }
        };

        // Para exceções de validação/domínio, usar status code apropriado
        if (ex is ArgumentException or InvalidOperationException)
        {
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Requisição inválida";
            problemDetails.Detail = ex.Message;
        }

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
