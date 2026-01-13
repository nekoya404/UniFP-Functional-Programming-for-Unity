using System;
using System.Runtime.CompilerServices;
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Safe operations for Result - automatically catches exceptions
    /// </summary>
    public static partial class ResultExtensions
    {
        /// <summary>
        /// Chain with Result-returning function that automatically catches exceptions (Zero GC)
        /// </summary>
        /// <remarks>
        /// Similar to Then, but automatically wraps exceptions in Result.Failure.
        /// Useful when the binder function might throw exceptions.
        /// </remarks>
        public static Result<TResult> ThenSafe<TSource, TResult>(
            this Result<TSource> source,
            Func<TSource, Result<TResult>> binder,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            if (source.IsFailure)
                return Result<TResult>.Failure(source.ErrorCode);

            return SafeExecutor.Execute(
                () => binder(source.Value),
                OperationType.Then,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>
        /// Transform value with automatic exception catching (Zero GC)
        /// </summary>
        /// <remarks>
        /// Similar to Map, but automatically wraps exceptions in Result.Failure.
        /// Useful when the mapper function might throw exceptions.
        /// </remarks>
        public static Result<TResult> MapSafe<TSource, TResult>(
            this Result<TSource> source,
            Func<TSource, TResult> mapper,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (source.IsFailure)
                return Result<TResult>.Failure(source.ErrorCode);

            return SafeExecutor.Execute(
                () => Result<TResult>.Success(mapper(source.Value)),
                OperationType.Map,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>
        /// Execute side effect with automatic exception catching (Zero GC)
        /// </summary>
        /// <remarks>
        /// Similar to Do, but automatically catches exceptions.
        /// Useful when the action might throw exceptions.
        /// </remarks>
        public static Result<T> DoSafe<T>(
            this Result<T> source,
            Action<T> action,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (source.IsFailure)
                return source;

            return SafeExecutor.Execute(
                () =>
                {
                    action(source.Value);
                    return source;
                },
                OperationType.Do,
                memberName,
                filePath,
                lineNumber);
        }
    }
}
