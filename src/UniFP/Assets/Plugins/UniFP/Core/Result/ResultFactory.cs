using System;
using System.Runtime.CompilerServices;

namespace UniFP
{
    /// <summary>
    /// Factory methods for creating Result instances
    /// </summary>
    public static partial class Result
    {
        /// <summary>Create a Result from a value (Zero GC)</summary>
        public static Result<T> FromValue<T>(T value)
        {
            return Result<T>.Success(value);
        }

        /// <summary>Create a Result from an ErrorCode (Zero GC)</summary>
        public static Result<T> FromError<T>(ErrorCode errorCode)
        {
            return Result<T>.Failure(errorCode);
        }

        /// <summary>Try to get a value from an operation, converting exceptions to Result</summary>
        public static Result<T> TryFromValue<T>(
            Func<T> func,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return Internal.SafeExecutor.Execute(
                () => Result<T>.Success(func()),
                OperationType.TryFromValue,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Try to get a Result from an operation, catching exceptions</summary>
        public static Result<T> TryFromResult<T>(
            Func<Result<T>> func,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return Internal.SafeExecutor.Execute(
                func,
                OperationType.TryFromResult,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Return Success if condition is true, otherwise Failure (Zero GC)</summary>
        public static Result<T> SuccessIf<T>(
            bool condition,
            T value,
            ErrorCode errorCode)
        {
            return condition
                ? Result<T>.Success(value)
                : Result<T>.Failure(errorCode);
        }

        /// <summary>Evaluate condition function and return Success/Failure (Zero GC)</summary>
        public static Result<T> SuccessIf<T>(
            Func<bool> conditionFunc,
            T value,
            ErrorCode errorCode,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return Internal.SafeExecutor.Execute(
                () => conditionFunc()
                    ? Result<T>.Success(value)
                    : Result<T>.Failure(errorCode),
                OperationType.FromValue,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Return Success if condition is false, otherwise Failure (Zero GC)</summary>
        public static Result<T> FailureIf<T>(
            bool condition,
            T value,
            ErrorCode errorCode)
        {
            return !condition
                ? Result<T>.Success(value)
                : Result<T>.Failure(errorCode);
        }

        /// <summary>Evaluate condition function and return Success/Failure (returns Success if false) (Zero GC)</summary>
        public static Result<T> FailureIf<T>(
            Func<bool> conditionFunc,
            T value,
            ErrorCode errorCode,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return Internal.SafeExecutor.Execute(
                () => !conditionFunc()
                    ? Result<T>.Success(value)
                    : Result<T>.Failure(errorCode),
                OperationType.FromValue,
                memberName,
                filePath,
                lineNumber);
        }
    }
}
