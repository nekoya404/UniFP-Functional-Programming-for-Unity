using System;
using System.Collections.Generic;
using System.Linq;

namespace UniFP
{
    /// <summary>
    /// Extensions for combining multiple Results
    /// </summary>
    public static class ResultCombinators
    {
        #region Combine

        public static Result<(T1, T2)> Combine<T1, T2>(
            Result<T1> result1,
            Result<T2> result2)
        {
            if (result1.IsFailure)
                return Result<(T1, T2)>.Failure(result1.ErrorCode);
            if (result2.IsFailure)
                return Result<(T1, T2)>.Failure(result2.ErrorCode);

            return Result<(T1, T2)>.Success((result1.Value, result2.Value));
        }

        public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(
            Result<T1> result1,
            Result<T2> result2,
            Result<T3> result3)
        {
            if (result1.IsFailure)
                return Result<(T1, T2, T3)>.Failure(result1.ErrorCode);
            if (result2.IsFailure)
                return Result<(T1, T2, T3)>.Failure(result2.ErrorCode);
            if (result3.IsFailure)
                return Result<(T1, T2, T3)>.Failure(result3.ErrorCode);

            return Result<(T1, T2, T3)>.Success((result1.Value, result2.Value, result3.Value));
        }

        public static Result<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(
            Result<T1> result1,
            Result<T2> result2,
            Result<T3> result3,
            Result<T4> result4)
        {
            if (result1.IsFailure)
                return Result<(T1, T2, T3, T4)>.Failure(result1.ErrorCode);
            if (result2.IsFailure)
                return Result<(T1, T2, T3, T4)>.Failure(result2.ErrorCode);
            if (result3.IsFailure)
                return Result<(T1, T2, T3, T4)>.Failure(result3.ErrorCode);
            if (result4.IsFailure)
                return Result<(T1, T2, T3, T4)>.Failure(result4.ErrorCode);

            return Result<(T1, T2, T3, T4)>.Success((
                result1.Value,
                result2.Value,
                result3.Value,
                result4.Value));
        }

        #endregion

        #region Zip

        public static Result<TResult> Zip<T1, T2, TResult>(
            this Result<T1> result1,
            Result<T2> result2,
            Func<T1, T2, TResult> zipper)
        {
            if (result1.IsFailure)
                return Result<TResult>.Failure(result1.ErrorCode);
            if (result2.IsFailure)
                return Result<TResult>.Failure(result2.ErrorCode);

            return Result<TResult>.Success(zipper(result1.Value, result2.Value));
        }

        public static Result<TResult> Zip<T1, T2, T3, TResult>(
            this Result<T1> result1,
            Result<T2> result2,
            Result<T3> result3,
            Func<T1, T2, T3, TResult> zipper)
        {
            if (result1.IsFailure)
                return Result<TResult>.Failure(result1.ErrorCode);
            if (result2.IsFailure)
                return Result<TResult>.Failure(result2.ErrorCode);
            if (result3.IsFailure)
                return Result<TResult>.Failure(result3.ErrorCode);

            return Result<TResult>.Success(zipper(result1.Value, result2.Value, result3.Value));
        }

        #endregion

        #region CombineAll

        public static Result<List<T>> CombineAll<T>(params Result<T>[] results)
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

        public static Result<List<T>> CombineAll<T>(IEnumerable<Result<T>> results)
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

        #region FirstSuccess

        public static Result<T> FirstSuccess<T>(params Result<T>[] results)
        {
            foreach (var result in results)
            {
                if (result.IsSuccess)
                    return result;
            }

            return results.Length > 0
                ? results[results.Length - 1]
                : Result<T>.Failure("No results provided");
        }

        #endregion
    }
}
