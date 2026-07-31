using MediatR;
using FinanceSap.Domain.Common;

namespace FinanceSap.Application.Commands
{
    public class RejectLoanCommand : IRequest<Result>
    {
        public Guid Id { get; }
        public Guid UserId { get; }
        public string? Reason { get; }

        public RejectLoanCommand(Guid id, Guid userId, string? reason = null)
        {
            Id = id;
            UserId = userId;
            Reason = reason;
        }
    }
}