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
    }
}
