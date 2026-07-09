using FinanceSap.Domain.Common;
using MediatR;

namespace FinanceSap.Application.UseCases.ApproveLoan;

public sealed record ApproveLoanCommand(Guid LoanId, Guid UserId) : IRequest<Result>;
