using FinanceSap.Application.UseCases.LoanDisbursal;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Enums;
using FinanceSap.Domain.Events;
using FinanceSap.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FinanceSap.Tests.Application.Financial;

public sealed class LoanApprovedEventHandlerTests
{
    [Fact(DisplayName = "LoanApprovedEvent — deve creditar conta e registrar Transaction de LoanDisbursal")]
    public async Task Handle_AccountExists_CreditsAndRecordsLoanDisbursal()
    {
        var accountRepo     = Substitute.For<IAccountRepository>();
        var transactionRepo = Substitute.For<ITransactionRepository>();
        var uow             = Substitute.For<IUnitOfWork>();

        var customerId = Guid.NewGuid();
        var account    = Account.Create("1234567890", customerId).Value!;
        accountRepo.GetByCustomerIdTrackedAsync(customerId, default).Returns(account);

        var handler = new LoanApprovedEventHandler(accountRepo, transactionRepo, uow);
        var evt     = new LoanApprovedEvent(Guid.NewGuid(), customerId, 5000m);

        await handler.Handle(evt, default);

        account.Balance.Should().Be(5000m);
        await transactionRepo.Received(1).AddAsync(
            Arg.Is<Transaction>(t =>
                t.Amount == 5000m &&
                t.TransactionType == TransactionType.LoanDisbursal &&
                t.AccountId == account.Id),
            default);
        await uow.Received(1).CommitAsync(default);
    }

    [Fact(DisplayName = "LoanApprovedEvent — cliente sem conta não deve lançar exceção")]
    public async Task Handle_NoAccount_CompletesGracefully()
    {
        var accountRepo = Substitute.For<IAccountRepository>();
        var uow         = Substitute.For<IUnitOfWork>();
        accountRepo.GetByCustomerIdTrackedAsync(Arg.Any<Guid>(), default).Returns((Account?)null);

        var handler = new LoanApprovedEventHandler(accountRepo, Substitute.For<ITransactionRepository>(), uow);
        var evt     = new LoanApprovedEvent(Guid.NewGuid(), Guid.NewGuid(), 1000m);

        var act = async () => await handler.Handle(evt, default);

        await act.Should().NotThrowAsync();
        await uow.DidNotReceive().CommitAsync(default);
    }
}
