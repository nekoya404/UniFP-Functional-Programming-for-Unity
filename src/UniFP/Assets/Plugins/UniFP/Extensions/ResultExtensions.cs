using System;
using System.Runtime.CompilerServices;
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Extension methods for Result monad operations
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>Chain with Result-returning function (with automatic caller tracking) - Zero GC</summary>
        public static Result<TResult> Then<TSource, TResult>(
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

        /// <summary>Transform value (with automatic caller tracking) - Zero GC</summary>
        public static Result<TResult> Map<TSource, TResult>(
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

        /// <summary>Execute side effect on success (failure is ignored)</summary>
        public static Result<T> Do<T>(this Result<T> source, Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (source.IsSuccess)
                action(source.Value);

            return source;
        }

        /// <summary>Execute side effect on failure - Zero GC</summary>
        public static Result<T> DoOnError<T>(this Result<T> source, Action<ErrorCode> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (source.IsFailure)
                action(source.ErrorCode);

            return source;
        }

        /// <summary>Execute side effect on failure (allocates string)</summary>
        [Obsolete("Use DoOnError(Action<ErrorCode>) for Zero GC")]
        public static Result<T> DoOnError<T>(this Result<T> source, Action<string> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (source.IsFailure)
                action(source.ErrorMessage);

            return source;
        }

        /// <summary>Validate with predicate - string error (allocates)</summary>
        public static Result<T> Filter<T>(
            this Result<T> source,
            Func<T, bool> predicate,
            string errorMessage,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if (string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentNullException(nameof(errorMessage));

            if (source.IsFailure)
                return source;

            return SafeExecutor.Execute(
                () =>
                {
                    if (predicate(source.Value))
                        return source;

                    var formattedError = SafeExecutor.FormatValidationError(OperationType.Filter, errorMessage, memberName, filePath, lineNumber);
                    return Result<T>.Failure(formattedError);
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
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

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

        /// <summary>Recover from failure with fallback - Zero GC</summary>
        public static Result<T> Recover<T>(
            this Result<T> source,
            Func<ErrorCode, T> fallback)
        {
            if (fallback == null)
                throw new ArgumentNullException(nameof(fallback));

            return source.IsSuccess
                ? source
                : Result<T>.Success(fallback(source.ErrorCode));
        }

    }
}
