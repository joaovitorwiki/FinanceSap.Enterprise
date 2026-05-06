using FinanceSap.Domain.Common;

namespace FinanceSap.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um empréstimo é solicitado.
/// Permite implementar side-effects assíncronos (ex: notificações, análise de crédito, etc.)
/// sem acoplar a entidade Loan a essas responsabilidades.
/// </summary>
public sealed record LoanRequestedEvent : IDomainEvent
{
    /// <summary>
    /// ID do empréstimo solicitado.
    /// </summary>
    public Guid LoanId { get; init; }

    /// <summary>
    /// ID do cliente que solicitou o empréstimo.
    /// </summary>
    public Guid CustomerId { get; init; }

    /// <summary>
    /// Valor principal solicitado.
    /// </summary>
    public decimal PrincipalAmount { get; init; }

    /// <summary>
    /// Taxa de juros anual aplicada.
    /// </summary>
    public decimal InterestRate { get; init; }

    /// <summary>
    /// Número de parcelas.
    /// </summary>
    public int Installments { get; init; }

    /// <summary>
    /// Valor da parcela mensal calculada.
    /// </summary>
    public decimal MonthlyPaymentAmount { get; init; }

    /// <summary>
    /// Timestamp de quando o evento ocorreu.
    /// </summary>
    public DateTime OccurredOn { get; init; }

    public LoanRequestedEvent(
        Guid loanId,
        Guid customerId,
        decimal principalAmount,
        decimal interestRate,
        int installments,
        decimal monthlyPaymentAmount)
    {
        LoanId = loanId;
        CustomerId = customerId;
        PrincipalAmount = principalAmount;
        InterestRate = interestRate;
        Installments = installments;
        MonthlyPaymentAmount = monthlyPaymentAmount;
        OccurredOn = DateTime.UtcNow;
    }
}
