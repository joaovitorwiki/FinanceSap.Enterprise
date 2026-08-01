using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using MediatR;

namespace FinanceSap.Application.Queries;

/// <summary>
/// Query para obter todos os empréstimos de um cliente específico.
/// </summary>
public sealed record GetLoansByCustomerQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<Loan>>>;