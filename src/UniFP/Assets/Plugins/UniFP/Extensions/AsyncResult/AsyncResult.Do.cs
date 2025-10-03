using System;
#if UNIFP_UNITASK
using Cysharp.Threading.Tasks;
#elif UNIFP_AWAITABLE
using UnityEngine;
#endif

namespace UniFP
{
    /// <summary>
    /// Async Result extensions - DoAsync operations
    /// Supports both UniTask (UNIFP_UNITASK) and Unity Awaitable (UNIFP_AWAITABLE)
    /// </summary>
    public static partial class AsyncResult
    {
#if UNIFP_UNITASK
        /// <summary>Execute async side effect on success (UniTask version)</summary>
        public static async UniTask<Result<T>> DoAsync<T>(
            this Result<T> source,
            Func<T, UniTask> action)
        {
            if (source.IsSuccess)
                await action(source.Value);

            return source;
        }

        /// <summary>Execute sync side effect on async result</summary>
        public static async UniTask<Result<T>> DoAsync<T>(
            this UniTask<Result<T>> sourceTask,
            Action<T> action)
        {
            var source = await sourceTask;
            if (source.IsSuccess)
                action(source.Value);

            return source;
        }

        /// <summary>Execute async side effect on async result</summary>
        public static async UniTask<Result<T>> DoAsync<T>(
            this UniTask<Result<T>> sourceTask,
            Func<T, UniTask> action)
        {
            var source = await sourceTask;
            if (source.IsSuccess)
                await action(source.Value);

            return source;
        }
#elif UNIFP_AWAITABLE
        /// <summary>Execute async side effect on success (Unity Awaitable version - fallback)</summary>
        public static async Awaitable<Result<T>> DoAsync<T>(
            this Result<T> source,
            Func<T, Awaitable> action)
        {
            if (source.IsSuccess)
                await action(source.Value);

            return source;
        }

        /// <summary>Execute sync side effect on async result (Awaitable version)</summary>
        public static async Awaitable<Result<T>> DoAsync<T>(
            this Awaitable<Result<T>> sourceTask,
            Action<T> action)
        {
            var source = await sourceTask;
            if (source.IsSuccess)
                action(source.Value);

            return source;
        }

        /// <summary>Execute async side effect on async result (Awaitable version)</summary>
        public static async Awaitable<Result<T>> DoAsync<T>(
            this Awaitable<Result<T>> sourceTask,
            Func<T, Awaitable> action)
        {
            var source = await sourceTask;
            if (source.IsSuccess)
                await action(source.Value);

            return source;
        }
#endif
    }
}
