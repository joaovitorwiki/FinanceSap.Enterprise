namespace FinanceSap.Api.Middlewares;

// Security Headers Middleware — OWASP A05 / OWASP A03.
// Aplicado globalmente: cobre 100% das respostas, incluindo 404, 429 e 500.
// Cada header tem justificativa de segurança explícita.
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Impede que o browser interprete o Content-Type de forma diferente do declarado.
        // Mitiga ataques de MIME-sniffing (OWASP A03).
        headers["X-Content-Type-Options"] = "nosniff";

        // Bloqueia renderização em iframe — mitiga clickjacking (OWASP A05).
        headers["X-Frame-Options"] = "DENY";

        // Política de referrer: não envia a URL de origem em requisições cross-origin.
        // Reduz vazamento de informações de navegação.
        headers["Referrer-Policy"] = "no-referrer";

        // Desativa o filtro XSS legado do IE/Edge antigo.
        // O valor "1" pode criar vulnerabilidades em alguns browsers — "0" é o padrão moderno.
        headers["X-XSS-Protection"] = "0";

        // CSP mínima segura para uma API JSON:
        // - default-src 'none': bloqueia tudo por padrão
        // - frame-ancestors 'none': reforça anti-clickjacking (complementa X-Frame-Options)
        // APIs puras não servem HTML/JS — CSP restritiva não impacta funcionalidade.
        headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

        // Remove headers que revelam stack tecnológico (fingerprinting).
        // Dificulta reconhecimento da infraestrutura por atacantes.
        headers.Remove("Server");
        headers.Remove("X-Powered-By");
        headers.Remove("X-AspNet-Version");
        headers.Remove("X-AspNetMvc-Version");

        await next(context);
    }
}
