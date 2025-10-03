using System;

namespace UniFP
{
    /// <summary>
    /// Zip operations for combining Results with custom functions
    /// </summary>
    public static partial class ResultExtensions
    {
        /// <summary>Zip 2 Results with a custom function</summary>
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

        /// <summary>Zip 3 Results with a custom function</summary>
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
    }
}
