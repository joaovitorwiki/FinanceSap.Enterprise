using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Enums;
using FinanceSap.Domain.Events;
using FinanceSap.Domain.Services;
using FluentAssertions;
using Xunit;

namespace FinanceSap.Tests.Domain.LoanTests;

/// <summary>
/// Testes unitários para a entidade Loan.
/// Valida invariantes de negócio, cálculos e domain events.
/// </summary>
public sealed class LoanTests
{
    private readonly CompoundInterestCalculator _calculator = new();
    private readonly Guid _customerId = Guid.NewGuid();

    // ─────────────────────────────────────────────────────────────────────────
    // HAPPY PATH — Criação Bem-Sucedida
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Create — Empréstimo válido deve ser criado com sucesso")]
    public void Create_WithValidParameters_ReturnsSuccessWithLoan()
    {
        // Arrange
        decimal principal = 10_000m;
        decimal rate = 0.12m;
        int installments = 12;

        // Act
        var result = Loan.Create(_customerId, principal, rate, installments, _calculator);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().NotBeEmpty();
        result.Value.CustomerId.Should().Be(_customerId);
        result.Value.PrincipalAmount.Should().Be(principal);
        result.Value.InterestRate.Should().Be(rate);
        result.Value.Installments.Should().Be(installments);
        result.Value.Status.Should().Be(LoanStatus.Pending);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Create — Deve calcular parcela mensal corretamente")]
    public void Create_WithValidParameters_CalculatesMonthlyPaymentCorrectly()
    {
        // Arrange
        decimal principal = 10_000m;
        decimal rate = 0.12m;
        int installments = 12;

        // Act
        var result = Loan.Create(_customerId, principal, rate, installments, _calculator);

        // Assert
        result.Value!.MonthlyPaymentAmount.Should().BeApproximately(885.62m, 1m);
        result.Value.TotalToPay.Should().BeApproximately(10_627.45m, 1m);
    }

    [Fact(DisplayName = "Create — Deve disparar LoanRequestedEvent")]
    public void Create_WithValidParameters_RaisesLoanRequestedEvent()
    {
        // Act
        var result = Loan.Create(_customerId, 10_000m, 0.12m, 12, _calculator);

        // Assert
        result.Value!.DomainEvents.Should().HaveCount(1);
        result.Value.DomainEvents.First().Should().BeOfType<LoanRequestedEvent>();

        var domainEvent = (LoanRequestedEvent)result.Value.DomainEvents.First();
        domainEvent.LoanId.Should().Be(result.Value.Id);
        domainEvent.CustomerId.Should().Be(_customerId);
        domainEvent.PrincipalAmount.Should().Be(10_000m);
        domainEvent.InterestRate.Should().Be(0.12m);
        domainEvent.Installments.Should().Be(12);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // VALIDAÇÃO — Invariantes de Negócio
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Create — CustomerId vazio deve retornar falha")]
    public void Create_WithEmptyCustomerId_ReturnsFailure()
    {
        // Act
        var result = Loan.Create(Guid.Empty, 10_000m, 0.12m, 12, _calculator);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cliente inválido");
    }

    [Theory(DisplayName = "Create — Principal inválido deve retornar falha")]
    [InlineData(0)]
    [InlineData(-1000)]
    public void Create_WithInvalidPrincipal_ReturnsFailure(decimal principal)
    {
        // Act
        var result = Loan.Create(_customerId, principal, 0.12m, 12, _calculator);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("maior que zero");
    }

    [Fact(DisplayName = "Create — Principal acima do limite deve retornar falha")]
    public void Create_WithPrincipalAboveLimit_ReturnsFailure()
    {
        // Act
        var result = Loan.Create(_customerId, 1_000_001m, 0.12m, 12, _calculator);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("máximo de empréstimo");
    }

    [Fact(DisplayName = "Create — Taxa negativa deve retornar falha")]
    public void Create_WithNegativeRate_ReturnsFailure()
    {
        // Act
        var result = Loan.Create(_customerId, 10_000m, -0.05m, 12, _calculator);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("taxa de juros");
    }

    [Fact(DisplayName = "Create — Taxa acima de 50% deve retornar falha")]
    public void Create_WithRateAbove50Percent_ReturnsFailure()
    {
        // Act
        var result = Loan.Create(_customerId, 10_000m, 0.51m, 12, _calculator);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("taxa de juros máxima");
    }

    [Theory(DisplayName = "Create — Número de parcelas inválido deve retornar falha")]
    [InlineData(0)]
    [InlineData(-12)]
    [InlineData(361)]
    public void Create_WithInvalidInstallments_ReturnsFailure(int installments)
    {
        // Act
        var result = Loan.Create(_customerId, 10_000m, 0.12m, installments, _calculator);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "Create — Calculator nulo deve retornar falha")]
    public void Create_WithNullCalculator_ReturnsFailure()
    {
        // Act
        var result = Loan.Create(_customerId, 10_000m, 0.12m, 12, null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Calculadora");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TRANSIÇÕES DE ESTADO
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Approve — Empréstimo pendente deve ser aprovado")]
    public void Approve_WhenPending_ChangesStatusToApproved()
    {
        // Arrange
        var loan = Loan.Create(_customerId, 10_000m, 0.12m, 12, _calculator).Value!;

        // Act
        var result = loan.Approve();

        // Assert
        result.IsSuccess.Should().BeTrue();
        loan.Status.Should().Be(LoanStatus.Approved);
        loan.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Approve — Empréstimo já aprovado não pode ser aprovado novamente")]
    public void Approve_WhenAlreadyApproved_ReturnsFailure()
    {
        // Arrange
        var loan = Loan.Create(_customerId, 10_000m, 0.12m, 12, _calculator).Value!;
        loan.Approve();

        // Act
        var result = loan.Approve();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("pendentes");
    }

    [Fact(DisplayName = "Reject — Empréstimo pendente deve ser rejeitado")]
    public void Reject_WhenPending_ChangesStatusToRejected()
    {
        // Arrange
        var loan = Loan.Create(_customerId, 10_000m, 0.12m, 12, _calculator).Value!;

        // Act
        var result = loan.Reject("Score de crédito insuficiente");

        // Assert
        result.IsSuccess.Should().BeTrue();
        loan.Status.Should().Be(LoanStatus.Rejected);
        loan.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Reject — Empréstimo já rejeitado não pode ser rejeitado novamente")]
    public void Reject_WhenAlreadyRejected_ReturnsFailure()
    {
        // Arrange
        var loan = Loan.Create(_customerId, 10_000m, 0.12m, 12, _calculator).Value!;
        loan.Reject();

        // Act
        var result = loan.Reject();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("pendentes");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MÉTODOS DE CONSULTA
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "GetTotalInterest — Deve calcular juros totais corretamente")]
    public void GetTotalInterest_ReturnsCorrectValue()
    {
        // Arrange
        var loan = Loan.Create(_customerId, 10_000m, 0.12m, 12, _calculator).Value!;

        // Act
        var totalInterest = loan.GetTotalInterest();

        // Assert
        // Total a pagar: ~10.627,45 - Principal: 10.000 = Juros: ~627,45
        totalInterest.Should().BeApproximately(627.45m, 1m);
    }

    [Fact(DisplayName = "GetMonthlyInterestRate — Deve calcular taxa mensal corretamente")]
    public void GetMonthlyInterestRate_ReturnsCorrectValue()
    {
        // Arrange
        var loan = Loan.Create(_customerId, 10_000m, 0.12m, 12, _calculator).Value!;

        // Act
        var monthlyRate = loan.GetMonthlyInterestRate();

        // Assert
        // Taxa mensal efetiva de 12% ao ano: ~0.9489% ao mês
        monthlyRate.Should().BeApproximately(0.009489m, 0.0001m);
    }

    [Theory(DisplayName = "IsFinal — Deve identificar estados finais corretamente")]
    [InlineData(LoanStatus.Approved, true)]
    [InlineData(LoanStatus.Rejected, true)]
    [InlineData(LoanStatus.Pending, false)]
    public void IsFinal_ReturnsCorrectValue(LoanStatus status, bool expectedIsFinal)
    {
        // Arrange
        var loan = Loan.Create(_customerId, 10_000m, 0.12m, 12, _calculator).Value!;
        
        if (status == LoanStatus.Approved)
            loan.Approve();
        else if (status == LoanStatus.Rejected)
            loan.Reject();

        // Act
        var isFinal = loan.IsFinal();

        // Assert
        isFinal.Should().Be(expectedIsFinal);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DOMAIN EVENTS
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "ClearDomainEvents — Deve limpar eventos de domínio")]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        // Arrange
        var loan = Loan.Create(_customerId, 10_000m, 0.12m, 12, _calculator).Value!;
        loan.DomainEvents.Should().HaveCount(1);

        // Act
        loan.ClearDomainEvents();

        // Assert
        loan.DomainEvents.Should().BeEmpty();
    }
}
