using System;
using System.Collections.Generic;
using System.Linq;

namespace UniFP
{
    /// <summary>
    /// Aggregate and fold operations for Result collections
    /// </summary>
    public static partial class ResultCollectionExtensions
    {
        /// <summary>
        /// Partitions Results into successes and failures
        /// </summary>
        public static (List<T> successes, List<ErrorCode> failures) Partition<T>(
            this IEnumerable<Result<T>> results)
        {
            var successes = new List<T>();
            var failures = new List<ErrorCode>();

            foreach (var result in results)
            {
                if (result.IsSuccess)
                    successes.Add(result.Value);
                else
                    failures.Add(result.ErrorCode);
            }

            return (successes, failures);
        }

        /// <summary>
        /// Folds over a collection with a Result-returning folder function
        /// Returns failure on first error
        /// </summary>
        public static Result<TAccumulate> Fold<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, Result<TAccumulate>> folder)
        {
            var accumulator = seed;

            foreach (var item in source)
            {
                var result = folder(accumulator, item);
                if (result.IsFailure)
                    return result;

                accumulator = result.Value;
            }

            return Result<TAccumulate>.Success(accumulator);
        }

        /// <summary>
        /// Aggregates Results using an aggregator function
        /// Returns failure on first error
        /// </summary>
        public static Result<T> AggregateResults<T>(
            this IEnumerable<Result<T>> results,
            Func<T, T, T> aggregator)
        {
            var list = results.ToList();

            if (list.Count == 0)
                return Result<T>.Failure(ErrorCode.InvalidInput);

            var firstResult = list[0];
            if (firstResult.IsFailure)
                return firstResult;

            var accumulator = firstResult.Value;

            for (int i = 1; i < list.Count; i++)
            {
                var result = list[i];
                if (result.IsFailure)
                    return result;

                accumulator = aggregator(accumulator, result.Value);
            }

            return Result<T>.Success(accumulator);
        }
    }
}
