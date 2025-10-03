using System;
using System.Runtime.CompilerServices;
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Railway-oriented programming extensions for Result
    /// </summary>
    public static class RailwayExtensions
    {
        #region DoStrict

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

        #endregion

        #region IfFailed

        /// <summary>Return alternative if source fails (fallback pattern)</summary>
        public static Result<T> IfFailed<T>(
            this Result<T> source,
            Func<Result<T>> alternative,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return SafeExecutor.Execute(
                () => source.IsSuccess ? source : alternative(),
                OperationType.IfFailed,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Return alternative if source fails (fallback pattern)</summary>
        public static Result<T> IfFailed<T>(this Result<T> source, Result<T> alternative)
        {
            return source.IsSuccess ? source : alternative;
        }

        #endregion

        #region ThenIf

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

        #endregion

        #region MapIf

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

        #endregion

        #region DoubleMap

        /// <summary>Map both success and error paths</summary>
        public static Result<TResult> DoubleMap<TSource, TResult>(
            this Result<TSource> source,
            Func<TSource, TResult> successMapper,
            Func<string, string> errorMapper,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return SafeExecutor.Execute(
                () => source.IsSuccess
                    ? Result<TResult>.Success(successMapper(source.Value))
                    : Result<TResult>.Failure(errorMapper(source.ErrorMessage)),
                OperationType.Map,
                memberName,
                filePath,
                lineNumber);
        }

        #endregion

        #region Catch

        /// <summary>Catch specific errors and handle them</summary>
        public static Result<T> Catch<T>(
            this Result<T> source,
            Func<string, bool> errorPredicate,
            Func<string, Result<T>> handler,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (source.IsSuccess)
                return source;

            return SafeExecutor.Execute(
                () => errorPredicate(source.ErrorMessage) ? handler(source.ErrorMessage) : source,
                OperationType.Recover,
                memberName,
                filePath,
                lineNumber);
        }

        #endregion
    }

    /// <summary>
    /// Unit type for operations that return nothing
    /// </summary>
    public readonly struct Unit
    {
        public static readonly Unit Default = new Unit();
    }
}
