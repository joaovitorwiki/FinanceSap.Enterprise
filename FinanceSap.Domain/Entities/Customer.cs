using FinanceSap.Domain.Common;
using FinanceSap.Domain.ValueObjects;

namespace FinanceSap.Domain.Entities;

// Customer promovido a Aggregate Root — possui identidade própria (Id) e
// pode ser persistido de forma independente de LoanApplication.
public sealed class Customer
{
    public Guid   Id       { get; private set; }
    public Cpf    Document { get; private set; }
    public string FullName { get; private set; }

    private Customer() { Document = default; FullName = null!; } // EF Core

    private Customer(Cpf document, string fullName)
    {
        Id       = Guid.NewGuid();
        Document = document;
        FullName = fullName;
    }

    public static Result<Customer> Create(string rawDocument, string fullName)
    {
        var cpfResult = Cpf.Create(rawDocument);
        if (!cpfResult.IsSuccess)
            return Result<Customer>.Failure(cpfResult.Error!);

        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 150)
            return Result<Customer>.Failure("Nome completo inválido.");

        return Result<Customer>.Success(new Customer(cpfResult.Value, fullName.Trim()));
    }
}
