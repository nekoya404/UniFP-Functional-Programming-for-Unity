using System;
using Cysharp.Threading.Tasks;

namespace UniFP
{
    /// <summary>
    /// Async Result extensions - DoAsync operations
    /// </summary>
    public static partial class AsyncResult
    {
        /// <summary>Execute async side effect on success</summary>
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
    }
}
