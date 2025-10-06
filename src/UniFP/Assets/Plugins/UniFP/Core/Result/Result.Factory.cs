using System;

namespace UniFP
{
    /// <summary>
    /// Result factory methods - Success, Failure creation
    /// </summary>
    public readonly partial struct Result<T>
    {
        /// <summary>Create success result (Zero GC)</summary>
        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, ErrorCode.None, null);
        }

        /// <summary>Create failure result with error code (Zero GC)</summary>
        public static Result<T> Failure(ErrorCode errorCode)
        {
            return new Result<T>(false, default, errorCode, null);
        }

        /// <summary>
        /// Create failure result with custom error message (allocates string)
        /// Use ErrorCode overload for Zero GC
        /// </summary>
        public static Result<T> Failure(string customErrorMessage)
        {
            if (string.IsNullOrWhiteSpace(customErrorMessage))
                throw new ArgumentNullException(nameof(customErrorMessage), "Error message cannot be null or empty.");

            return new Result<T>(false, default, ErrorCode.Unknown, customErrorMessage);
        }

        /// <summary>Internal: Create failure with both ErrorCode and custom message</summary>
        internal static Result<T> Failure(ErrorCode errorCode, string customErrorMessage)
        {
            return new Result<T>(false, default, errorCode, customErrorMessage);
        }
    }
}
