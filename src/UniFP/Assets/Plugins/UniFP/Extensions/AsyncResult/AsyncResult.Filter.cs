using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Async Result extensions - FilterAsync operations
    /// </summary>
    public static partial class AsyncResult
    {
        /// <summary>Filter with async predicate</summary>
        public static async UniTask<Result<T>> FilterAsync<T>(
            this Result<T> source,
            Func<T, UniTask<bool>> predicate,
            string errorMessage,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (source.IsFailure)
                return source;

            return await SafeExecutor.ExecuteAsync(
                async () =>
                {
                    if (await predicate(source.Value))
                        return source;

                    var formattedError = SafeExecutor.FormatValidationError(OperationType.FilterAsync, errorMessage, memberName, filePath, lineNumber);
                    return Result<T>.Failure(formattedError);
                },
                OperationType.FilterAsync,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Filter sync predicate on async Result</summary>
        public static async UniTask<Result<T>> FilterAsync<T>(
            this UniTask<Result<T>> sourceTask,
            Func<T, bool> predicate,
            string errorMessage)
        {
            var source = await sourceTask;
            return source.Filter(predicate, errorMessage);
        }
    }
}
