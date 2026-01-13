using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UniFP.Performance
{
    /// <summary>
    /// Static delegate cache to avoid GC allocations in hot paths
    /// Store commonly used lambdas as static fields
    /// Thread-safe implementation using ConcurrentDictionary
    /// </summary>
    public static class DelegateCache
    {
        /// <summary>Check if value is greater than zero</summary>
        public static readonly Func<int, bool> IsPositive = x => x > 0;

        /// <summary>Check if value is not null</summary>
        public static Func<T, bool> IsNotNull<T>() where T : class
        {
            return _isNotNullCache<T>.Value;
        }

        private static class _isNotNullCache<T> where T : class
        {
            public static readonly Func<T, bool> Value = x => x != null;
        }

        /// <summary>Check if string is not null or empty</summary>
        public static readonly Func<string, bool> IsNotNullOrEmpty = s => !string.IsNullOrEmpty(s);

        /// <summary>Check if string is not null or whitespace</summary>
        public static readonly Func<string, bool> IsNotNullOrWhitespace = s => !string.IsNullOrWhiteSpace(s);

        /// <summary>Convert to string</summary>
        public static Func<T, string> ToString<T>()
        {
            return _toStringCache<T>.Value;
        }

        private static class _toStringCache<T>
        {
            public static readonly Func<T, string> Value = x => x?.ToString() ?? string.Empty;
        }

        /// <summary>Identity function</summary>
        public static Func<T, T> Identity<T>()
        {
            return _identityCache<T>.Value;
        }

        private static class _identityCache<T>
        {
            public static readonly Func<T, T> Value = x => x;
        }

        /// <summary>Thread-safe custom delegate cache</summary>
        private static readonly ConcurrentDictionary<string, Delegate> _customCache = new ConcurrentDictionary<string, Delegate>();

        /// <summary>
        /// Cache a delegate with a key (Thread-safe)
        /// Use this for frequently used lambdas
        /// </summary>
        public static TDelegate GetOrAdd<TDelegate>(string key, Func<TDelegate> factory) where TDelegate : Delegate
        {
            return (TDelegate)_customCache.GetOrAdd(key, _ => factory());
        }

        /// <summary>Clear custom cache (Thread-safe)</summary>
        public static void Clear()
        {
            _customCache.Clear();
        }
    }

    /// <summary>
    /// Usage examples for delegate caching
    /// </summary>
    public static class DelegateCacheExamples
    {
        // ❌ Bad - allocates every frame
        public static void BadExample(Result<int> result)
        {
            result.Filter(x => x > 0, "Must be positive");  // Lambda allocated!
        }

        // ✅ Good - reuses cached delegate
        public static void GoodExample(Result<int> result)
        {
            result.Filter(DelegateCache.IsPositive, "Must be positive");  // Zero allocation!
        }

        // ✅ Good - custom cached delegate
        private static readonly Func<User, bool> IsAdult = u => u.Age >= 18;

        public static void CustomExample(Result<User> result)
        {
            result.Filter(IsAdult, "Must be adult");  // Zero allocation!
        }

        public class User
        {
            public int Age { get; set; }
        }
    }
}
