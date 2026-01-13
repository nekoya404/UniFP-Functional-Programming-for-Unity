using System;
using System.Runtime.CompilerServices;
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Fallback and recovery patterns for railway-oriented programming
    /// </summary>
    public static partial class RailwayExtensions
    {
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

            var errorMessage = source.GetErrorMessage();
            return SafeExecutor.Execute(
                () => errorPredicate(errorMessage) ? handler(errorMessage) : source,
                OperationType.Recover,
                memberName,
                filePath,
                lineNumber);
        }
    }
}
