using FinanceSap.Domain.Entities;
using MediatR;

namespace FinanceSap.Application.Queries;

public sealed record GetCustomerByIdQuery(Guid Id, Guid UserId) : IRequest<Customer?>;
