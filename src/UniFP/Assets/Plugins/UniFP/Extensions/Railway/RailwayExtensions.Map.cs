using System;
using System.Runtime.CompilerServices;
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Dual mapping operations for railway-oriented programming
    /// </summary>
    public static partial class RailwayExtensions
    {
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
                    : Result<TResult>.Failure(errorMapper(source.GetErrorMessage())),
                OperationType.Map,
                memberName,
                filePath,
                lineNumber);
        }
    }

    /// <summary>
    /// Unit type for operations that return nothing
    /// </summary>
    public readonly struct Unit
    {
        public static readonly Unit Default = new Unit();
    }
}
