using FinanceSap.Domain.Common;

namespace FinanceSap.Domain.Events;

public sealed record LoanApprovedEvent(
    Guid    LoanId,
    Guid    CustomerId,
    decimal Amount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
