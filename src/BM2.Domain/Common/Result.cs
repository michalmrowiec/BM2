namespace BM2.Domain.Common;

public readonly struct Result
{
    private Result(Error? error) => Error = error;

    public static Result Success => default;

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public static implicit operator Result(Error error) => new(error);
}

public readonly struct Result<T>
{
    private readonly T? _value;

    private Result(T? value, Error? error)
    {
        _value = value;
        Error = error;
    }

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("A failed result has no value.");

    public static implicit operator Result<T>(T value) => new(value, null);

    public static implicit operator Result<T>(Error error) => new(default, error);
}
