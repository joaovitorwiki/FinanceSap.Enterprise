namespace FinanceSap.Application.UseCases.CreateCustomer;

// DTO de entrada imutável. Expõe apenas os campos necessários para criar um Customer.
// record garante imutabilidade e igualdade por valor — ideal para commands.
public sealed record CreateCustomerCommand(
    string Document,  // CPF — validação de Módulo 11 ocorre no Domain
    string FullName
);
