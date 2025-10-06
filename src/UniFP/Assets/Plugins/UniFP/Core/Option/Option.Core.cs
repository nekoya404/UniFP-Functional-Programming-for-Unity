using System;

namespace UniFP
{
    /// <summary>
    /// Option monad: Represents a value that may or may not exist (alternative to null)
    /// Zero-allocation struct-based implementation
    /// </summary>
    public readonly partial struct Option<T>
    {
        private readonly bool _hasValue;
        private readonly T _value;

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

        public static implicit operator bool(Option<T> option) => option._hasValue;

        public override string ToString()
        {
            return _hasValue ? $"Some({_value})" : "None";
        }
    }
}
