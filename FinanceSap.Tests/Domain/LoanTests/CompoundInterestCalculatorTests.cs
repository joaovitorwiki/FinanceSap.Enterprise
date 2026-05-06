using FinanceSap.Domain.Services;
using FluentAssertions;
using Xunit;

namespace FinanceSap.Tests.Domain.LoanTests;

/// <summary>
/// Testes unitários para CompoundInterestCalculator.
/// Valida a fórmula da Tabela Price e casos extremos.
/// </summary>
public sealed class CompoundInterestCalculatorTests
{
    private readonly CompoundInterestCalculator _calculator = new();

    // ─────────────────────────────────────────────────────────────────────────
    // HAPPY PATH — Cálculos Válidos
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "CalculateInstallment — Empréstimo de R$ 10.000 a 12% ao ano em 12 meses")]
    public void CalculateInstallment_WithValidParameters_ReturnsCorrectValue()
    {
        // Arrange
        decimal principal = 10_000m;
        decimal annualRate = 0.12m; // 12% ao ano
        int months = 12;

        // Act
        var installment = _calculator.CalculateInstallment(principal, annualRate, months);

        // Assert
        // Valor calculado pela fórmula da Tabela Price: ~R$ 885,62
        installment.Should().BeApproximately(885.62m, 1m,
            because: "a fórmula da Tabela Price deve calcular corretamente");
    }

    [Fact(DisplayName = "CalculateInstallment — Taxa zero deve retornar divisão simples")]
    public void CalculateInstallment_WithZeroRate_ReturnsPrincipalDividedByMonths()
    {
        // Arrange
        decimal principal = 12_000m;
        decimal annualRate = 0m; // Sem juros
        int months = 12;

        // Act
        var installment = _calculator.CalculateInstallment(principal, annualRate, months);

        // Assert
        installment.Should().Be(1_000m,
            because: "empréstimo sem juros deve dividir o principal igualmente");
    }

    [Fact(DisplayName = "CalculateTotalAmount — Deve retornar soma de todas as parcelas")]
    public void CalculateTotalAmount_WithValidParameters_ReturnsTotalPayment()
    {
        // Arrange
        decimal principal = 10_000m;
        decimal annualRate = 0.12m;
        int months = 12;

        // Act
        var totalAmount = _calculator.CalculateTotalAmount(principal, annualRate, months);

        // Assert
        // Total calculado: ~R$ 10.627,45 (principal + juros)
        totalAmount.Should().BeApproximately(10_627.45m, 1m);
        totalAmount.Should().BeGreaterThan(principal,
            because: "o total deve incluir juros");
    }

    [Theory(DisplayName = "CalculateInstallment — Diferentes cenários de empréstimo")]
    [InlineData(5_000, 0.10, 6, 856.75)]   // R$ 5k, 10% a.a., 6 meses
    [InlineData(20_000, 0.15, 24, 960.80)] // R$ 20k, 15% a.a., 24 meses
    [InlineData(50_000, 0.08, 36, 1_560.39)] // R$ 50k, 8% a.a., 36 meses
    public void CalculateInstallment_WithDifferentScenarios_ReturnsExpectedValues(
        decimal principal, decimal annualRate, int months, decimal expectedInstallment)
    {
        // Act
        var installment = _calculator.CalculateInstallment(principal, annualRate, months);

        // Assert
        installment.Should().BeApproximately(expectedInstallment, 1m,
            because: "o cálculo deve ser consistente com a Tabela Price");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // VALIDAÇÃO — Parâmetros Inválidos
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "CalculateInstallment — Principal zero deve lançar exceção")]
    public void CalculateInstallment_WithZeroPrincipal_ThrowsArgumentException()
    {
        // Act
        var act = () => _calculator.CalculateInstallment(0m, 0.12m, 12);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*principal*maior que zero*");
    }

    [Fact(DisplayName = "CalculateInstallment — Principal negativo deve lançar exceção")]
    public void CalculateInstallment_WithNegativePrincipal_ThrowsArgumentException()
    {
        // Act
        var act = () => _calculator.CalculateInstallment(-1000m, 0.12m, 12);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*principal*maior que zero*");
    }

    [Fact(DisplayName = "CalculateInstallment — Taxa negativa deve lançar exceção")]
    public void CalculateInstallment_WithNegativeRate_ThrowsArgumentException()
    {
        // Act
        var act = () => _calculator.CalculateInstallment(10_000m, -0.05m, 12);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*taxa de juros*negativa*");
    }

    [Fact(DisplayName = "CalculateInstallment — Taxa acima de 100% deve lançar exceção")]
    public void CalculateInstallment_WithRateAbove100Percent_ThrowsArgumentException()
    {
        // Act
        var act = () => _calculator.CalculateInstallment(10_000m, 1.5m, 12);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*taxa de juros*100%*");
    }

    [Fact(DisplayName = "CalculateInstallment — Meses zero deve lançar exceção")]
    public void CalculateInstallment_WithZeroMonths_ThrowsArgumentException()
    {
        // Act
        var act = () => _calculator.CalculateInstallment(10_000m, 0.12m, 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*meses*maior que zero*");
    }

    [Fact(DisplayName = "CalculateInstallment — Meses acima de 360 deve lançar exceção")]
    public void CalculateInstallment_WithMonthsAbove360_ThrowsArgumentException()
    {
        // Act
        var act = () => _calculator.CalculateInstallment(10_000m, 0.12m, 361);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*prazo máximo*360*");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CASOS EXTREMOS
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "CalculateInstallment — Empréstimo de 1 mês deve retornar principal + juros")]
    public void CalculateInstallment_WithOneMonth_ReturnsCorrectValue()
    {
        // Arrange
        decimal principal = 1_000m;
        decimal annualRate = 0.12m;
        int months = 1;

        // Act
        var installment = _calculator.CalculateInstallment(principal, annualRate, months);

        // Assert
        // Em 1 mês, a parcela é praticamente o principal + juros de 1 mês
        installment.Should().BeGreaterThan(principal);
        installment.Should().BeLessThan(principal * 1.02m,
            because: "juros de 1 mês devem ser pequenos");
    }

    [Fact(DisplayName = "CalculateInstallment — Empréstimo de 360 meses (limite máximo)")]
    public void CalculateInstallment_With360Months_ReturnsValidValue()
    {
        // Arrange
        decimal principal = 100_000m;
        decimal annualRate = 0.10m;
        int months = 360;

        // Act
        var installment = _calculator.CalculateInstallment(principal, annualRate, months);

        // Assert
        installment.Should().BeGreaterThan(0);
        installment.Should().BeLessThan(principal,
            because: "parcela mensal deve ser menor que o principal em prazos longos");
    }
}
