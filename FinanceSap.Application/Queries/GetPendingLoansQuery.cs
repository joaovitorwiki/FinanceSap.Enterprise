using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using MediatR;

namespace FinanceSap.Application.Queries;

/// <summary>
/// Query para obter todos os empréstimos pendentes (para admin/manager).
/// </summary>
public sealed record GetPendingLoansQuery : IRequest<Result<IReadOnlyList<Loan>>>;