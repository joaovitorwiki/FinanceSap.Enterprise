using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using FinanceSap.Domain.Services;
using MediatR;

namespace FinanceSap.Application.Commands
{
    /// <summary>
    /// Handler for creating a new loan request.
    /// Implements the CQRS command pattern with proper Result-based error handling.
    /// </summary>
    public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, Result<Loan>>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoanCalculator _loanCalculator;

        public CreateLoanCommandHandler(
            ILoanRepository loanRepository,
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            ILoanCalculator loanCalculator)
        {
            _loanRepository = loanRepository;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _loanCalculator = loanCalculator;
        }

        public async Task<Result<Loan>> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
        {
            // Validate customer exists
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
            {
                return Result<Loan>.Failure("Cliente não encontrado.", ErrorType.NotFound);
            }

            // Create the loan using the factory method
            var result = Loan.Create(
                request.CustomerId,
                request.Amount,
                request.InterestRate,
                request.TermInMonths,
                _loanCalculator);

            if (!result.IsSuccess)
            {
                return Result<Loan>.Failure(result.Error ?? "Falha ao criar empréstimo.");
            }

            // At this point, result.Value is guaranteed to be non-null because IsSuccess is true
            // The null-forgiving operator is safe here as the factory method ensures a valid Loan
            var loan = result.Value!;

            await _loanRepository.AddAsync(loan, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<Loan>.Success(loan);
        }
    }
}