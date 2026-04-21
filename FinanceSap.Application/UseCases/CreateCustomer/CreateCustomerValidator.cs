using System.Text.RegularExpressions;
using FluentValidation;

namespace FinanceSap.Application.UseCases.CreateCustomer;

public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    // Allowlist para nome: letras Unicode (incluindo acentos), espaços e hífens.
    // Abordagem allowlist é mais segura que blocklist — rejeita qualquer caractere
    // não explicitamente autorizado, incluindo <, >, ', ", ;, --, scripts, etc.
    // \p{L} cobre letras de todos os alfabetos (latim, cirílico, árabe, etc.).
    private static readonly Regex FullNameAllowList =
        new(@"^[\p{L}\s\-']+$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    public CreateCustomerValidator()
    {
        RuleFor(x => x.Document)
            .NotEmpty().WithMessage("Dados inválidos.")
            .Matches(@"^[\d.\-]+$").WithMessage("Dados inválidos.");
        // Mensagem genérica intencional — não revela ao atacante quais caracteres são aceitos.

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Dados inválidos.")
            .MaximumLength(150).WithMessage("Dados inválidos.")
            // Allowlist: apenas letras Unicode, espaços, hífens e apóstrofos (nomes como O'Brien).
            // Bloqueia: <script>, tags HTML, SQL injection, caracteres de controle.
            .Matches(FullNameAllowList).WithMessage("Dados inválidos.");
        // Mensagem genérica: não informa ao atacante quais caracteres foram bloqueados.
    }
}
