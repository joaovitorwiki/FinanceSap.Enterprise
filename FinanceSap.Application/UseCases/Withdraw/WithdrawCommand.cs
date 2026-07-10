using MediatR;

namespace FinanceSap.Application.UseCases.Withdraw;

public sealed record WithdrawCommand(Guid AccountId, decimal Amount, Guid UserId) : IRequest<Result>;
