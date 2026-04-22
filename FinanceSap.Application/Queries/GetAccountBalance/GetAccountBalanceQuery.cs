using FinanceSap.Domain.Common;
using MediatR;

namespace FinanceSap.Application.Queries.GetAccountBalance;

// Query — GetAccountBalance.
// Retorna o saldo da conta do Customer autenticado.
// UserId é injetado pelo controller a partir do JWT — usado para validação de ownership (IDOR prevention).
public sealed record GetAccountBalanceQuery(Guid UserId) : IRequest<Result<decimal>>;
