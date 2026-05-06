using FinanceSap.Application.UseCases.RequestLoan;
using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Enums;
using FinanceSap.Domain.Interfaces;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace FinanceSap.Tests.Application;

public sealed class RequestLoanHandlerTests
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RequestLoanCommand> _validator;
    private readonly RequestLoanHandler _handler;

    public RequestLoanHandlerTests()
    {
        _customerRepository = Substitute.For<ICustomerRepository>();
        _loanRepository = Substitute.For<ILoanRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _validator = new RequestLoanValidator();

        _handler = new RequestLoanHandler(
            _customerRepository,
            _loanRepository,
            _unitOfWork,
            _validator);
    }

    [Fact(DisplayName = "HandleAsync — Comando válido deve criar empréstimo e retornar Id")]
    public async Task HandleAsync_WithValidCommand_CreatesLoanAndReturnsId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create("52998224725", "João Silva").Value!;
        _customerRepository.GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(customer);

        var command = new RequestLoanCommand(
            CustomerId: customerId,
            Amount: 10000m,
            Installments: 12,
            AnnualRate: 0.12m // 12% as decimal
        );

        // Act
        var result = await _handler.HandleAsync(command);

        // Debug: Print error if test fails
        if (!result.IsSuccess)
        {
            Console.WriteLine($"Test failed with error: {result.Error}");
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Error.Should().BeNull();

        await _loanRepository.Received(1).AddAsync(
            Arg.Is<Loan>(l => 
                l.CustomerId == customerId &&
                l.PrincipalAmount == 10000m &&
                l.Installments == 12 &&
                l.InterestRate == 0.12m &&
                l.Status == LoanStatus.Pending),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "HandleAsync — Cliente inexistente deve retornar NotFound")]
    public async Task HandleAsync_WithNonExistentCustomer_ReturnsNotFound()
    {
        // Arrange
        _customerRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        var command = new RequestLoanCommand(
            CustomerId: Guid.NewGuid(),
            Amount: 10000m,
            Installments: 12,
            AnnualRate: 0.12m // 12% as decimal
        );

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Contain("Cliente não encontrado");

        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Theory(DisplayName = "HandleAsync — Comando inválido deve retornar erro de validação")]
    [InlineData(0, 12, 0.12)]      // Amount zero
    [InlineData(-1000, 12, 0.12)]  // Amount negativo
    [InlineData(10000, 0, 0.12)]   // Installments zero
    [InlineData(10000, 361, 0.12)] // Installments > 360
    [InlineData(10000, 12, -0.01)] // AnnualRate negativo
    [InlineData(10000, 12, 0.51)]  // AnnualRate > 50%
    public async Task HandleAsync_WithInvalidCommand_ReturnsValidationError(
        decimal amount, int installments, decimal annualRate)
    {
        // Arrange
        var command = new RequestLoanCommand(
            CustomerId: Guid.NewGuid(),
            Amount: amount,
            Installments: installments,
            AnnualRate: annualRate
        );

        // Act
        var result = await _handler.HandleAsync(command);

        // Debug: Print error for analysis
        Console.WriteLine($"Validation test error: {result.Error}");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();

        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "HandleAsync — Deve calcular valores corretamente usando CompoundInterestCalculator")]
    public async Task HandleAsync_CalculatesLoanValuesCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create("52998224725", "Maria Oliveira").Value!;
        _customerRepository.GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(customer);

        var command = new RequestLoanCommand(
            CustomerId: customerId,
            Amount: 5000m,
            Installments: 6,
            AnnualRate: 0.10m // 10% as decimal
        );

        // Act
        var result = await _handler.HandleAsync(command);

        // Debug: Print error if test fails
        if (!result.IsSuccess)
        {
            Console.WriteLine($"Calculation test failed with error: {result.Error}");
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();

        await _loanRepository.Received(1).AddAsync(
            Arg.Is<Loan>(l => 
                l.CustomerId == customerId &&
                l.PrincipalAmount == 5000m &&
                l.Installments == 6 &&
                l.InterestRate == 0.10m &&
                l.MonthlyPaymentAmount > 0 &&
                l.TotalToPay > l.PrincipalAmount),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "HandleAsync — CustomerId vazio deve retornar erro de validação")]
    public async Task HandleAsync_WithEmptyCustomerId_ReturnsValidationError()
    {
        // Arrange
        var command = new RequestLoanCommand(
            CustomerId: Guid.Empty,
            Amount: 10000m,
            Installments: 12,
            AnnualRate: 0.12m // 12% as decimal
        );

        // Act
        var result = await _handler.HandleAsync(command);

        // Debug: Print error for analysis
        Console.WriteLine($"Empty CustomerId test error: {result.Error}");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Dados inválidos");
    }
}
