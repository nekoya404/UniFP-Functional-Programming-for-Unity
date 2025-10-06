using System;

namespace UniFP
{
    /// <summary>
    /// Result monad operations - Map, Bind, Match
    /// </summary>
    public readonly partial struct Result<T>
    {
        /// <summary>Transform value on success</summary>
        public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            return IsSuccess
                ? Result<TResult>.Success(mapper(_value))
                : Result<TResult>.Failure(_errorCode, _customErrorMessage);
        }

        /// <summary>Chain with Result-returning function</summary>
        public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
        {
            return IsSuccess
                ? binder(_value)
                : Result<TResult>.Failure(_errorCode, _customErrorMessage);
        }

        /// <summary>Match success or failure case (Zero GC with ErrorCode callback)</summary>
        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<ErrorCode, TResult> onFailure)
        {
            return IsSuccess
                ? onSuccess(_value)
                : onFailure(_errorCode);
        }

        /// <summary>Match success or failure with side effects (Zero GC)</summary>
        public void Match(Action<T> onSuccess, Action<ErrorCode> onFailure)
        {
            if (IsSuccess)
                onSuccess(_value);
            else
                onFailure(_errorCode);
        }
    }
}
