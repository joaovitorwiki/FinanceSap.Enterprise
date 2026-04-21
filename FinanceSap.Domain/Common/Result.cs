namespace FinanceSap.Domain.Common;

// Classifica o tipo de falha para que camadas superiores tomem decisões
// sem fazer parsing de strings de erro — elimina acoplamento por mensagem.
public enum ErrorType
{
    Validation, // entrada inválida           → HTTP 400
    Conflict,   // violação de unicidade      → HTTP 409
    NotFound,   // recurso inexistente        → HTTP 404
    Unexpected  // falha não categorizada     → HTTP 500
}

public sealed class Result<T>
{
    public T?         Value     { get; }
    public string?    Error     { get; }
    public ErrorType  ErrorType { get; }
    public bool       IsSuccess => Error is null;

    private Result(T value)                            { Value = value; }
    private Result(string error, ErrorType errorType)  { Error = error; ErrorType = errorType; }

    public static Result<T> Success(T value)
        => new(value);

    public static Result<T> Failure(string error, ErrorType errorType = ErrorType.Validation)
        => new(error, errorType);
}

public sealed class Result
{
    public string?   Error     { get; }
    public ErrorType ErrorType { get; }
    public bool      IsSuccess => Error is null;

    private Result()                                   { }
    private Result(string error, ErrorType errorType)  { Error = error; ErrorType = errorType; }

    public static Result Success()
        => new();

    public static Result Failure(string error, ErrorType errorType = ErrorType.Validation)
        => new(error, errorType);
}
