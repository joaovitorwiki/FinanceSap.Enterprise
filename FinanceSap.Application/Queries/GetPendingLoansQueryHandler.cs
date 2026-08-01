using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.Queries;

/// <summary>
/// Handler para obter todos os empréstimos pendentes (para admin/manager).
/// </summary>
public sealed class GetPendingLoansQueryHandler(ILoanRepository loanRepository)
    : IRequestHandler<GetPendingLoansQuery, Result<IReadOnlyList<Loan>>>
{
    public async Task<Result<IReadOnlyList<Loan>>> Handle(GetPendingLoansQuery request, CancellationToken cancellationToken)
    {
        var loans = await loanRepository.GetPendingLoansAsync(cancellationToken);
        return Result<IReadOnlyList<Loan>>.Success(loans);
    }
}