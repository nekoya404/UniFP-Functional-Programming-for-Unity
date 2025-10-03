using System;

namespace UniFP
{
    /// <summary>
    /// Side effect operations for Result
    /// </summary>
    public static partial class ResultExtensions
    {
        /// <summary>Execute side effect on success (failure is ignored)</summary>
        public static Result<T> Do<T>(this Result<T> source, Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (source.IsSuccess)
                action(source.Value);

            return source;
        }

        /// <summary>Execute side effect on failure - Zero GC</summary>
        public static Result<T> DoOnError<T>(this Result<T> source, Action<ErrorCode> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (source.IsFailure)
                action(source.ErrorCode);

            return source;
        }
    }
}
