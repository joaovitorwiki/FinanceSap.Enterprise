using FinanceSap.Application.UseCases.Deposit;
using FinanceSap.Application.UseCases.Transfer;
using FinanceSap.Application.UseCases.Withdraw;
using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Enums;
using FinanceSap.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FinanceSap.Tests.Application.Financial;

public sealed class AccountOperationsHandlerTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Account CreateAccount(Guid customerId, decimal balance = 0m)
    {
        var result = Account.Create("1234567890", customerId);
        result.IsSuccess.Should().BeTrue();
        var account = result.Value!;
        if (balance > 0) account.Credit(balance);
        return account;
    }

    // ── DepositHandler ───────────────────────────────────────────────────────

    [Fact(DisplayName = "Deposit — valor positivo deve creditar e registrar Transaction")]
    public async Task Deposit_ValidAmount_CreditsAndRecordsTransaction()
    {
        var accountRepo     = Substitute.For<IAccountRepository>();
        var transactionRepo = Substitute.For<ITransactionRepository>();
        var uow             = Substitute.For<IUnitOfWork>();

        var customerId = Guid.NewGuid();
        var account    = CreateAccount(customerId);
        accountRepo.GetByIdTrackedAsync(account.Id, default).Returns(account);

        var handler = new DepositHandler(accountRepo, transactionRepo, uow);
        var result  = await handler.Handle(new DepositCommand(account.Id, 500m), default);

        result.IsSuccess.Should().BeTrue();
        account.Balance.Should().Be(500m);
        await transactionRepo.Received(1).AddAsync(
            Arg.Is<Transaction>(t => t.Amount == 500m && t.TransactionType == TransactionType.Deposit),
            default);
        await uow.Received(1).CommitAsync(default);
    }

    [Fact(DisplayName = "Deposit — valor zero deve retornar falha de validação")]
    public async Task Deposit_ZeroAmount_ReturnsFailure()
    {
        var handler = new DepositHandler(
            Substitute.For<IAccountRepository>(),
            Substitute.For<ITransactionRepository>(),
            Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new DepositCommand(Guid.NewGuid(), 0m), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Deposit — conta inexistente deve retornar NotFound")]
    public async Task Deposit_AccountNotFound_ReturnsNotFound()
    {
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetByIdTrackedAsync(Arg.Any<Guid>(), default).Returns((Account?)null);

        var handler = new DepositHandler(accountRepo, Substitute.For<ITransactionRepository>(), Substitute.For<IUnitOfWork>());
        var result  = await handler.Handle(new DepositCommand(Guid.NewGuid(), 100m), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    // ── WithdrawHandler ──────────────────────────────────────────────────────

    [Fact(DisplayName = "Withdraw — saldo suficiente deve debitar e registrar Transaction")]
    public async Task Withdraw_SufficientBalance_DebitsAndRecordsTransaction()
    {
        var accountRepo     = Substitute.For<IAccountRepository>();
        var transactionRepo = Substitute.For<ITransactionRepository>();
        var uow             = Substitute.For<IUnitOfWork>();
        var userContext     = Substitute.For<IUserContext>();

        var customerId = Guid.NewGuid();
        var userId     = Guid.NewGuid();
        var account    = CreateAccount(customerId, 1000m);

        accountRepo.GetByIdTrackedAsync(account.Id, default).Returns(account);
        userContext.GetCustomerIdByUserIdAsync(userId, default).Returns(customerId);

        var handler = new WithdrawHandler(accountRepo, transactionRepo, uow, userContext);
        var result  = await handler.Handle(new WithdrawCommand(account.Id, 300m, userId), default);

        result.IsSuccess.Should().BeTrue();
        account.Balance.Should().Be(700m);
        await transactionRepo.Received(1).AddAsync(
            Arg.Is<Transaction>(t => t.Amount == 300m && t.TransactionType == TransactionType.Withdrawal),
            default);
    }

    [Fact(DisplayName = "Withdraw — saldo insuficiente deve retornar falha de validação")]
    public async Task Withdraw_InsufficientBalance_ReturnsValidationFailure()
    {
        var accountRepo = Substitute.For<IAccountRepository>();
        var userContext = Substitute.For<IUserContext>();

        var customerId = Guid.NewGuid();
        var userId     = Guid.NewGuid();
        var account    = CreateAccount(customerId, 100m);

        accountRepo.GetByIdTrackedAsync(account.Id, default).Returns(account);
        userContext.GetCustomerIdByUserIdAsync(userId, default).Returns(customerId);

        var handler = new WithdrawHandler(accountRepo, Substitute.For<ITransactionRepository>(), Substitute.For<IUnitOfWork>(), userContext);
        var result  = await handler.Handle(new WithdrawCommand(account.Id, 500m, userId), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Saldo insuficiente");
    }

    [Fact(DisplayName = "Withdraw — IDOR: UserId diferente do dono deve retornar NotFound")]
    public async Task Withdraw_WrongOwner_ReturnsNotFound()
    {
        var accountRepo = Substitute.For<IAccountRepository>();
        var userContext = Substitute.For<IUserContext>();

        var account = CreateAccount(Guid.NewGuid(), 1000m);
        accountRepo.GetByIdTrackedAsync(account.Id, default).Returns(account);
        // Retorna um CustomerId diferente do dono da conta
        userContext.GetCustomerIdByUserIdAsync(Arg.Any<Guid>(), default).Returns(Guid.NewGuid());

        var handler = new WithdrawHandler(accountRepo, Substitute.For<ITransactionRepository>(), Substitute.For<IUnitOfWork>(), userContext);
        var result  = await handler.Handle(new WithdrawCommand(account.Id, 100m, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    // ── TransferHandler ──────────────────────────────────────────────────────

    [Fact(DisplayName = "Transfer — válida deve debitar origem, creditar destino e registrar 2 Transactions")]
    public async Task Transfer_Valid_DebitsSourceCreditsDest_RecordsTwoTransactions()
    {
        var accountRepo     = Substitute.For<IAccountRepository>();
        var transactionRepo = Substitute.For<ITransactionRepository>();
        var uow             = Substitute.For<IUnitOfWork>();
        var userContext     = Substitute.For<IUserContext>();

        var customerId  = Guid.NewGuid();
        var userId      = Guid.NewGuid();
        var source      = CreateAccount(customerId, 1000m);
        var destination = CreateAccount(Guid.NewGuid(), 0m);

        accountRepo.GetByIdTrackedAsync(source.Id, default).Returns(source);
        accountRepo.GetByIdTrackedAsync(destination.Id, default).Returns(destination);
        userContext.GetCustomerIdByUserIdAsync(userId, default).Returns(customerId);

        var handler = new TransferHandler(accountRepo, transactionRepo, uow, userContext);
        var result  = await handler.Handle(
            new TransferCommand(source.Id, destination.Id, 400m, userId), default);

        result.IsSuccess.Should().BeTrue();
        source.Balance.Should().Be(600m);
        destination.Balance.Should().Be(400m);
        await transactionRepo.Received(2).AddAsync(Arg.Any<Transaction>(), default);
    }

    [Fact(DisplayName = "Transfer — origem igual ao destino deve retornar falha")]
    public async Task Transfer_SameAccount_ReturnsFailure()
    {
        var handler = new TransferHandler(
            Substitute.For<IAccountRepository>(),
            Substitute.For<ITransactionRepository>(),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<IUserContext>());

        var id     = Guid.NewGuid();
        var result = await handler.Handle(new TransferCommand(id, id, 100m, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("iguais");
    }

    [Fact(DisplayName = "Transfer — IDOR: UserId diferente do dono da origem deve retornar NotFound")]
    public async Task Transfer_WrongOwner_ReturnsNotFound()
    {
        var accountRepo = Substitute.For<IAccountRepository>();
        var userContext = Substitute.For<IUserContext>();

        var source = CreateAccount(Guid.NewGuid(), 1000m);
        accountRepo.GetByIdTrackedAsync(source.Id, default).Returns(source);
        userContext.GetCustomerIdByUserIdAsync(Arg.Any<Guid>(), default).Returns(Guid.NewGuid());

        var handler = new TransferHandler(accountRepo, Substitute.For<ITransactionRepository>(), Substitute.For<IUnitOfWork>(), userContext);
        var result  = await handler.Handle(
            new TransferCommand(source.Id, Guid.NewGuid(), 100m, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }
}
