using FinanceSap.Domain.Interfaces;

namespace FinanceSap.Domain.Services;

/// <summary>
/// Implementação padrão do cálculo de empréstimo usando Tabela Price (Sistema Francês).
/// Fórmula: PMT = P × [i(1+i)^n] / [(1+i)^n - 1]
/// Onde:
///   - PMT = Valor da parcela mensal
///   - P   = Principal (valor do empréstimo)
///   - i   = Taxa de juros mensal
///   - n   = Número de parcelas
/// </summary>
public sealed class CompoundInterestCalculator : ILoanCalculator
{
    /// <summary>
    /// Calcula o valor da parcela mensal usando a Tabela Price.
    /// </summary>
    public decimal CalculateInstallment(decimal principal, decimal annualRate, int months)
    {
        ValidateParameters(principal, annualRate, months);

        // Converte taxa anual para mensal: i = (1 + taxa_anual)^(1/12) - 1
        var monthlyRate = (decimal)Math.Pow((double)(1 + annualRate), 1.0 / 12.0) - 1;

        // Caso especial: taxa zero (empréstimo sem juros)
        if (monthlyRate == 0)
            return principal / months;

        // Fórmula da Tabela Price: PMT = P × [i(1+i)^n] / [(1+i)^n - 1]
        var factor = (decimal)Math.Pow((double)(1 + monthlyRate), months);
        var numerator = monthlyRate * factor;
        var denominator = factor - 1;

        return principal * (numerator / denominator);
    }

    /// <summary>
    /// Calcula o valor total a pagar (soma de todas as parcelas).
    /// </summary>
    public decimal CalculateTotalAmount(decimal principal, decimal annualRate, int months)
    {
        var installment = CalculateInstallment(principal, annualRate, months);
        return installment * months;
    }

    // ── Validação de Parâmetros ──────────────────────────────────────────────
    private static void ValidateParameters(decimal principal, decimal annualRate, int months)
    {
        if (principal <= 0)
            throw new ArgumentException(
                "O valor principal deve ser maior que zero.",
                nameof(principal));

        if (annualRate < 0)
            throw new ArgumentException(
                "A taxa de juros não pode ser negativa.",
                nameof(annualRate));

        if (months <= 0)
            throw new ArgumentException(
                "O número de meses deve ser maior que zero.",
                nameof(months));

        // Limite de segurança: taxa anual máxima de 100% (proteção contra overflow)
        if (annualRate > 1.0m)
            throw new ArgumentException(
                "A taxa de juros anual não pode exceder 100%.",
                nameof(annualRate));

        // Limite de segurança: máximo 360 meses (30 anos)
        if (months > 360)
            throw new ArgumentException(
                "O prazo máximo é de 360 meses (30 anos).",
                nameof(months));
    }
}
