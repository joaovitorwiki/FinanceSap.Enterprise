using FinanceSap.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinanceSap.Tests.Integration;

// Factory base reutilizável por todos os testes de integração.
// Environment Parity: lê configurações via variáveis de ambiente (12-Factor App).
// IAsyncLifetime garante schema atualizado e estado limpo antes de cada teste.
public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // ── Environment Configuration ────────────────────────────────────────────────
        // Define o ambiente como Testing para isolar de Development/Production.
        // O CI/CD injeta ASPNETCORE_ENVIRONMENT=Testing via workflow.
        builder.UseEnvironment(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Testing"
        );

        // ── Database Connection String ─────────────────────────────────────────────
        // Prioridade:
        // 1. ConnectionStrings__DefaultConnection (CI/CD via GitHub Actions)
        // 2. Fallback: localhost (desenvolvimento local com Docker)
        //
        // O double underscore (__) é a notação do .NET para hierarquia de config:
        // ConnectionStrings__DefaultConnection → { "ConnectionStrings": { "DefaultConnection": "..." } }
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=3306;Database=financesap_tests;Uid=root;Pwd=root;";

        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);

        // ── Logging Configuration ──────────────────────────────────────────────────
        // Suprime logs de EF Command em testes para output limpo.
        // O CI/CD pode sobrescrever via Logging__LogLevel__* env vars.
        builder.UseSetting(
            "Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command",
            Environment.GetEnvironmentVariable("Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command") ?? "Warning"
        );

        // ── JWT Configuration (Automatic via Environment Variables) ────────────────────
        // O .NET ConfigurationManager automaticamente lê Jwt__Key, Jwt__Issuer, Jwt__Audience
        // das variáveis de ambiente e mapeia para appsettings:Jwt:*.
        // Não é necessário configurar manualmente aqui — o DependencyInjection.cs
        // já usa configuration["Jwt:Key"], que resolve automaticamente.
    }

    // Executado UMA VEZ antes de todos os testes da classe que usa esta fixture.
    // Aplica migrations para garantir que o schema esteja atualizado.
    public async Task InitializeAsync()
    {
        using var scope  = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }

    // Executado após todos os testes — limpa o banco para não vazar estado entre suítes.
    public new async Task DisposeAsync()
    {
        using var scope  = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.ExecuteSqlRawAsync("DELETE FROM customers");
        await base.DisposeAsync();
    }

    // Método auxiliar para limpar apenas a tabela customers entre testes individuais.
    public async Task ResetCustomersAsync()
    {
        using var scope  = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.ExecuteSqlRawAsync("DELETE FROM customers");
    }
}
