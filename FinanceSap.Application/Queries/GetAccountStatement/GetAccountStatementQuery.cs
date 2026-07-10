using FinanceSap.Domain.Entities;
using MediatR;

namespace FinanceSap.Application.Queries.GetAccountStatement;

public sealed record GetAccountStatementQuery(
    Guid AccountId,
    Guid UserId,
    int  Page     = 1,
    int  PageSize = 20) : IRequest<Result<AccountStatementResult>>;

public sealed record AccountStatementResult(
    Guid                      AccountId,
    decimal                   Balance,
    int                       TotalCount,
    int                       Page,
    int                       PageSize,
    IReadOnlyList<Transaction> Transactions);
