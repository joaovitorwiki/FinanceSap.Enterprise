using FinanceSap.Domain.Common;
using FinanceSap.Domain.Enums;
using FinanceSap.Domain.Events;
using FinanceSap.Domain.Interfaces;

namespace FinanceSap.Domain.Entities;

/// <summary>
/// Aggregate Root — Empréstimo.
/// Encapsula todas as invariantes de negócio relacionadas a empréstimos.
/// Usa Strategy Pattern (ILoanCalculator) para permitir diferentes métodos de cálculo.
/// </summary>
public sealed class Loan : BaseEntity
{
    /// <summary>
    /// ID do cliente que solicitou o empréstimo.
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Valor principal do empréstimo (montante solicitado).
    /// </summary>
    public decimal PrincipalAmount { get; private set; }

    /// <summary>
    /// Taxa de juros anual (ex: 0.12 para 12% ao ano).
    /// </summary>
    public decimal InterestRate { get; private set; }

    /// <summary>
    /// Número de parcelas mensais.
    /// </summary>
    public int Installments { get; private set; }

    /// <summary>
    /// Valor da parcela mensal calculada.
    /// </summary>
    public decimal MonthlyPaymentAmount { get; private set; }

    /// <summary>
    /// Valor total a pagar (principal + juros).
    /// </summary>
    public decimal TotalToPay { get; private set; }

    /// <summary>
    /// Status atual do empréstimo.
    /// </summary>
    public LoanStatus Status { get; private set; }

    // ── Navegação (opcional) ─────────────────────────────────────────────────
    public Customer? Customer { get; private set; }

    // ── Construtor privado para EF Core ──────────────────────────────────────
    private Loan() { }

    // ── Factory Method (Named Constructor Pattern) ───────────────────────────
    /// <summary>
    /// Cria um novo empréstimo com cálculo automático de parcelas.
    /// </summary>
    /// <param name="customerId">ID do cliente solicitante.</param>
    /// <param name="principalAmount">Valor principal solicitado.</param>
    /// <param name="interestRate">Taxa de juros anual (ex: 0.12 para 12%).</param>
    /// <param name="installments">Número de parcelas mensais.</param>
    /// <param name="calculator">Strategy para cálculo de parcelas.</param>
    /// <returns>Resultado contendo o Loan criado ou erro de validação.</returns>
    public static Result<Loan> Create(
        Guid customerId,
        decimal principalAmount,
        decimal interestRate,
        int installments,
        ILoanCalculator calculator)
    {
        // ── Validação de Invariantes ─────────────────────────────────────────
        if (customerId == Guid.Empty)
            return Result<Loan>.Failure("Cliente inválido.");

        if (principalAmount <= 0)
            return Result<Loan>.Failure("O valor do empréstimo deve ser maior que zero.");

        if (principalAmount > 1_000_000)
            return Result<Loan>.Failure("O valor máximo de empréstimo é R$ 1.000.000,00.");

        if (interestRate < 0)
            return Result<Loan>.Failure("A taxa de juros não pode ser negativa.");

        if (interestRate > 0.50m) // 50% ao ano
            return Result<Loan>.Failure("A taxa de juros máxima é de 50% ao ano.");

        if (installments <= 0)
            return Result<Loan>.Failure("O número de parcelas deve ser maior que zero.");

        if (installments > 360) // 30 anos
            return Result<Loan>.Failure("O prazo máximo é de 360 meses (30 anos).");

        if (calculator is null)
            return Result<Loan>.Failure("Calculadora de empréstimo não fornecida.");

        // ── Cálculo de Parcelas ──────────────────────────────────────────────
        decimal monthlyPayment;
        decimal totalToPay;

        try
        {
            monthlyPayment = calculator.CalculateInstallment(principalAmount, interestRate, installments);
            totalToPay = calculator.CalculateTotalAmount(principalAmount, interestRate, installments);
        }
        catch (Exception ex)
        {
            return Result<Loan>.Failure($"Erro ao calcular parcelas: {ex.Message}");
        }

        // ── Criação da Entidade ──────────────────────────────────────────────
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PrincipalAmount = principalAmount,
            InterestRate = interestRate,
            Installments = installments,
            MonthlyPaymentAmount = Math.Round(monthlyPayment, 2), // Arredonda para 2 casas decimais
            TotalToPay = Math.Round(totalToPay, 2),
            Status = LoanStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // ── Domain Event ─────────────────────────────────────────────────────
        // Dispara evento para permitir side-effects assíncronos
        // (ex: notificação ao cliente, análise de crédito automática, etc.)
        loan.AddDomainEvent(new LoanRequestedEvent(
            loan.Id,
            loan.CustomerId,
            loan.PrincipalAmount,
            loan.InterestRate,
            loan.Installments,
            loan.MonthlyPaymentAmount
        ));

        return Result<Loan>.Success(loan);
    }

    // ── Métodos de Transição de Estado ───────────────────────────────────────
    /// <summary>
    /// Aprova o empréstimo.
    /// </summary>
    public Result Approve()
    {
        if (Status != LoanStatus.Pending)
            return Result.Failure("Apenas empréstimos pendentes podem ser aprovados.");

        Status = LoanStatus.Approved;
        MarkAsUpdated();

        // Evento de aprovação pode ser adicionado aqui no futuro
        // AddDomainEvent(new LoanApprovedEvent(Id, CustomerId));

        return Result.Success();
    }

    /// <summary>
    /// Rejeita o empréstimo.
    /// </summary>
    /// <param name="reason">Motivo da rejeição (opcional).</param>
    public Result Reject(string? reason = null)
    {
        if (Status != LoanStatus.Pending)
            return Result.Failure("Apenas empréstimos pendentes podem ser rejeitados.");

        Status = LoanStatus.Rejected;
        MarkAsUpdated();

        // Evento de rejeição pode ser adicionado aqui no futuro
        // AddDomainEvent(new LoanRejectedEvent(Id, CustomerId, reason));

        return Result.Success();
    }

    // ── Métodos de Consulta (Query Methods) ──────────────────────────────────
    /// <summary>
    /// Calcula o total de juros pagos ao final do empréstimo.
    /// </summary>
    public decimal GetTotalInterest() => TotalToPay - PrincipalAmount;

    /// <summary>
    /// Calcula a taxa efetiva mensal aplicada.
    /// </summary>
    public decimal GetMonthlyInterestRate()
    {
        return (decimal)Math.Pow((double)(1 + InterestRate), 1.0 / 12.0) - 1;
    }

    /// <summary>
    /// Verifica se o empréstimo está em um estado final (aprovado ou rejeitado).
    /// </summary>
    public bool IsFinal() => Status is LoanStatus.Approved or LoanStatus.Rejected;
}
