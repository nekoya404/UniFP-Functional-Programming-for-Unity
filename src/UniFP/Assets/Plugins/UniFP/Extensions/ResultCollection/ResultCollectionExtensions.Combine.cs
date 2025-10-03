using System;
using System.Collections.Generic;
using UniFP.Performance;

namespace UniFP
{
    /// <summary>
    /// Combine and filter operations for Result collections
    /// Uses ResultPool for GC-friendly list allocations
    /// </summary>
    public static partial class ResultCollectionExtensions
    {
        /// <summary>
        /// Combines multiple Results into a single Result containing all values
        /// Returns failure on first error
        /// </summary>
        public static Result<List<T>> CombineAll<T>(this IEnumerable<Result<T>> results)
        {
            return ResultPool.UsePooledList<T, Result<List<T>>>(pooledList =>
            {
                foreach (var result in results)
                {
                    if (result.IsFailure)
                        return Result<List<T>>.Failure(result.ErrorCode);

                    pooledList.Add(result.Value);
                }

                return Result<List<T>>.Success(new List<T>(pooledList));
            });
        }

        /// <summary>
        /// Filters elements using a predicate that returns Result
        /// Returns failure on first error
        /// </summary>
        public static Result<List<T>> FilterResults<T>(
            this IEnumerable<T> source,
            Func<T, Result<bool>> predicate)
        {
            return ResultPool.UsePooledList<T, Result<List<T>>>(pooledList =>
            {
                foreach (var item in source)
                {
                    var result = predicate(item);
                    if (result.IsFailure)
                        return Result<List<T>>.Failure(result.ErrorCode);

                    if (result.Value)
                        pooledList.Add(item);
                }

                return Result<List<T>>.Success(new List<T>(pooledList));
            });
        }
    }
}
