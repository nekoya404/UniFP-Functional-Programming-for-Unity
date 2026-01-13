using System;

namespace UniFP
{
    /// <summary>
    /// Side effects and conversion operations for Option
    /// </summary>
    public readonly partial struct Option<T>
    {
        /// <summary>Execute side effect on Some</summary>
        public Option<T> OnSome(Action<T> action)
        {
            if (_hasValue)
                action(_value);
            return this;
        }

        /// <summary>Execute side effect on None</summary>
        public Option<T> OnNone(Action action)
        {
            if (!_hasValue)
                action();
            return this;
        }

        /// <summary>
        /// Convert Option to Result with error code for None case
        /// Use this when None represents a specific error condition
        /// </summary>
        public Result<T> ToResult(ErrorCode errorCode)
        {
            return _hasValue ? Result<T>.Success(_value) : Result<T>.Failure(errorCode);
        }

        /// <summary>
        /// Convert Option to Result, treating None as success with default value
        /// Use when None is a valid state, not an error
        /// </summary>
        public Result<T> ToResultOrDefault(T defaultValue)
        {
            return Result<T>.Success(_hasValue ? _value : defaultValue);
        }

        /// <summary>
        /// Convert Option to Result, using a factory for default value when None
        /// </summary>
        public Result<T> ToResultOrElse(Func<T> defaultFactory)
        {
            return Result<T>.Success(_hasValue ? _value : defaultFactory());
        }
    }
}
