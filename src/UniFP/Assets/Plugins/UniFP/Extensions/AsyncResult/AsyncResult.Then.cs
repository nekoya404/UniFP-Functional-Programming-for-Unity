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
    /// Async Result extensions - ThenAsync operations
    /// Supports both UniTask (UNIFP_UNITASK) and Unity Awaitable (UNIFP_AWAITABLE)
    /// </summary>
    public static partial class AsyncResult
    {
#if UNIFP_UNITASK
        /// <summary>Bind async function to Result (UniTask version)</summary>
        public static UniTask<Result<TResult>> ThenAsync<TSource, TResult>(
            this Result<TSource> source,
            Func<TSource, UniTask<Result<TResult>>> binder,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (source.IsFailure)
                return UniTask.FromResult(Result<TResult>.Failure(source.ErrorCode));

            return SafeExecutor.ExecuteAsync(
                () => binder(source.Value),
                OperationType.ThenAsync,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Bind sync function to async Result</summary>
        public static async UniTask<Result<TResult>> ThenAsync<TSource, TResult>(
            this UniTask<Result<TSource>> sourceTask,
            Func<TSource, Result<TResult>> binder)
        {
            var source = await sourceTask;
            return source.Bind(binder);
        }

        /// <summary>Bind async function to async Result</summary>
        public static async UniTask<Result<TResult>> ThenAsync<TSource, TResult>(
            this UniTask<Result<TSource>> sourceTask,
            Func<TSource, UniTask<Result<TResult>>> binder,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            var source = await sourceTask;
            if (source.IsFailure)
                return Result<TResult>.Failure(source.ErrorCode);

            return await SafeExecutor.ExecuteAsync(
                () => binder(source.Value),
                OperationType.ThenAsync,
                memberName,
                filePath,
                lineNumber);
        }
#elif UNIFP_AWAITABLE
        /// <summary>Bind async function to Result (Unity Awaitable version - fallback)</summary>
        public static Awaitable<Result<TResult>> ThenAsync<TSource, TResult>(
            this Result<TSource> source,
            Func<TSource, Awaitable<Result<TResult>>> binder,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (source.IsFailure)
                return Awaitable.FromResult(Result<TResult>.Failure(source.ErrorCode));

            return SafeExecutor.ExecuteAsync(
                () => binder(source.Value),
                OperationType.ThenAsync,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Bind sync function to async Result (Awaitable version)</summary>
        public static async Awaitable<Result<TResult>> ThenAsync<TSource, TResult>(
            this Awaitable<Result<TSource>> sourceTask,
            Func<TSource, Result<TResult>> binder)
        {
            var source = await sourceTask;
            return source.Bind(binder);
        }

        /// <summary>Bind async function to async Result (Awaitable version)</summary>
        public static async Awaitable<Result<TResult>> ThenAsync<TSource, TResult>(
            this Awaitable<Result<TSource>> sourceTask,
            Func<TSource, Awaitable<Result<TResult>>> binder,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            var source = await sourceTask;
            if (source.IsFailure)
                return Result<TResult>.Failure(source.ErrorCode);

            return await SafeExecutor.ExecuteAsync(
                () => binder(source.Value),
                OperationType.ThenAsync,
                memberName,
                filePath,
                lineNumber);
        }
#endif
    }
}
