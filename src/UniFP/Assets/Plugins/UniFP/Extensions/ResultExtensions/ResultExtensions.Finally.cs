using System;
#if UNIFP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace UniFP
{
    /// <summary>
    /// Finally operations for Result - executes final logic regardless of success/failure
    /// </summary>
    public static partial class ResultExtensions
    {
        /// <summary>Execute final function and return its result (functional version of try-finally)</summary>
        public static TOut Finally<T, TOut>(
            this Result<T> result,
            Func<Result<T>, TOut> finalFunc)
        {
            if (finalFunc == null)
                throw new ArgumentNullException(nameof(finalFunc));

            return finalFunc(result);
        }

        /// <summary>Execute side effect and return original Result</summary>
        public static Result<T> Finally<T>(
            this Result<T> result,
            Action<Result<T>> finalAction)
        {
            if (finalAction == null)
                throw new ArgumentNullException(nameof(finalAction));

            finalAction(result);
            return result;
        }

#if UNIFP_UNITASK
        /// <summary>Execute async final function and return its result (UniTask version)</summary>
        public static async UniTask<TOut> Finally<T, TOut>(
            this Result<T> result,
            Func<Result<T>, UniTask<TOut>> finalFunc)
        {
            if (finalFunc == null)
                throw new ArgumentNullException(nameof(finalFunc));

            return await finalFunc(result);
        }

        /// <summary>Execute sync final function on async Result (UniTask version)</summary>
        public static async UniTask<TOut> Finally<T, TOut>(
            this UniTask<Result<T>> resultTask,
            Func<Result<T>, TOut> finalFunc)
        {
            if (finalFunc == null)
                throw new ArgumentNullException(nameof(finalFunc));

            var result = await resultTask;
            return finalFunc(result);
        }

        /// <summary>Execute async final function on async Result (UniTask version)</summary>
        public static async UniTask<TOut> Finally<T, TOut>(
            this UniTask<Result<T>> resultTask,
            Func<Result<T>, UniTask<TOut>> finalFunc)
        {
            if (finalFunc == null)
                throw new ArgumentNullException(nameof(finalFunc));

            var result = await resultTask;
            return await finalFunc(result);
        }

        /// <summary>Execute async side effect and return original Result (UniTask version)</summary>
        public static async UniTask<Result<T>> Finally<T>(
            this Result<T> result,
            Func<Result<T>, UniTask> finalAction)
        {
            if (finalAction == null)
                throw new ArgumentNullException(nameof(finalAction));

            await finalAction(result);
            return result;
        }

        /// <summary>Execute sync side effect on async Result and return original (UniTask version)</summary>
        public static async UniTask<Result<T>> Finally<T>(
            this UniTask<Result<T>> resultTask,
            Action<Result<T>> finalAction)
        {
            if (finalAction == null)
                throw new ArgumentNullException(nameof(finalAction));

            var result = await resultTask;
            finalAction(result);
            return result;
        }

        /// <summary>Execute async side effect on async Result and return original (UniTask version)</summary>
        public static async UniTask<Result<T>> Finally<T>(
            this UniTask<Result<T>> resultTask,
            Func<Result<T>, UniTask> finalAction)
        {
            if (finalAction == null)
                throw new ArgumentNullException(nameof(finalAction));

            var result = await resultTask;
            await finalAction(result);
            return result;
        }
#elif UNIFP_AWAITABLE
        /// <summary>Execute async final function and return its result (Unity Awaitable version - fallback)</summary>
        public static async Awaitable<TOut> Finally<T, TOut>(
            this Result<T> result,
            Func<Result<T>, Awaitable<TOut>> finalFunc)
        {
            if (finalFunc == null)
                throw new ArgumentNullException(nameof(finalFunc));

            return await finalFunc(result);
        }

        /// <summary>Execute sync final function on async Result (Awaitable version)</summary>
        public static async Awaitable<TOut> Finally<T, TOut>(
            this Awaitable<Result<T>> resultTask,
            Func<Result<T>, TOut> finalFunc)
        {
            if (finalFunc == null)
                throw new ArgumentNullException(nameof(finalFunc));

            var result = await resultTask;
            return finalFunc(result);
        }

        /// <summary>Execute async final function on async Result (Awaitable version)</summary>
        public static async Awaitable<TOut> Finally<T, TOut>(
            this Awaitable<Result<T>> resultTask,
            Func<Result<T>, Awaitable<TOut>> finalFunc)
        {
            if (finalFunc == null)
                throw new ArgumentNullException(nameof(finalFunc));

            var result = await resultTask;
            return await finalFunc(result);
        }

        /// <summary>Execute async side effect and return original Result (Awaitable version)</summary>
        public static async Awaitable<Result<T>> Finally<T>(
            this Result<T> result,
            Func<Result<T>, Awaitable> finalAction)
        {
            if (finalAction == null)
                throw new ArgumentNullException(nameof(finalAction));

            await finalAction(result);
            return result;
        }

        /// <summary>Execute sync side effect on async Result and return original (Awaitable version)</summary>
        public static async Awaitable<Result<T>> Finally<T>(
            this Awaitable<Result<T>> resultTask,
            Action<Result<T>> finalAction)
        {
            if (finalAction == null)
                throw new ArgumentNullException(nameof(finalAction));

            var result = await resultTask;
            finalAction(result);
            return result;
        }

        /// <summary>Execute async side effect on async Result and return original (Awaitable version)</summary>
        public static async Awaitable<Result<T>> Finally<T>(
            this Awaitable<Result<T>> resultTask,
            Func<Result<T>, Awaitable> finalAction)
        {
            if (finalAction == null)
                throw new ArgumentNullException(nameof(finalAction));

            var result = await resultTask;
            await finalAction(result);
            return result;
        }
#endif
    }
}
