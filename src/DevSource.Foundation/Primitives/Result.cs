using System.Collections.ObjectModel;

namespace DevSource.Foundation.Primitives;

/// <summary>
/// It represents the result of an operation with no return value.
/// </summary>
/// <remarks>
/// Use <see cref="Result"/> for expected and recoverable business failures.
/// Use <see cref="Guard"/> for programmer errors that should fail fast.
/// </remarks>
public class Result
{
    private static readonly IReadOnlyCollection<Error> EmptyErrors = [];

    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the collection of errors associated with the result.
    /// </summary>
    public IReadOnlyCollection<Error> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="errors">The collection of errors associated with the operation.</param>
    protected Result(bool isSuccess, IReadOnlyCollection<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Any(error => error is null))
        {
            throw new ArgumentException("Errors cannot contain null values.", nameof(errors));
        }

        switch (isSuccess)
        {
            case true when errors.Count > 0:
                throw new InvalidOperationException("A successful result cannot contain errors.");
            case false when errors.Count == 0:
                throw new InvalidOperationException("A failed result must contain at least one error.");
            default:
                IsSuccess = isSuccess;
                Errors = errors;
                break;
        }
    }    

    /// <summary>
    /// Creates a successful result without a value.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success()
    {
        return new Result(true, EmptyErrors);
    }

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">The error associated with the operation.</param>
    /// <returns>A failed result.</returns>
    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result(false, [error]);
    }

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <param name="errors">The errors associated with the operation.</param>
    /// <returns>A failed result.</returns>
    public static Result Failure(params Error[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return Failure((IEnumerable<Error>)errors);
    }

    /// <summary>
    /// Creates a failed result with a collection of errors.
    /// </summary>
    /// <param name="errors">The errors associated with the operation.</param>
    /// <returns>A failed result.</returns>
    public static Result Failure(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var normalizedErrors = errors
            .Distinct()
            .ToArray();

        if (normalizedErrors.Length == 0)
        {
            throw new ArgumentException("At least one error is required.", nameof(errors));
        }

        var readOnlyErrors = new ReadOnlyCollection<Error>(normalizedErrors);
        return new Result(false, readOnlyErrors);
    }
}

/// <summary>
/// Represents the result of an operation with a return value.
/// </summary>
/// <typeparam name="T">The type of the value returned in case of success.</typeparam>
public sealed class Result<T> : Result
{
    private readonly T? value;

    private Result(T? value) : base(true, [])
    {
        this.value = value;
    }

    private Result(IReadOnlyCollection<Error> errors) : base(false, errors)
    {
        value = default;
    }

    /// <summary>
    /// Gets the value of the operation when the result is successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public T Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("Cannot access value from a failed result.");

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The value returned in case of success.</param>
    /// <returns>A successful result.</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">The error associated with the operation.</param>
    /// <returns>A failed result.</returns>
    public static new Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>([error]);
    }

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <param name="errors">The errors associated with the operation.</param>
    /// <returns>A failed result.</returns>
    public static new Result<T> Failure(params Error[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return Failure((IEnumerable<Error>)errors);
    }

    /// <summary>
    /// Creates a failed result with a collection of errors.
    /// </summary>
    /// <param name="errors">The errors associated with the operation.</param>
    /// <returns>A failed result.</returns>
    public static new Result<T> Failure(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var normalizedErrors = errors
            .Distinct()
            .ToArray();

        if (normalizedErrors.Length == 0)
        {
            throw new ArgumentException("At least one error is required.", nameof(errors));
        }

        var readOnlyErrors = new ReadOnlyCollection<Error>(normalizedErrors);
        return new Result<T>(readOnlyErrors);
    }
}
