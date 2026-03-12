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
namespace AudioCuesheetEditor.Services
{
    /// <summary>
    /// Defines types of errors
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// Validation failed with error
        /// </summary>
        ValidationFailed
    }

    /// <summary>
    /// Represents an error of an service
    /// </summary>
    public class Error(ErrorType type, string message)
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
    /// Indicates a service result
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Indicates if the result was successful
        /// </summary>
        public bool IsSuccess { get; }
        /// <summary>
        /// The error if one occurred
        /// </summary>
        public Error? Error { get; }

        /// <summary>
        /// Private constructor
        /// </summary>
        protected Result()
        {
            IsSuccess = true;
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        protected Result(Error error)
        {
            IsSuccess = false;
            Error = error;
        }
        /// <summary>
        /// Creates a success result
        /// </summary>
        /// <returns></returns>
        public static Result Success() => new();
        /// <summary>
        /// Creates a failure result
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static Result Failure(Error error) => new(error);
    }

    /// <summary>
    /// Represent the result of a service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T> : Result
    {
        /// <summary>
        /// The result value
        /// </summary>
        public T? Value { get; }

        private Result(T value) : base()
        {
            Value = value;
        }

        private Result(Error error) : base(error) { }
        /// <summary>
        /// Creates a success result
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Result<T> Success(T value) => new(value);
        /// <summary>
        /// Creates a failure result
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static new Result<T> Failure(Error error) => new(error);
    }
}
