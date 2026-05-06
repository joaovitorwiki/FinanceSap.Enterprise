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
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"financesap_test_{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_dbPath}");
        builder.UseSetting("Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command", "Warning");
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch (IOException) { }
    }

    public async Task ResetCustomersAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.ExecuteSqlRawAsync("DELETE FROM customers");
    }
}
