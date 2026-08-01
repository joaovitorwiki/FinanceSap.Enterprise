using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using MediatR;

namespace FinanceSap.Application.Queries.GetAccountByCustomer;

/// <summary>
/// Query to get the primary account for the authenticated customer.
/// </summary>
public sealed record GetAccountByCustomerQuery(Guid UserId) : IRequest<Result<Account>>;