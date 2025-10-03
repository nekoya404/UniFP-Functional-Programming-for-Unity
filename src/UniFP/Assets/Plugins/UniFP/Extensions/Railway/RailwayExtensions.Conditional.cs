using System;
using System.Runtime.CompilerServices;
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Strict and conditional operations for railway-oriented programming
    /// </summary>
    public static partial class RailwayExtensions
    {
        /// <summary>Execute side effect that must succeed. If side effect fails, entire pipeline fails.</summary>
        public static Result<T> DoStrict<T>(
            this Result<T> source,
            Func<T, Result<Unit>> strictAction,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (source.IsFailure)
                return source;

            return SafeExecutor.Execute(
                () =>
                {
                    var result = strictAction(source.Value);
                    return result.IsFailure ? Result<T>.Failure(result.ErrorCode) : source;
                },
                OperationType.Do,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Conditionally bind based on predicate</summary>
        public static Result<T> ThenIf<T>(
            this Result<T> source,
            Func<T, bool> condition,
            Func<T, Result<T>> binder,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (source.IsFailure)
                return source;

            return SafeExecutor.Execute(
                () => condition(source.Value) ? binder(source.Value) : source,
                OperationType.ThenIf,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Conditionally map based on predicate</summary>
        public static Result<T> MapIf<T>(
            this Result<T> source,
            Func<T, bool> condition,
            Func<T, T> mapper,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (source.IsFailure)
                return source;

            return SafeExecutor.Execute(
                () => condition(source.Value) ? Result<T>.Success(mapper(source.Value)) : source,
                OperationType.Map,
                memberName,
                filePath,
                lineNumber);
        }
    }
}
