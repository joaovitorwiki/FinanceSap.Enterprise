using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.Queries;

public sealed class GetLoanApplicationByIdQueryHandler(
    ILoanApplicationRepository repository,
    IUserContext userContext)
    : IRequestHandler<GetLoanApplicationByIdQuery, LoanApplication?>
{
    public async Task<LoanApplication?> Handle(GetLoanApplicationByIdQuery request, CancellationToken ct)
    {
        var application = await repository.GetByIdAsync(request.Id, ct);
        if (application is null) return null;

        // IDOR: valida que o CustomerId do JWT é dono desta solicitação.
        var ownerCustomerId = await userContext.GetCustomerIdByUserIdAsync(request.UserId, ct);
        if (ownerCustomerId is null || ownerCustomerId.Value != application.CustomerId)
            return null;

        return application;
    }
}
