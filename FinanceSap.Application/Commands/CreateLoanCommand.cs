using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using MediatR;

namespace FinanceSap.Application.Commands
{
    /// <summary>
    /// Command to create a new loan request.
    /// Returns a Result containing the created Loan on success, or an error on failure.
    /// </summary>
    public sealed record CreateLoanCommand(
        Guid CustomerId,
        decimal Amount,
        decimal InterestRate,
        int TermInMonths
    ) : IRequest<Result<Loan>>;
}