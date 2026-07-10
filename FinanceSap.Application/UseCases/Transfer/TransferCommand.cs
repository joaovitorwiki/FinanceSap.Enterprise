using MediatR;

namespace FinanceSap.Application.UseCases.Transfer;

public sealed record TransferCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    Guid UserId) : IRequest<Result>;
