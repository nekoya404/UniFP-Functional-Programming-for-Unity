using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Async Result extensions - MapAsync operations
    /// </summary>
    public static partial class AsyncResult
    {
        /// <summary>Map with async function</summary>
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
    }
}
