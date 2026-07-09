using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.Queries;

public sealed class GetCustomerByIdQueryHandler(
    ICustomerRepository customerRepository,
    IUserContext userContext)
    : IRequestHandler<GetCustomerByIdQuery, Customer?>
{
    public async Task<Customer?> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        // IDOR: valida que o CustomerId do JWT corresponde ao recurso solicitado.
        // Retorna null (→ 404) se não pertencer ao usuário — não vaza existência do recurso.
        var ownerCustomerId = await userContext.GetCustomerIdByUserIdAsync(request.UserId, ct);
        if (ownerCustomerId is null || ownerCustomerId.Value != request.Id)
            return null;

        return await customerRepository.GetByIdAsync(request.Id, ct);
    }
}
