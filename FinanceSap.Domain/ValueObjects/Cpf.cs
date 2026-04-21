using FinanceSap.Domain.Common;

namespace FinanceSap.Domain.ValueObjects;

/// Value Object imutável que representa um CPF válido.
/// - readonly record struct: alocado na stack, igualdade estrutural gerada pelo compilador.
/// - Criação exclusiva via <see cref="Create"/> — construtor privado impede instâncias inválidas.
/// - Conversão implícita de string para uso fluente em código interno confiável.
public readonly record struct Cpf
{
    // Armazena apenas os 11 dígitos, sem formatação.
    public string Value { get; }

    private Cpf(string value) => Value = value;

    /// Cria um Cpf validado. Retorna Failure sem lançar exceções.
    public static Result<Cpf> Create(string? raw)
    {
        var digits = raw?.Replace(".", "").Replace("-", "").Trim() ?? string.Empty;

        if (!IsValid(digits))
            return Result<Cpf>.Failure("CPF inválido.");

        return Result<Cpf>.Success(new Cpf(digits));
    }

    // Conversão implícita para uso em contextos internos confiáveis (ex: seeds, testes).
    public static implicit operator Cpf(string raw)   => Create(raw).Value;
    public static implicit operator string(Cpf cpf)   => cpf.Value;

    public override string ToString() => $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}";

    // --- Algoritmo Módulo 11 ---

    private static bool IsValid(string digits)
    {
        if (digits.Length != 11 || !digits.All(char.IsDigit))
            return false;

        // Rejeita sequências homogêneas (ex: 000...0, 111...1).
        if (digits.Distinct().Count() == 1)
            return false;

        return CheckDigit(digits, 9) && CheckDigit(digits, 10);
    }

    /// Valida o dígito verificador na posição <paramref name="position"/> (9 ou 10).
    private static bool CheckDigit(string digits, int position)
    {
        // Peso inicial: position+1 para o primeiro dígito, decrementando até 2.
        var sum = 0;
        for (var i = 0; i < position; i++)
            sum += (digits[i] - '0') * (position + 1 - i);

        var remainder = sum % 11;
        var expected  = remainder < 2 ? 0 : 11 - remainder;

        return (digits[position] - '0') == expected;
    }
}
