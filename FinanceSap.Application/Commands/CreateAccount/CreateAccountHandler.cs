using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using MediatR;

namespace FinanceSap.Application.Commands.CreateAccount;

// Handler — CreateAccount.
// Gera AccountNumber único, cria Account via factory method do Domain e persiste.
public sealed class CreateAccountHandler(
    IAccountRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAccountCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateAccountCommand request,
        CancellationToken ct)
    {
        // Verifica se o Customer já possui uma conta (1:1).
        var exists = await repository.ExistsByCustomerIdAsync(request.CustomerId, ct);
        if (exists)
            return Result<Guid>.Failure(
                "Cliente já possui uma conta.",
                ErrorType.Conflict);

        // Gera AccountNumber único de 10 dígitos.
        // Em produção, usar algoritmo que garanta unicidade global (ex: timestamp + random + checksum).
        var accountNumber = GenerateAccountNumber();

        var accountResult = Account.Create(accountNumber, request.CustomerId);
        if (!accountResult.IsSuccess)
            return Result<Guid>.Failure(accountResult.Error!);

        await repository.AddAsync(accountResult.Value!, ct);
        await unitOfWork.CommitAsync(ct);

        return Result<Guid>.Success(accountResult.Value!.Id);
    }

    private static string GenerateAccountNumber()
    {
        // Geração simplificada: timestamp + random.
        // Produção: usar algoritmo robusto com checksum (ex: Luhn) e verificação de unicidade no banco.
        var timestamp = DateTime.UtcNow.Ticks.ToString()[^6..];
        var random    = Random.Shared.Next(1000, 9999);
        return $"{timestamp}{random}";
    }
}
