using System;

namespace UniFP
{
    /// <summary>
    /// Option monad: Represents a value that may or may not exist (alternative to null)
    /// </summary>
    public readonly struct Option<T>
    {
        private readonly bool _hasValue;
        private readonly T _value;

        #region Properties

        public bool IsSome => _hasValue;
        public bool IsNone => !_hasValue;

        public T Value
        {
            get
            {
                if (!_hasValue)
                    throw new InvalidOperationException("Cannot access Value when None");
                return _value;
            }
        }

        #endregion

        #region Construction

        private Option(bool hasValue, T value)
        {
            _hasValue = hasValue;
            _value = value;
        }

        public static Option<T> Some(T value)
        {
            return new Option<T>(true, value);
        }

        public static Option<T> None()
        {
            return new Option<T>(false, default);
        }

        public static Option<T> From(T value)
        {
            return value != null ? Some(value) : None();
        }

        #endregion

        #region Monad Operations

        public Option<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            return _hasValue ? Option<TResult>.Some(mapper(_value)) : Option<TResult>.None();
        }

        public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder)
        {
            return _hasValue ? binder(_value) : Option<TResult>.None();
        }

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

        #endregion

        #region Utility Methods

        public T GetValueOr(T defaultValue)
        {
            return _hasValue ? _value : defaultValue;
        }

        public T GetValueOrElse(Func<T> fallback)
        {
            return _hasValue ? _value : fallback();
        }

        public Option<T> Filter(Func<T, bool> predicate)
        {
            return _hasValue && predicate(_value) ? this : None();
        }

        public Option<T> OnSome(Action<T> action)
        {
            if (_hasValue)
                action(_value);
            return this;
        }

        public Option<T> OnNone(Action action)
        {
            if (!_hasValue)
                action();
            return this;
        }

        public Result<T> ToResult(string errorMessage)
        {
            return _hasValue ? Result<T>.Success(_value) : Result<T>.Failure(errorMessage);
        }

        #endregion

        #region Operators

        public static implicit operator bool(Option<T> option) => option._hasValue;

        #endregion

        public override string ToString()
        {
            return _hasValue ? $"Some({_value})" : "None";
        }
    }
}
