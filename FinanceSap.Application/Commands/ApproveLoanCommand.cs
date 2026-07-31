using MediatR;
using FinanceSap.Domain.Common;

namespace FinanceSap.Application.Commands
{
    public class ApproveLoanCommand : IRequest<Result>
    {
        public Guid Id { get; }
        public Guid UserId { get; }

        public ApproveLoanCommand(Guid id, Guid userId)
        {
            Id = id;
            UserId = userId;
        }
    }
}