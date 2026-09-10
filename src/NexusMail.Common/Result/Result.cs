namespace NexusMail.Common.Result;

/// <summary>
/// Represents either a successful value or an error.
/// No external dependencies — plain C# only.
/// </summary>
public sealed class Result<TValue>
{
    private readonly TValue? _value;
    private readonly Error? _error;

    private Result(TValue value)
    {
        IsSuccess = true;
        _value = value;
        _error = null;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a failed Result.");

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error of a successful Result.");

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(Error error) => new(error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);

    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<Error, TOut> onFailure)
        => IsSuccess ? onSuccess(_value!) : onFailure(_error!);
}

/// <summary>
/// Non-generic Result for void operations.
/// </summary>
public sealed class Result
{
    private readonly Error? _error;

    private Result() => IsSuccess = true;
    private Result(Error error) { IsSuccess = false; _error = error; }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error of a successful Result.");

    public static Result Success() => new();
    public static Result Failure(Error error) => new(error);
    public static implicit operator Result(Error error) => Failure(error);
}
