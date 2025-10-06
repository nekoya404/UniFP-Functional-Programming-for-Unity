using System;

namespace UniFP
{
    /// <summary>
    /// Error recovery operations for Result
    /// </summary>
    public static partial class ResultExtensions
    {
        /// <summary>Recover from failure with fallback - Zero GC</summary>
        public static Result<T> Recover<T>(
            this Result<T> source,
            Func<ErrorCode, T> fallback)
        {
            if (fallback == null)
                throw new ArgumentNullException(nameof(fallback));

            return source.IsSuccess
                ? source
                : Result<T>.Success(fallback(source.ErrorCode));
        }
    }
}
