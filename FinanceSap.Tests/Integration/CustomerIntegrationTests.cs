using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FinanceSap.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace FinanceSap.Tests.Integration;

// Suíte de testes de integração de alta fidelidade para POST /api/customers.
// Usa MySQL real (Docker) — sem mocks de banco — para máxima fidelidade ao ambiente de produção.
// IClassFixture<T>: a factory é criada uma vez e compartilhada entre todos os testes da classe.
[Collection("Integration")]
public sealed class CustomerIntegrationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    // CPFs matematicamente válidos reservados para esta suíte.
    private const string ValidCpf         = "52998224725";
    private const string ValidCpfAlt      = "11144477735";
    private const string ValidCpfFormatted = "529.982.247-25";

    // -------------------------------------------------------------------------
    // SETUP / TEARDOWN
    // -------------------------------------------------------------------------

    // Garante estado limpo antes de cada teste individual — evita acoplamento entre testes.
    public Task InitializeAsync() => factory.ResetCustomersAsync();
    public Task DisposeAsync()    => Task.CompletedTask;

    // -------------------------------------------------------------------------
    // HAPPY PATH — Criação bem-sucedida
    // -------------------------------------------------------------------------

    [Fact(DisplayName = "POST /api/customers — CPF válido deve retornar 201 Created e persistir no banco")]
    public async Task Create_WithValidPayload_Returns201AndPersistsToDatabase()
    {
        // Arrange
        var payload = new { document = ValidCpf, fullName = "João da Silva" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", payload);

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            because: "um payload válido deve resultar em criação bem-sucedida");

        response.Headers.Location.Should().NotBeNull(
            because: "RFC 7231 exige Location header no 201 Created");

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id   = body.GetProperty("id").GetGuid();
        id.Should().NotBeEmpty(because: "o ID do recurso criado deve ser retornado no body");

        // Assert — Persistência real no banco
        using var scope   = factory.Services.CreateScope();
        var       context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customer = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        customer.Should().NotBeNull(because: "o cliente deve ter sido persistido no banco de dados");
        customer!.FullName.Should().Be("João da Silva");
        customer.Document.Value.Should().Be(ValidCpf);
    }

    [Fact(DisplayName = "POST /api/customers — CPF formatado deve ser aceito e normalizado")]
    public async Task Create_WithFormattedCpf_Returns201AndStoresOnlyDigits()
    {
        // Arrange
        var payload = new { document = ValidCpfFormatted, fullName = "Maria Oliveira" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", payload);

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id   = body.GetProperty("id").GetGuid();

        // Assert — Banco armazena apenas dígitos (sem formatação)
        using var scope   = factory.Services.CreateScope();
        var       context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customer = await context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        customer!.Document.Value.Should().Be(ValidCpf,
            because: "o CPF deve ser normalizado (apenas dígitos) antes da persistência");
    }

    // -------------------------------------------------------------------------
    // SEGURANÇA — Idempotência / Prevenção de duplicidade
    // -------------------------------------------------------------------------

    [Fact(DisplayName = "POST /api/customers — CPF duplicado deve retornar 409 Conflict")]
    public async Task Create_WithDuplicateCpf_Returns409Conflict()
    {
        // Arrange — cria o primeiro registro
        var payload = new { document = ValidCpf, fullName = "Carlos Pereira" };
        var first   = await _client.PostAsJsonAsync("/api/customers", payload);
        first.StatusCode.Should().Be(HttpStatusCode.Created, because: "o primeiro cadastro deve ser aceito");

        // Act — tenta criar com o mesmo CPF
        var duplicate = await _client.PostAsJsonAsync("/api/customers", payload);

        // Assert
        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict,
            because: "CPF duplicado viola a integridade financeira — deve retornar 409 Conflict");

        var problem = await duplicate.Content.ReadFromJsonAsync<JsonElement>();

        problem.GetProperty("status").GetInt32().Should().Be(409);
        problem.GetProperty("detail").GetString().Should().Contain("CPF",
            because: "a mensagem de erro deve identificar o motivo do conflito");

        // Garante que apenas UM registro existe no banco
        using var scope   = factory.Services.CreateScope();
        var       context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var       count   = await context.Customers.CountAsync(c => c.Document == ValidCpf);
        count.Should().Be(1, because: "a tentativa duplicada não deve criar um segundo registro");
    }

    // -------------------------------------------------------------------------
    // SEGURANÇA — Input Sanitization / Prevenção de XSS
    // -------------------------------------------------------------------------

    [Theory(DisplayName = "POST /api/customers — Payloads maliciosos devem ser rejeitados ou neutralizados")]
    [InlineData("<script>alert(1)</script>",          "XSS via script tag")]
    [InlineData("'; DROP TABLE customers; --",        "SQL Injection attempt")]
    [InlineData("<img src=x onerror=alert(1)>",       "XSS via img onerror")]
    [InlineData("javascript:alert(document.cookie)",  "XSS via javascript protocol")]
    public async Task Create_WithMaliciousFullName_DoesNotExecuteAndHandlesSafely(
        string maliciousInput, string scenario)
    {
        // Arrange
        var payload = new { document = ValidCpfAlt, fullName = maliciousInput };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", payload);

        // Assert — O sistema NUNCA deve retornar 500 para inputs maliciosos.
        // Aceitar (201) ou rejeitar (400) são ambos comportamentos seguros —
        // o que não é aceitável é executar o script ou expor detalhes internos.
        response.StatusCode.Should().BeOneOf(
            [HttpStatusCode.Created, HttpStatusCode.BadRequest],
            because: $"[{scenario}] o sistema deve tratar o input sem falhar com 500");

        var rawBody = await response.Content.ReadAsStringAsync();

        // Garante que o input malicioso não é refletido de volta sem tratamento
        // (proteção contra Reflected XSS na mensagem de erro)
        rawBody.Should().NotContain("<script>",
            because: $"[{scenario}] tags de script não devem ser refletidas na resposta");
        rawBody.Should().NotContain("onerror=",
            because: $"[{scenario}] atributos de evento não devem ser refletidos na resposta");

        // Se foi aceito (201), verifica que o valor foi armazenado como texto literal
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            var id   = body.GetProperty("id").GetGuid();

            using var scope   = factory.Services.CreateScope();
            var       context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var       customer = await context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

            // O valor armazenado deve ser o texto literal — nunca executado
            customer!.FullName.Should().Be(maliciousInput,
                because: "a camada de persistência armazena texto literal — a sanitização ocorre na camada de apresentação (output encoding)");
        }

        // Cleanup para não interferir em outros InlineData
        await factory.ResetCustomersAsync();
    }

    // -------------------------------------------------------------------------
    // RESILIÊNCIA — Falha de banco sem Information Exposure
    // -------------------------------------------------------------------------

    [Fact(DisplayName = "POST /api/customers — Falha de banco deve retornar 500 sem expor StackTrace")]
    public async Task Create_WhenDatabaseIsUnavailable_Returns500WithoutStackTrace()
    {
        // Arrange — cria um client com connection string inválida para simular falha de banco
        var brokenFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host =>
            {
                host.UseEnvironment("Testing");
                host.UseSetting(
                    "ConnectionStrings:DefaultConnection",
                    // Porta inexistente — garante falha de conexão imediata
                    "Server=localhost;Port=19999;Database=financesap_tests;Uid=root;Pwd=root;"
                );
            });

        var brokenClient = brokenFactory.CreateClient();
        var payload      = new { document = ValidCpf, fullName = "Teste Falha" };

        // Act
        var response = await brokenClient.PostAsJsonAsync("/api/customers", payload);

        // Assert — HTTP
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError,
            because: "falha de infraestrutura deve resultar em 500 Internal Server Error");

        // Assert — Information Exposure Prevention (OWASP A05)
        var rawBody = await response.Content.ReadAsStringAsync();

        rawBody.Should().NotContain("StackTrace",
            because: "stack traces expõem detalhes internos da aplicação (OWASP A05)");
        rawBody.Should().NotContain("at FinanceSap",
            because: "namespaces internos não devem vazar para o cliente");
        rawBody.Should().NotContain("MySql",
            because: "detalhes do driver de banco não devem ser expostos");
        rawBody.Should().NotContain("Exception",
            because: "nomes de exceções internas não devem vazar para o cliente");

        // Assert — RFC 7807 Problem Details com mensagem genérica
        // Usa TryGetProperty para evitar KeyNotFoundException se a propriedade não existir
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (problem.TryGetProperty("status", out var statusProp))
        {
            statusProp.GetInt32().Should().Be(500);
        }

        if (problem.TryGetProperty("title", out var titleProp))
        {
            titleProp.GetString().Should().Be(
                "Ocorreu um erro interno no servidor.",
                because: "o título deve ser genérico e não revelar detalhes técnicos");
        }

        if (problem.TryGetProperty("detail", out var detailProp))
        {
            detailProp.GetString().Should().Be(
                "Um erro inesperado ocorreu. Contate o suporte.",
                because: "o detail deve ser uma mensagem segura e padronizada");
        }

        await brokenFactory.DisposeAsync();
    }

    // -------------------------------------------------------------------------
    // VALIDAÇÃO DE ENTRADA — Campos obrigatórios
    // -------------------------------------------------------------------------

    [Theory(DisplayName = "POST /api/customers — Campos inválidos devem retornar 400 Bad Request")]
    [InlineData("",            "João Silva",  "CPF vazio")]
    [InlineData("12345678901", "",            "Nome vazio")]
    [InlineData("00000000000", "João Silva",  "CPF com sequência homogênea")]
    [InlineData("1234567890",  "João Silva",  "CPF com 10 dígitos")]
    public async Task Create_WithInvalidFields_Returns400BadRequest(
        string document, string fullName, string scenario)
    {
        // Arrange
        var payload = new { document, fullName };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: $"[{scenario}] dados inválidos devem ser rejeitados com 400");

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("status").GetInt32().Should().Be(400);
    }
}
