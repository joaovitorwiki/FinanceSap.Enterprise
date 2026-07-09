using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.Queries;

public sealed class GetLoanByIdQueryHandler(
    ILoanRepository loanRepository,
    IUserContext userContext)
    : IRequestHandler<GetLoanByIdQuery, Loan?>
{
    public async Task<Loan?> Handle(GetLoanByIdQuery request, CancellationToken ct)
    {
        var loan = await loanRepository.GetByIdAsync(request.Id, ct);
        if (loan is null) return null;

        // IDOR: valida que o CustomerId do JWT é dono deste empréstimo.
        // Retorna null (→ 404) se não pertencer ao usuário — não vaza existência do recurso.
        var ownerCustomerId = await userContext.GetCustomerIdByUserIdAsync(request.UserId, ct);
        if (ownerCustomerId is null || ownerCustomerId.Value != loan.CustomerId)
            return null;

        return loan;
    }
}
