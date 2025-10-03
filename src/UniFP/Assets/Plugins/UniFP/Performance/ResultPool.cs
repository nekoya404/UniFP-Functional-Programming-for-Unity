using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace UniFP.Performance
{
    /// <summary>
    /// Object pooling utilities for Result collections to reduce GC allocations
    /// Use these for hot paths that process multiple Results
    /// </summary>
    public static class ResultPool
    {
        private static readonly ObjectPool<List<string>> _stringListPool = new ObjectPool<List<string>>(
            createFunc: () => new List<string>(),
            actionOnGet: list => list.Clear(),
            actionOnRelease: list => list.Clear(),
            actionOnDestroy: list => { },
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 100
        );

        /// <summary>
        /// Get a pooled List of strings
        /// IMPORTANT: Must call ReleaseStringList when done!
        /// </summary>
        public static List<string> GetStringList()
        {
            return _stringListPool.Get();
        }

        /// <summary>
        /// Return a List of strings to the pool
        /// </summary>
        public static void ReleaseStringList(List<string> list)
        {
            _stringListPool.Release(list);
        }

        /// <summary>
        /// Generic list pool (creates new pool for each type)
        /// </summary>
        public static class ListPool<T>
        {
            private static readonly ObjectPool<List<T>> _pool = new ObjectPool<List<T>>(
                createFunc: () => new List<T>(),
                actionOnGet: list => list.Clear(),
                actionOnRelease: list => list.Clear(),
                actionOnDestroy: list => { },
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 100
            );

            public static List<T> Get() => _pool.Get();
            public static void Release(List<T> list) => _pool.Release(list);

            /// <summary>
            /// Get list with initial capacity to avoid resizing
            /// </summary>
            public static List<T> Get(int capacity)
            {
                var list = _pool.Get();
                if (list.Capacity < capacity)
                    list.Capacity = capacity;
                return list;
            }
        }

        /// <summary>
        /// Helper to use pooled list with automatic release
        /// Exception-safe: list is cleared before release even if action throws
        /// </summary>
        public static TResult UsePooledList<T, TResult>(Func<List<T>, TResult> action)
        {
            var list = ListPool<T>.Get();
            try
            {
                return action(list);
            }
            catch
            {
                list.Clear();
                throw;
            }
            finally
            {
                ListPool<T>.Release(list);
            }
        }

        /// <summary>
        /// Helper to use pooled list with automatic release (with initial capacity)
        /// Exception-safe: list is cleared before release even if action throws
        /// </summary>
        public static TResult UsePooledList<T, TResult>(int capacity, Func<List<T>, TResult> action)
        {
            var list = ListPool<T>.Get(capacity);
            try
            {
                return action(list);
            }
            catch
            {
                list.Clear();
                throw;
            }
            finally
            {
                ListPool<T>.Release(list);
            }
        }
    }

    /// <summary>
    /// Usage examples for Result pooling
    /// </summary>
    public static class ResultPoolExamples
    {
        // ❌ Bad - allocates List every time
        public static Result<List<int>> BadExample(int[] items)
        {
            var results = new List<int>();  // Heap allocation!

            foreach (var item in items)
            {
                results.Add(item * 2);
            }

            return Result<List<int>>.Success(results);
        }

        // ✅ Good - uses pooled list
        public static Result<List<int>> GoodExample(int[] items)
        {
            return ResultPool.UsePooledList<int, Result<List<int>>>(items.Length, list =>
            {
                foreach (var item in items)
                {
                    list.Add(item * 2);
                }

                // Copy to new list before releasing pool
                var result = new List<int>(list);
                return Result<List<int>>.Success(result);
            });
        }

        // ✅ Best - manual control for maximum performance
        public static Result<List<int>> BestExample(int[] items)
        {
            var list = ResultPool.ListPool<int>.Get(items.Length);

            try
            {
                foreach (var item in items)
                {
                    list.Add(item * 2);
                }

                var result = new List<int>(list);
                return Result<List<int>>.Success(result);
            }
            finally
            {
                ResultPool.ListPool<int>.Release(list);
            }
        }
    }
}
