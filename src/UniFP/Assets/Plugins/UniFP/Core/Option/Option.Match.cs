using System;

namespace UniFP
{
    /// <summary>
    /// Pattern matching and value extraction for Option
    /// </summary>
    public readonly partial struct Option<T>
    {
        /// <summary>
        /// Execute the appropriate branch for Some and None cases.
        /// </summary>
        /// <typeparam name="TResult">Return type of the matching functions</typeparam>
        /// <param name="onSome">Function invoked when Option is Some</param>
        /// <param name="onNone">Function invoked when Option is None</param>
        /// <returns>Result of the executed branch</returns>
        public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone)
        {
            return _hasValue ? onSome(_value) : onNone();
        }

        /// <summary>
        /// Execute the provided actions for Some and None cases.
        /// </summary>
        /// <param name="onSome">Action invoked when Option is Some</param>
        /// <param name="onNone">Action invoked when Option is None</param>
        public void Match(Action<T> onSome, Action onNone)
        {
            if (_hasValue)
                onSome(_value);
            else
                onNone();
        }

        /// <summary>Get value or default</summary>
        public T GetValueOr(T defaultValue)
        {
            return _hasValue ? _value : defaultValue;
        }

        /// <summary>Get value or compute fallback</summary>
        public T GetValueOrElse(Func<T> fallback)
        {
            return _hasValue ? _value : fallback();
        }
    }
}
