using FinanceSap.Domain.Entities;
using MediatR;

namespace FinanceSap.Application.Queries;

public sealed record GetLoanApplicationByIdQuery(Guid Id, Guid UserId) : IRequest<LoanApplication?>;
