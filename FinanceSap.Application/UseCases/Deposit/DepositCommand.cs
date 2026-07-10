using MediatR;

namespace FinanceSap.Application.UseCases.Deposit;

public sealed record DepositCommand(Guid AccountId, decimal Amount) : IRequest<Result>;
