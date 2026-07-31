using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Common;
using MediatR;

namespace FinanceSap.Application.Queries
{
    public sealed record GetLoanByIdQuery(Guid Id, Guid UserId, bool IsAdmin) : IRequest<Result<Loan>>;
}
