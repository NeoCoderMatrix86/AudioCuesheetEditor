//This file is part of AudioCuesheetEditor.

//AudioCuesheetEditor is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//AudioCuesheetEditor is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see
//<http: //www.gnu.org/licenses />.
namespace AudioCuesheetEditor.Services;

/// <summary>
/// Defines types of errors
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Validation failed with error
    /// </summary>
    ValidationFailed,
    /// <summary>
    /// Resource already exists
    /// </summary>
    AlreadyDone,
    /// <summary>
    /// Current action is not possible
    /// </summary>
    NotPossible,
    /// <summary>
    /// Action was aborted
    /// </summary>
    Aborted,
}

/// <summary>
/// Represents failure of a service
/// </summary>
public class FailureDetails(ErrorType type, string message)
{
    /// <summary>
    /// Type of error
    /// </summary>
    public ErrorType Type { get; } = type;
    /// <summary>
    /// The error message
    /// </summary>
    public string Message { get; } = message;
}


/// <summary>
/// Indicates a service result.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Contains the failure details if the operation failed.
    /// </summary>
    public FailureDetails? FailureDetails { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    protected Result()
    {
        IsSuccess = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class with an error.
    /// </summary>
    /// <param name="failureDetails"></param>
    protected Result(FailureDetails failureDetails)
    {
        IsSuccess = false;
        FailureDetails = failureDetails;
    }

    /// <summary>
    /// Creates a successful result without a value.
    /// </summary>
    public static Result Success() => new();

    /// <summary>
    /// Creates a successful result containing a value.
    /// </summary>
    public static Result<T> Success<T>(T value) => new(value);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static FailureResult Failure(FailureDetails error) => new(error);
}

/// <summary>
/// Represents a failed result before conversion to a specific result type.
/// </summary>
public sealed class FailureResult : Result
{
    internal FailureResult(FailureDetails error) : base(error) { }
}

/// <summary>
/// Represents a service result containing a value.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the result value.
    /// </summary>
    public T? Value { get; }

    internal Result(T value)
    {
        Value = value;
    }

    Result(FailureDetails error) : base(error) { }

    /// <summary>
    /// Converts a failed result to a typed failed result.
    /// </summary>
    public static implicit operator Result<T>(FailureResult failure)
    {
        return new Result<T>(failure.FailureDetails ?? throw new InvalidOperationException("A failed result requires failure details."));
    }
}
