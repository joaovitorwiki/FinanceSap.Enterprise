using FinanceSap.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinanceSap.Tests.Integration;

// Factory base reutilizável por todos os testes de integração.
// Aponta para o MySQL real do Docker — sem mocks de banco.
// IAsyncLifetime garante schema atualizado e estado limpo antes de cada teste.
public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Connection string para o MySQL rodando no Docker.
    // Em CI/CD, sobrescreva via variável de ambiente TEST_DB_CONNECTION.
    private const string TestConnectionString =
        "Server=localhost;Port=3306;Database=financesap_tests;Uid=root;Pwd=root;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Sobrescreve a connection string da aplicação pela de testes —
        // banco isolado (financesap_tests) para não contaminar dados de produção/dev.
        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            TestConnectionString
        );

        // Suprime logs de EF Command em testes para output limpo.
        builder.UseSetting(
            "Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command",
            "Warning"
        );
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
