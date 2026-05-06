using FinanceSap.Application.UseCases.CreateCustomer;
using FinanceSap.Application.UseCases.RequestLoan;
using FinanceSap.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;
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
    }

    [Fact(DisplayName = "POST /api/loans — Deve criar empréstimo com sucesso")]
    public async Task PostLoans_WithValidData_ShouldCreateLoan()
    {
        // Arrange - Primeiro criar um cliente
        var createCustomerCommand = new CreateCustomerCommand("52998224725", "João Silva");
        var customerResponse = await _client.PostAsJsonAsync("/api/customers", createCustomerCommand);
        customerResponse.EnsureSuccessStatusCode();
        
        var customerContent = await customerResponse.Content.ReadAsStringAsync();
        var customerResult = JsonSerializer.Deserialize<JsonElement>(customerContent);
        var customerId = customerResult.GetProperty("id").GetGuid();

        var loanCommand = new RequestLoanCommand(
            CustomerId: customerId,
            Amount: 10000m,
            Installments: 12,
            AnnualRate: 0.12m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", loanCommand);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("loanId").GetGuid().Should().NotBeEmpty();
    }

    [Fact(DisplayName = "POST /api/loans — Cliente inexistente deve retornar NotFound")]
    public async Task PostLoans_WithNonExistentCustomer_ShouldReturnNotFound()
    {
        // Arrange
        var loanCommand = new RequestLoanCommand(
            CustomerId: Guid.NewGuid(),
            Amount: 10000m,
            Installments: 12,
            AnnualRate: 0.12m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", loanCommand);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "POST /api/loans — Dados inválidos devem retornar BadRequest")]
    public async Task PostLoans_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var loanCommand = new RequestLoanCommand(
            CustomerId: Guid.NewGuid(),
            Amount: 0, // Invalid amount
            Installments: 12,
            AnnualRate: 0.12m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/loans", loanCommand);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}