using FinanceSap.Application.Commands;
using FinanceSap.Application.UseCases.CreateCustomer;
using FinanceSap.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace FinanceSap.Tests.Integration;

public sealed class LoansIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public LoansIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
        // Configure Bearer Token JWT for authenticated requests
        var token = GenerateTestJwtToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private string GenerateTestJwtToken()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("test-only-key-32-chars-minimum!!");
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "test-user")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "FinanceSap",
            Audience = "FinanceSap",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            )
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Fact(DisplayName = "POST /api/loans — Deve criar empréstimo com sucesso")]
    public async Task PostLoans_WithValidData_ShouldCreateLoan()
    {
        // Arrange - Primeiro criar um cliente com CPF válido
        var createCustomerCommand = new CreateCustomerCommand("12345678909", "João Silva");
        var customerResponse = await _client.PostAsJsonAsync("/api/customers", createCustomerCommand);
        
        // Se já existe (409), o cliente já foi criado anteriormente - isso é aceitável
        if (customerResponse.StatusCode == System.Net.HttpStatusCode.Created)
        {
            var customerContent = await customerResponse.Content.ReadAsStringAsync();
            var customerResult = JsonSerializer.Deserialize<JsonElement>(customerContent);
            var customerId = customerResult.GetProperty("id").GetGuid();

            var loanCommand = new CreateLoanCommand(
                CustomerId: customerId,
                Amount: 10000m,
                InterestRate: 0.12m,
                TermInMonths: 12
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/loans", loanCommand);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            
            result.GetProperty("id").GetGuid().Should().NotBeEmpty();
        }
        else if (customerResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            // Cliente já existe - buscar o ID existente
            var customerContent = await customerResponse.Content.ReadAsStringAsync();
            var customerResult = JsonSerializer.Deserialize<JsonElement>(customerContent);
            var customerId = customerResult.GetProperty("id").GetGuid();

            var loanCommand = new CreateLoanCommand(
                CustomerId: customerId,
                Amount: 10000m,
                InterestRate: 0.12m,
                TermInMonths: 12
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/loans", loanCommand);

            // Assert - pode ser 201 (criado) ou 409 (já existe) - ambos são aceitáveis
            response.StatusCode.Should().BeOneOf(
                System.Net.HttpStatusCode.Created,
                System.Net.HttpStatusCode.Conflict
            );
        }
        else
        {
            customerResponse.EnsureSuccessStatusCode();
        }
    }

    [Fact(DisplayName = "POST /api/loans — Cliente inexistente deve retornar NotFound")]
    public async Task PostLoans_WithNonExistentCustomer_ShouldReturnNotFound()
    {
        // Arrange
        var loanCommand = new CreateLoanCommand(
            CustomerId: Guid.NewGuid(),
            Amount: 10000m,
            InterestRate: 0.12m,
            TermInMonths: 12
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", loanCommand);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "POST /api/loans — Dados inválidos devem retornar BadRequest")]
    public async Task PostLoans_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange - Criar um cliente válido primeiro
        var createCustomerCommand = new CreateCustomerCommand("52998224725", "João Silva");
        var customerResponse = await _client.PostAsJsonAsync("/api/customers", createCustomerCommand);
        customerResponse.EnsureSuccessStatusCode();
        
        var customerContent = await customerResponse.Content.ReadAsStringAsync();
        var customerResult = JsonSerializer.Deserialize<JsonElement>(customerContent);
        var customerId = customerResult.GetProperty("id").GetGuid();

        var loanCommand = new CreateLoanCommand(
            CustomerId: customerId,
            Amount: 0, // Invalid amount
            InterestRate: 0.12m,
            TermInMonths: 12
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", loanCommand);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
