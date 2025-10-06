using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniFP.Performance;

namespace UniFP
{
    /// <summary>
    /// Select operations for Result collections
    /// Uses ResultPool for GC-friendly list allocations
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

        /// <summary>
        /// Maps each element to an async Result and collects successes
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
    }
}
