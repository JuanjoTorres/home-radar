namespace HomeRadar.Domain.Common;

public sealed record DomainError(string Code, string Message);

public readonly record struct Result<T>
{
    internal Result(T? value, DomainError? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }

    public DomainError? Error { get; }

    public bool IsSuccess => Error is null;

    public static implicit operator Result<T>(T value) => new(value, null);

    public static implicit operator Result<T>(DomainError error) => new(default, error);

    public Result<TOut> Map<TOut>(Func<T, TOut> map) => IsSuccess ? Result.Success(map(Value!)) : Result.Failure<TOut>(Error!);
}

public static class Result
{
    public static Result<T> Success<T>(T value) => new(value, null);

    public static Result<T> Failure<T>(DomainError error) => new(default, error);
}
