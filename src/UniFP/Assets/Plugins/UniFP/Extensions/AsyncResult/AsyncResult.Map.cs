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
    /// Async Result extensions - MapAsync operations
    /// Supports both UniTask (UNIFP_UNITASK) and Unity Awaitable (UNIFP_AWAITABLE)
    /// </summary>
    public static partial class AsyncResult
    {
#if UNIFP_UNITASK
        /// <summary>Map with async function (UniTask version)</summary>
        public static UniTask<Result<TResult>> MapAsync<TSource, TResult>(
            this Result<TSource> source,
            Func<TSource, UniTask<TResult>> mapper,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (source.IsFailure)
                return UniTask.FromResult(Result<TResult>.Failure(source.ErrorCode));

            return SafeExecutor.ExecuteAsync(
                async () => Result<TResult>.Success(await mapper(source.Value)),
                OperationType.MapAsync,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Map sync function on async Result</summary>
        public static async UniTask<Result<TResult>> MapAsync<TSource, TResult>(
            this UniTask<Result<TSource>> sourceTask,
            Func<TSource, TResult> mapper)
        {
            var source = await sourceTask;
            return source.Map(mapper);
        }

        /// <summary>Map async Result with async function</summary>
        public static async UniTask<Result<TResult>> MapAsync<TSource, TResult>(
            this UniTask<Result<TSource>> sourceTask,
            Func<TSource, UniTask<TResult>> mapper,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            var source = await sourceTask;
            if (source.IsFailure)
                return Result<TResult>.Failure(source.ErrorCode);

            return await SafeExecutor.ExecuteAsync(
                async () => Result<TResult>.Success(await mapper(source.Value)),
                OperationType.MapAsync,
                memberName,
                filePath,
                lineNumber);
        }
#elif UNIFP_AWAITABLE
        /// <summary>Map with async function (Unity Awaitable version - fallback)</summary>
        public static Awaitable<Result<TResult>> MapAsync<TSource, TResult>(
            this Result<TSource> source,
            Func<TSource, Awaitable<TResult>> mapper,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (source.IsFailure)
                return Awaitable.FromResult(Result<TResult>.Failure(source.ErrorCode));

            return SafeExecutor.ExecuteAsync(
                async () => Result<TResult>.Success(await mapper(source.Value)),
                OperationType.MapAsync,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Map sync function on async Result (Awaitable version)</summary>
        public static async Awaitable<Result<TResult>> MapAsync<TSource, TResult>(
            this Awaitable<Result<TSource>> sourceTask,
            Func<TSource, TResult> mapper)
        {
            var source = await sourceTask;
            return source.Map(mapper);
        }

        /// <summary>Map async Result with async function (Awaitable version)</summary>
        public static async Awaitable<Result<TResult>> MapAsync<TSource, TResult>(
            this Awaitable<Result<TSource>> sourceTask,
            Func<TSource, Awaitable<TResult>> mapper,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            var source = await sourceTask;
            if (source.IsFailure)
                return Result<TResult>.Failure(source.ErrorCode);

            return await SafeExecutor.ExecuteAsync(
                async () => Result<TResult>.Success(await mapper(source.Value)),
                OperationType.MapAsync,
                memberName,
                filePath,
                lineNumber);
        }
#endif
    }
}
