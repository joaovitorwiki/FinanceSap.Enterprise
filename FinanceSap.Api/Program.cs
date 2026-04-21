using FinanceSap.Api.Extensions;
using FinanceSap.Api.Middlewares;
using FinanceSap.Application;
using FinanceSap.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Serviços ──────────────────────────────────────────────────────────────────

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Rate Limiter + configurações de segurança da camada API.
builder.Services.AddApiSecurity(builder.Configuration);

// CORS registrado no container — política configurável por ambiente.
// Em produção, substitua AllowAnyOrigin por origens explícitas.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()));

// ── Pipeline ──────────────────────────────────────────────────────────────────
// A ORDEM É CRÍTICA para segurança:
// 1. CORS         — negocia origens antes de qualquer processamento
// 2. RateLimiter  — rejeita abuso o mais cedo possível, antes de tocar a aplicação
// 3. SecHeaders   — aplica headers em TODAS as respostas, incluindo 429 e 500
// 4. Exception    — captura qualquer exceção dos middlewares seguintes
// 5. HTTPS        — redireciona antes de processar a rota
// 6. Rotas        — processa a requisição legítima

var app = builder.Build();

app.UseCors();
app.UseRateLimiter();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Expõe o entry point para WebApplicationFactory<Program> nos testes de integração.
public partial class Program;
