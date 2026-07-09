using FinanceSap.Domain.Common;
using MediatR;

namespace FinanceSap.Application.UseCases.RejectLoan;

public sealed record RejectLoanCommand(Guid LoanId, Guid UserId, string? Reason = null) : IRequest<Result>;
