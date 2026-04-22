namespace FinanceSap.Api.Middlewares;

// Security Headers Middleware — OWASP A05 / OWASP A03.
// Aplicado globalmente: cobre 100% das respostas, incluindo 404, 429 e 500.
// Detecta endpoints de documentação (Scalar/OpenAPI) e aplica CSP relaxada apenas para eles.
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private static readonly string[] DocumentationPaths =
    [
        "/openapi/",
        "/scalar/"
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // Detecta se é requisição para documentação (Scalar ou OpenAPI spec).
        var isDocumentation = DocumentationPaths.Any(docPath => path.StartsWith(docPath));

        // Headers comuns — aplicados em TODAS as respostas.
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "no-referrer";
        headers["X-XSS-Protection"] = "0";

        // Remove headers que revelam stack tecnológico (fingerprinting).
        headers.Remove("Server");
        headers.Remove("X-Powered-By");
        headers.Remove("X-AspNet-Version");
        headers.Remove("X-AspNetMvc-Version");

        if (isDocumentation)
        {
            // CSP relaxada para Scalar — permite inline scripts/styles e CDNs necessários.
            // Scalar precisa de 'unsafe-inline', 'unsafe-eval' e acesso aos CDNs.
            headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' cdn.jsdelivr.net unpkg.com; " +
                "style-src 'self' 'unsafe-inline' cdn.jsdelivr.net fonts.googleapis.com; " +
                "font-src 'self' fonts.gstatic.com cdn.jsdelivr.net; " +
                "img-src 'self' data: cdn.jsdelivr.net; " +
                "connect-src 'self'";

            // Permite iframe apenas para documentação (alguns viewers precisam).
            headers["X-Frame-Options"] = "SAMEORIGIN";
        }
        else
        {
            // CSP estrita para API JSON — bloqueia tudo por padrão.
            // APIs puras não servem HTML/JS — CSP restritiva não impacta funcionalidade.
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

            // Bloqueia renderização em iframe — mitiga clickjacking (OWASP A05).
            headers["X-Frame-Options"] = "DENY";
        }

        await next(context);
    }
}
