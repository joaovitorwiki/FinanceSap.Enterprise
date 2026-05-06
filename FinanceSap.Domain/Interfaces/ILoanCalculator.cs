namespace FinanceSap.Domain.Interfaces;

/// <summary>
/// Strategy Pattern — Define o contrato para cálculo de parcelas de empréstimo.
/// Permite múltiplas implementações (juros compostos, simples, SAC, Price, etc.)
/// sem modificar a entidade Loan (Open/Closed Principle).
/// </summary>
public interface ILoanCalculator
{
    /// <summary>
    /// Calcula o valor da parcela mensal de um empréstimo.
    /// </summary>
    /// <param name="principal">Valor principal do empréstimo (montante solicitado).</param>
    /// <param name="annualRate">Taxa de juros anual (ex: 0.12 para 12% ao ano).</param>
    /// <param name="months">Número de meses para pagamento.</param>
    /// <returns>Valor da parcela mensal.</returns>
    /// <exception cref="ArgumentException">Se os parâmetros forem inválidos.</exception>
    decimal CalculateInstallment(decimal principal, decimal annualRate, int months);

    /// <summary>
    /// Calcula o valor total a pagar (principal + juros).
    /// </summary>
    /// <param name="principal">Valor principal do empréstimo.</param>
    /// <param name="annualRate">Taxa de juros anual.</param>
    /// <param name="months">Número de meses para pagamento.</param>
    /// <returns>Valor total a pagar ao final do empréstimo.</returns>
    decimal CalculateTotalAmount(decimal principal, decimal annualRate, int months);
}
