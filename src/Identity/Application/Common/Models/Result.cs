namespace ServerGame.Application.Common.Models;

public class Result
{
    internal Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public bool Succeeded { get; init; }

    public string[] Errors { get; init; }

    public static Result Success() => new(true, Array.Empty<string>());

    public static Result Failure(IEnumerable<string> errors) => new(false, errors);

    public static Result Failure(params string[] errors) => new(false, errors);
}

// Versão genérica
public class Result<T> : Result
{
    public T? Value { get; init; }

    private Result(T? value, bool succeeded, IEnumerable<string> errors)
        : base(succeeded, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, Array.Empty<string>());

    public static new Result<T> Failure(IEnumerable<string> errors) => new(default, false, errors);

    public static new Result<T> Failure(params string[] errors) => new(default, false, errors);
}
