using System;
using System.Runtime.CompilerServices;
#if UNIFP_UNITASK
using Cysharp.Threading.Tasks;
#elif UNIFP_AWAITABLE
using UnityEngine;
#endif
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Async Result extensions - FilterAsync operations
    /// Supports both UniTask (UNIFP_UNITASK) and Unity Awaitable (UNIFP_AWAITABLE)
    /// </summary>
    public static partial class AsyncResult
    {
#if UNIFP_UNITASK
        /// <summary>Filter with async predicate (UniTask version)</summary>
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
#elif UNIFP_AWAITABLE
        /// <summary>Filter with async predicate (Unity Awaitable version - fallback)</summary>
        public static async Awaitable<Result<T>> FilterAsync<T>(
            this Result<T> source,
            Func<T, Awaitable<bool>> predicate,
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

        /// <summary>Filter sync predicate on async Result (Awaitable version)</summary>
        public static async Awaitable<Result<T>> FilterAsync<T>(
            this Awaitable<Result<T>> sourceTask,
            Func<T, bool> predicate,
            string errorMessage)
        {
            var source = await sourceTask;
            return source.Filter(predicate, errorMessage);
        }
#endif
    }
}
