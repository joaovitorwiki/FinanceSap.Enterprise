using FinanceSap.Domain.Entities;
using MediatR;

namespace FinanceSap.Application.Queries;

public sealed record GetLoanByIdQuery(Guid Id, Guid UserId) : IRequest<Loan?>;
