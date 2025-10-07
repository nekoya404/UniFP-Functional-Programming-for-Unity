using System;

namespace UniFP
{
    /// <summary>
    /// Result combinator methods for merging multiple Results
    /// </summary>
    public readonly partial struct Result<T>
    {
        // Note: For extension methods like Zip, see ResultExtensions.Zip.cs
    }

    /// <summary>
    /// Static combinator methods for Result
    /// </summary>
    public static partial class Result
    {
        /// <summary>Combine 2 Results into a tuple</summary>
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

        /// <summary>Combine 2 Results into a tuple - NOPE compatibility alias</summary>
        public static Result<(T1, T2)> CombineValues<T1, T2>(
            Result<T1> result1,
            Result<T2> result2) => Combine(result1, result2);

        /// <summary>Combine 3 Results into a tuple</summary>
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

        /// <summary>Combine 3 Results into a tuple - NOPE compatibility alias</summary>
        public static Result<(T1, T2, T3)> CombineValues<T1, T2, T3>(
            Result<T1> result1,
            Result<T2> result2,
            Result<T3> result3) => Combine(result1, result2, result3);

        /// <summary>Combine 4 Results into a tuple</summary>
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

        /// <summary>Combine 4 Results into a tuple - NOPE compatibility alias</summary>
        public static Result<(T1, T2, T3, T4)> CombineValues<T1, T2, T3, T4>(
            Result<T1> result1,
            Result<T2> result2,
            Result<T3> result3,
            Result<T4> result4) => Combine(result1, result2, result3, result4);

        /// <summary>
        /// Returns the first successful Result, or the last failure if all fail
        /// </summary>
        public static Result<T> FirstSuccess<T>(params Result<T>[] results)
        {
            foreach (var result in results)
            {
                if (result.IsSuccess)
                    return result;
            }

            return results.Length > 0
                ? results[results.Length - 1]
                : Result<T>.Failure(ErrorCode.InvalidInput);
        }
    }
}
