using FinanceSap.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FinanceSap.Tests.ValueObjects;

/// Cobertura unitária do Value Object <see cref="Cpf"/>.
/// Padrão AAA (Arrange / Act / Assert) em todos os testes.
/// Teorias [Theory] + [InlineData] para casos data-driven.
public sealed class CpfTests
{
    // -------------------------------------------------------------------------
    // SUCESSO
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("52998224725")]          // dígitos puros
    [InlineData("529.982.247-25")]       // formato com pontos e hífen
    [InlineData("  52998224725  ")]      // whitespace nas bordas
    [InlineData("111.444.777-35")]       // outro CPF válido formatado
    [InlineData("11144477735")]          // mesmo CPF sem formatação
    public void Create_WhenCpfIsValid_ShouldReturnSuccess(string raw)
    {
        // Act
        var result = Cpf.Create(raw);

        // Assert
        result.IsSuccess.Should().BeTrue(
            because: "'{0}' é um CPF matematicamente válido", raw);
        result.Value.Value.Should().HaveLength(11)
            .And.MatchRegex("^[0-9]{11}$",
            because: "o CPF armazenado deve conter apenas os 11 dígitos sem formatação");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Create_WhenCpfIsValid_ShouldStripFormatting()
    {
        // Arrange
        const string formatted = "529.982.247-25";
        const string expected  = "52998224725";

        // Act
        var result = Cpf.Create(formatted);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected,
            because: "pontos e hífen devem ser removidos antes do armazenamento");
    }

    // -------------------------------------------------------------------------
    // TAMANHO INVÁLIDO
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("1234567890")]           // 10 dígitos — falta 1
    [InlineData("123456789012")]         // 12 dígitos — sobra 1
    [InlineData("1")]                    // muito curto
    [InlineData("123456789012345")]      // muito longo
    public void Create_WhenLengthIsInvalid_ShouldReturnFailure(string raw)
    {
        // Act
        var result = Cpf.Create(raw);

        // Assert
        result.IsSuccess.Should().BeFalse(
            because: "'{0}' não possui 11 dígitos", raw);
        result.Error.Should().Be("CPF inválido.");
    }

    // -------------------------------------------------------------------------
    // CARACTERES INVÁLIDOS
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("5299822472A")]          // letra no final
    [InlineData("A2998224725")]          // letra no início
    [InlineData("5299822@725")]          // símbolo especial
    [InlineData("529 982 247")]          // espaços internos (não são pontos/hífen)
    [InlineData("cpfinvalido1")]         // texto com dígitos misturados
    public void Create_WhenContainsNonDigitCharacters_ShouldReturnFailure(string raw)
    {
        // Act
        var result = Cpf.Create(raw);

        // Assert
        result.IsSuccess.Should().BeFalse(
            because: "'{0}' contém caracteres não numéricos inválidos", raw);
        result.Error.Should().Be("CPF inválido.");
    }

    // -------------------------------------------------------------------------
    // DÍGITO VERIFICADOR INVÁLIDO (Módulo 11)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("52998224724")]          // último dígito errado (correto: 5)
    [InlineData("52998224715")]          // penúltimo dígito errado
    [InlineData("12345678901")]          // sequência numérica sem dígitos válidos
    [InlineData("98765432109")]          // dígito final alterado (correto: 0 → 9)
    [InlineData("11111111112")]          // quase homogêneo, mas falha no Módulo 11
    public void Create_WhenCheckDigitIsInvalid_ShouldReturnFailure(string raw)
    {
        // Act
        var result = Cpf.Create(raw);

        // Assert
        result.IsSuccess.Should().BeFalse(
            because: "'{0}' falha na validação do Módulo 11", raw);
        result.Error.Should().Be("CPF inválido.");
    }

    // -------------------------------------------------------------------------
    // NULO / VAZIO / WHITESPACE
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenNullOrWhitespace_ShouldReturnFailure(string? raw)
    {
        // Act
        var result = Cpf.Create(raw);

        // Assert
        result.IsSuccess.Should().BeFalse(
            because: "null, vazio e whitespace não representam um CPF");
        result.Error.Should().Be("CPF inválido.");
    }

    // -------------------------------------------------------------------------
    // SEQUÊNCIAS HOMOGÊNEAS (proteção de domínio)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("22222222222")]
    [InlineData("33333333333")]
    [InlineData("44444444444")]
    [InlineData("55555555555")]
    [InlineData("66666666666")]
    [InlineData("77777777777")]
    [InlineData("88888888888")]
    [InlineData("99999999999")]
    public void Create_WhenAllDigitsAreEqual_ShouldReturnFailure(string raw)
    {
        // Act
        var result = Cpf.Create(raw);

        // Assert
        result.IsSuccess.Should().BeFalse(
            because: "'{0}' é uma sequência homogênea matematicamente inválida pelo domínio", raw);
        result.Error.Should().Be("CPF inválido.");
    }

    // -------------------------------------------------------------------------
    // MENSAGEM DE ERRO — PROTEÇÃO DE DOMÍNIO
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WhenInvalid_ErrorMessageShouldBeExact()
    {
        // Arrange
        const string expectedError = "CPF inválido.";

        // Act
        var result = Cpf.Create("00000000000");

        // Assert
        result.Error.Should().Be(expectedError,
            because: "a mensagem de erro deve ser exata para não vazar detalhes internos ao cliente");
    }

    // -------------------------------------------------------------------------
    // IMUTABILIDADE E IGUALDADE ESTRUTURAL (readonly record struct)
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_TwoInstancesWithSameDigits_ShouldBeEqual()
    {
        // Arrange
        const string raw = "52998224725";

        // Act
        var cpf1 = Cpf.Create(raw).Value;
        var cpf2 = Cpf.Create(raw).Value;

        // Assert
        cpf1.Should().Be(cpf2,
            because: "readonly record struct usa igualdade por valor — mesmos dígitos = mesma identidade");
    }

    [Fact]
    public void Create_TwoInstancesWithDifferentDigits_ShouldNotBeEqual()
    {
        // Arrange
        var cpf1 = Cpf.Create("52998224725").Value;
        var cpf2 = Cpf.Create("11144477735").Value;

        // Assert
        cpf1.Should().NotBe(cpf2);
    }

    // -------------------------------------------------------------------------
    // TOSTRING — FORMATAÇÃO
    // -------------------------------------------------------------------------

    [Fact]
    public void ToString_ShouldReturnFormattedCpf()
    {
        // Arrange
        var cpf = Cpf.Create("52998224725").Value;

        // Act
        var formatted = cpf.ToString();

        // Assert
        formatted.Should().Be("529.982.247-25",
            because: "ToString deve retornar o CPF no formato NNN.NNN.NNN-DD");
    }

    // -------------------------------------------------------------------------
    // CONVERSÃO IMPLÍCITA
    // -------------------------------------------------------------------------

    [Fact]
    public void ImplicitConversion_FromValidString_ShouldReturnCpfWithCorrectValue()
    {
        // Arrange
        const string raw = "52998224725";

        // Act
        Cpf cpf = raw; // conversão implícita

        // Assert
        cpf.Value.Should().Be(raw,
            because: "a conversão implícita de string válida deve produzir o mesmo valor");
    }

    [Fact]
    public void ImplicitConversion_ToCpfAndBackToString_ShouldPreserveDigits()
    {
        // Arrange
        const string raw = "52998224725";

        // Act
        Cpf   cpf    = raw;          // string → Cpf
        string back  = cpf;          // Cpf → string

        // Assert
        back.Should().Be(raw,
            because: "a conversão de ida e volta deve preservar os 11 dígitos originais");
    }
}
