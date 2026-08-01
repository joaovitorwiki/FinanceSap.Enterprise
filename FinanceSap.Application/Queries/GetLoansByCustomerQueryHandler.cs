using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.Queries;

/// <summary>
/// Handler para obter todos os empréstimos de um cliente específico.
/// </summary>
public sealed class GetLoansByCustomerQueryHandler(ILoanRepository loanRepository)
    : IRequestHandler<GetLoansByCustomerQuery, Result<IReadOnlyList<Loan>>>
{
    public async Task<Result<IReadOnlyList<Loan>>> Handle(GetLoansByCustomerQuery request, CancellationToken cancellationToken)
    {
        var loans = await loanRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        if (loans.Count == 0)
        {
            return Result<IReadOnlyList<Loan>>.Success(loans);
        }

        return Result<IReadOnlyList<Loan>>.Success(loans);
    }
}