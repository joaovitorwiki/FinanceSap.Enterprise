using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Common;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.Queries
{
    public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, Result<Loan>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IUserContext _userContext;

        public GetLoanByIdQueryHandler(ILoanRepository loanRepository, IUserContext userContext)
        {
            _loanRepository = loanRepository;
            _userContext = userContext;
        }

        public async Task<Result<Loan>> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
        {
            var loan = await _loanRepository.GetByIdAsync(request.Id);
            if (loan is null)
            {
                return Result<Loan>.Failure("Loan not found", ErrorType.NotFound);
            }

            // IDOR Protection: a user should only be allowed to retrieve a loan if they are the owner of that loan
            // (matching CustomerId) OR if they possess an Administrative/Elevated role.
            if (!request.IsAdmin)
            {
                var ownerCustomerId = await _userContext.GetCustomerIdByUserIdAsync(request.UserId, cancellationToken);
                if (ownerCustomerId is null || ownerCustomerId.Value != loan.CustomerId)
                {
                    // Return NotFound to avoid leaking the existence of the resource
                    return Result<Loan>.Failure("Loan not found", ErrorType.NotFound);
                }
            }

            return Result<Loan>.Success(loan);
        }
    }
}
