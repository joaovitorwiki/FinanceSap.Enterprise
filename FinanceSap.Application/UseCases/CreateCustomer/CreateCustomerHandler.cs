using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using FluentValidation;

namespace FinanceSap.Application.UseCases.CreateCustomer;

// Orquestra a criação de um Customer.
// Primary Constructor (C# 13) para injeção de dependência — sem campo privado boilerplate.
// Fluxo: validar entrada → verificar duplicidade → criar entidade → persistir → retornar Id.
public sealed class CreateCustomerHandler(
    ICustomerRepository repository,
    IUnitOfWork unitOfWork,
    IValidator<CreateCustomerCommand> validator)
{
    public async Task<Result<Guid>> HandleAsync(
        CreateCustomerCommand command,
        CancellationToken ct = default)
    {
        // 1. Validação de formato (FluentValidation)
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return Result<Guid>.Failure(errors);
        }

        // 2. Verificação de duplicidade — regra de negócio que requer acesso a dados
        var exists = await repository.ExistsByDocumentAsync(command.Document, ct);
        if (exists)
            return Result<Guid>.Failure(
                "Já existe um cliente cadastrado com este CPF.",
                ErrorType.Conflict);

        // 3. Criação da entidade via factory method do Domain
        //    Customer.Create() executa a validação de Módulo 11 internamente
        var customerResult = Customer.Create(command.Document, command.FullName);
        if (!customerResult.IsSuccess)
            return Result<Guid>.Failure(customerResult.Error!);

        // 4. Persistência atômica via Unit of Work
        await repository.AddAsync(customerResult.Value!, ct);
        await unitOfWork.CommitAsync(ct);

        return Result<Guid>.Success(customerResult.Value!.Id);
    }
}
