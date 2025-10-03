using System;
using System.Runtime.CompilerServices;
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Core binding and mapping operations for Result
    /// </summary>
    public static partial class ResultExtensions
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
    }
}
