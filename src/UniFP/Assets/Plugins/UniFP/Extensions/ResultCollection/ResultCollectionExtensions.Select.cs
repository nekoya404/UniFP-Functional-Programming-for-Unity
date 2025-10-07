using System;
using System.Collections.Generic;
#if UNIFP_UNITASK
using Cysharp.Threading.Tasks;
#endif
using UniFP.Performance;

namespace UniFP
{
    /// <summary>
    /// Select operations for Result collections
    /// Uses ResultPool for GC-friendly list allocations
    /// Supports both UniTask (UNIFP_UNITASK) and Unity Awaitable (UNIFP_AWAITABLE)
    /// </summary>
    public static partial class ResultCollectionExtensions
    {
        /// <summary>
        /// Maps each element to a Result and collects successes
        /// Returns failure on first error
        /// </summary>
        public static Result<List<TResult>> SelectResults<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, Result<TResult>> selector)
        {
            return ResultPool.UsePooledList<TResult, Result<List<TResult>>>(pooledList =>
            {
                foreach (var item in source)
                {
                    var result = selector(item);
                    if (result.IsFailure)
                        return Result<List<TResult>>.Failure(result.ErrorCode);

                    pooledList.Add(result.Value);
                }

                return Result<List<TResult>>.Success(new List<TResult>(pooledList));
            });
        }

#if UNIFP_UNITASK
        /// <summary>
        /// Maps each element to an async Result and collects successes (UniTask version)
        /// Returns failure on first error
        /// </summary>
        public static async UniTask<Result<List<TResult>>> SelectResultsAsync<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, UniTask<Result<TResult>>> selector)
        {
            return await ResultPool.UsePooledList<TResult, UniTask<Result<List<TResult>>>>(async pooledList =>
            {
                foreach (var item in source)
                {
                    var result = await selector(item);
                    if (result.IsFailure)
                        return Result<List<TResult>>.Failure(result.ErrorCode);

                    pooledList.Add(result.Value);
                }

                return Result<List<TResult>>.Success(new List<TResult>(pooledList));
            });
        }
#elif UNIFP_AWAITABLE
        /// <summary>
        /// Maps each element to an async Result and collects successes (Unity Awaitable version - fallback)
        /// Returns failure on first error
        /// </summary>
        public static async Awaitable<Result<List<TResult>>> SelectResultsAsync<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, Awaitable<Result<TResult>>> selector)
        {
            return await ResultPool.UsePooledList<TResult, Awaitable<Result<List<TResult>>>>(async pooledList =>
            {
                foreach (var item in source)
                {
                    var result = await selector(item);
                    if (result.IsFailure)
                        return Result<List<TResult>>.Failure(result.ErrorCode);

                    pooledList.Add(result.Value);
                }

                return Result<List<TResult>>.Success(new List<TResult>(pooledList));
            });
        }
#endif
    }
}

