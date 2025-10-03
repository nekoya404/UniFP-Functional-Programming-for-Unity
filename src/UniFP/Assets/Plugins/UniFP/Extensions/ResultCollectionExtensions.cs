using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace UniFP
{
    /// <summary>
    /// Collection extensions for Result monad
    /// </summary>
    public static class ResultCollectionExtensions
    {
        #region SelectResults

        public static Result<List<TResult>> SelectResults<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, Result<TResult>> selector)
        {
            var results = new List<TResult>();

            foreach (var item in source)
            {
                var result = selector(item);
                if (result.IsFailure)
                    return Result<List<TResult>>.Failure(result.ErrorCode);

                results.Add(result.Value);
            }

            return Result<List<TResult>>.Success(results);
        }

        public static async UniTask<Result<List<TResult>>> SelectResultsAsync<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, UniTask<Result<TResult>>> selector)
        {
            var results = new List<TResult>();

            foreach (var item in source)
            {
                var result = await selector(item);
                if (result.IsFailure)
                    return Result<List<TResult>>.Failure(result.ErrorCode);

                results.Add(result.Value);
            }

            return Result<List<TResult>>.Success(results);
        }

        #endregion

        #region CombineAll

        public static Result<List<T>> CombineAll<T>(this IEnumerable<Result<T>> results)
        {
            var values = new List<T>();

            foreach (var result in results)
            {
                if (result.IsFailure)
                    return Result<List<T>>.Failure(result.ErrorCode);

                values.Add(result.Value);
            }

            return Result<List<T>>.Success(values);
        }

        #endregion

        #region Filter

        public static Result<List<T>> FilterResults<T>(
            this IEnumerable<T> source,
            Func<T, Result<bool>> predicate)
        {
            var filtered = new List<T>();

            foreach (var item in source)
            {
                var result = predicate(item);
                if (result.IsFailure)
                    return Result<List<T>>.Failure(result.ErrorCode);

                if (result.Value)
                    filtered.Add(item);
            }

            return Result<List<T>>.Success(filtered);
        }

        #endregion

        #region Partition

        public static (List<T> successes, List<string> failures) Partition<T>(
            this IEnumerable<Result<T>> results)
        {
            var successes = new List<T>();
            var failures = new List<string>();

            foreach (var result in results)
            {
                if (result.IsSuccess)
                    successes.Add(result.Value);
                else
                    failures.Add(result.ErrorMessage);
            }

            return (successes, failures);
        }

        #endregion

        #region Fold

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

        #endregion

        #region Aggregate

        public static Result<T> AggregateResults<T>(
            this IEnumerable<Result<T>> results,
            Func<T, T, T> aggregator)
        {
            var list = results.ToList();
            
            if (list.Count == 0)
                return Result<T>.Failure("Cannot aggregate empty collection");

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

        #endregion

        #region SelectResults

        public static IEnumerable<T> SelectSuccesses<T>(this IEnumerable<Result<T>> results)
        {
            return results.Where(r => r.IsSuccess).Select(r => r.Value);
        }

        public static IEnumerable<string> SelectFailures<T>(this IEnumerable<Result<T>> results)
        {
            return results.Where(r => r.IsFailure).Select(r => r.ErrorMessage);
        }

        #endregion
    }
}
