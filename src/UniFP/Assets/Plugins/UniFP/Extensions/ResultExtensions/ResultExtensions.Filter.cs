using System;
using System.Runtime.CompilerServices;
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Filter and validation operations for Result
    /// </summary>
    public static partial class ResultExtensions
    {
        /// <summary>Validate with predicate - string error (allocates)</summary>
        public static Result<T> Filter<T>(
            this Result<T> source,
            Func<T, bool> predicate,
            string errorMessage,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (source.IsFailure)
                return source;

            return SafeExecutor.Execute(
                () =>
                {
                    if (predicate(source.Value))
                        return source;

#if UNITY_EDITOR || UNIFP_DEBUG
                    var formattedError = $"[{System.IO.Path.GetFileName(filePath)}:{lineNumber} in {memberName}] {OperationType.Filter.ToDisplayString()} failed: {errorMessage}";
                    UnityEngine.Debug.LogWarning(formattedError);
                    return Result<T>.Failure(formattedError);
#else
                    return Result<T>.Failure(errorMessage);
#endif
                },
                OperationType.Filter,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Validate with predicate - ErrorCode (Zero GC)</summary>
        public static Result<T> Filter<T>(
            this Result<T> source,
            Func<T, bool> predicate,
            ErrorCode errorCode,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (source.IsFailure)
                return source;

            return SafeExecutor.Execute(
                () =>
                {
                    if (predicate(source.Value))
                        return source;

                    return Result<T>.Failure(errorCode);
                },
                OperationType.Filter,
                memberName,
                filePath,
                lineNumber);
        }
    }
}
