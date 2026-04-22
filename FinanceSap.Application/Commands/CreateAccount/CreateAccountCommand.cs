using FinanceSap.Domain.Common;
using MediatR;

namespace FinanceSap.Application.Commands.CreateAccount;

// Command — CreateAccount.
// Implementa IRequest<Result<Guid>> do MediatR — retorna o ID da conta criada ou falha.
public sealed record CreateAccountCommand(Guid CustomerId) : IRequest<Result<Guid>>;
