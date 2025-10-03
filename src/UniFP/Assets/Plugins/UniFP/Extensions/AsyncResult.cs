using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Async extensions for Result monad with UniTask
    /// </summary>
    public static class AsyncResult
    {
        #region Async Bind

        /// <summary>Bind async function to Result</summary>
        public static UniTask<Result<TResult>> ThenAsync<TSource, TResult>(
            this Result<TSource> source,
            Func<TSource, UniTask<Result<TResult>>> binder,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

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
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

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

        #endregion

        #region Async Map

        /// <summary>Map with async function</summary>
        public static UniTask<Result<TResult>> MapAsync<TSource, TResult>(
            this Result<TSource> source,
            Func<TSource, UniTask<TResult>> mapper,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

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
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

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

        #endregion

        #region Async Do

        /// <summary>Execute async side effect on success</summary>
        public static async UniTask<Result<T>> DoAsync<T>(
            this Result<T> source,
            Func<T, UniTask> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (source.IsSuccess)
                await action(source.Value);

            return source;
        }

        /// <summary>Execute sync side effect on async result</summary>
        public static async UniTask<Result<T>> DoAsync<T>(
            this UniTask<Result<T>> sourceTask,
            Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

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
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var source = await sourceTask;
            if (source.IsSuccess)
                await action(source.Value);

            return source;
        }

        #endregion

        #region Async Filter

        /// <summary>Filter with async predicate</summary>
        public static async UniTask<Result<T>> FilterAsync<T>(
            this Result<T> source,
            Func<T, UniTask<bool>> predicate,
            string errorMessage,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if (string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentNullException(nameof(errorMessage));

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

        #endregion

        #region Async Try

        /// <summary>Convert async exceptions to Result</summary>
        public static UniTask<Result<T>> TryAsync<T>(
            Func<UniTask<T>> asyncFunc,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (asyncFunc == null)
                throw new ArgumentNullException(nameof(asyncFunc));

            return SafeExecutor.ExecuteAsync(
                async () => Result<T>.Success(await asyncFunc()),
                OperationType.Then,
                memberName,
                filePath,
                lineNumber);
        }

        #endregion
    }
}
