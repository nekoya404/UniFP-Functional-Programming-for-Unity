using System;

namespace UniFP
{
    /// <summary>
    /// Monadic operations for Option
    /// </summary>
    public readonly partial struct Option<T>
    {
        /// <summary>Transform value if Some</summary>
        public Option<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            return _hasValue ? Option<TResult>.Some(mapper(_value)) : Option<TResult>.None();
        }

        /// <summary>Bind with Option-returning function</summary>
        public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder)
        {
            return _hasValue ? binder(_value) : Option<TResult>.None();
        }

        /// <summary>Filter based on predicate</summary>
        public Option<T> Filter(Func<T, bool> predicate)
        {
            return _hasValue && predicate(_value) ? this : None();
        }
    }
}
