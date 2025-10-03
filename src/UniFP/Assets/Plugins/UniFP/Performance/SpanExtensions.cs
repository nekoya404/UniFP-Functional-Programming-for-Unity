using System;
using System.Collections.Generic;

namespace UniFP.Performance
{
    /// <summary>
    /// Span-based Result extensions for zero-allocation collection processing
    /// Use these for performance-critical hot paths
    /// NOTE: Span<T> is stack-allocated and cannot be stored in fields or async methods
    /// </summary>
    public static class SpanExtensions
    {
        /// <summary>
        /// Process span of items with Result-returning selector (zero List allocation)
        /// Returns early on first failure
        /// </summary>
        public static Result<int> SelectResults<TSource>(
            this ReadOnlySpan<TSource> source,
            Span<TSource> destination,
            Func<TSource, Result<TSource>> selector)
        {
            if (destination.Length < source.Length)
                return Result<int>.Failure("Destination span is too small");

            for (int i = 0; i < source.Length; i++)
            {
                var result = selector(source[i]);
                if (result.IsFailure)
                    return Result<int>.Failure(result.ErrorCode);

                destination[i] = result.Value;
            }

            return Result<int>.Success(source.Length);
        }

        /// <summary>
        /// Map span of items (zero allocation for transformation)
        /// </summary>
        public static void Map<TSource, TResult>(
            this ReadOnlySpan<TSource> source,
            Span<TResult> destination,
            Func<TSource, TResult> mapper)
        {
            if (destination.Length < source.Length)
                throw new ArgumentException("Destination span is too small");

            for (int i = 0; i < source.Length; i++)
            {
                destination[i] = mapper(source[i]);
            }
        }

        /// <summary>
        /// Filter span of items to destination (zero allocation)
        /// Returns number of items that passed the filter
        /// </summary>
        public static int Filter<T>(
            this ReadOnlySpan<T> source,
            Span<T> destination,
            Func<T, bool> predicate)
        {
            int writeIndex = 0;

            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    if (writeIndex >= destination.Length)
                        return writeIndex; // Destination full

                    destination[writeIndex++] = source[i];
                }
            }

            return writeIndex;
        }

        /// <summary>
        /// Count items that match predicate (zero allocation)
        /// </summary>
        public static int Count<T>(
            this ReadOnlySpan<T> source,
            Func<T, bool> predicate)
        {
            int count = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Check if any item matches predicate (zero allocation)
        /// </summary>
        public static bool Any<T>(
            this ReadOnlySpan<T> source,
            Func<T, bool> predicate)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if all items match predicate (zero allocation)
        /// </summary>
        public static bool All<T>(
            this ReadOnlySpan<T> source,
            Func<T, bool> predicate)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i]))
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Usage examples for Span-based processing
    /// </summary>
    public static class SpanExamples
    {
        // ❌ Bad - allocates List
        public static List<int> BadExample(int[] items)
        {
            var results = new List<int>();  // Heap allocation!
            foreach (var item in items)
            {
                results.Add(item * 2);
            }
            return results;
        }

        // ✅ Good - uses Span (zero allocation)
        public static void GoodExample(ReadOnlySpan<int> items, Span<int> results)
        {
            items.Map(results, x => x * 2);  // Zero allocation!
        }

        // ✅ Best - stackalloc for small arrays
        public static void BestExample(ReadOnlySpan<int> items)
        {
            Span<int> results = stackalloc int[items.Length];  // Stack allocation!
            items.Map(results, x => x * 2);
            
            // Use results...
        }

        // ⚠️ Warning - stackalloc only for small sizes!
        public static void DangerousExample(ReadOnlySpan<int> items)
        {
            if (items.Length > 1024)
                throw new ArgumentException("Too large for stack allocation!");
                
            Span<int> results = stackalloc int[items.Length];
            items.Map(results, x => x * 2);
        }

        // ✅ Safe - use array pool for large data
        public static void SafeLargeExample(ReadOnlySpan<int> items)
        {
            var buffer = System.Buffers.ArrayPool<int>.Shared.Rent(items.Length);
            try
            {
                var results = buffer.AsSpan(0, items.Length);
                items.Map(results, x => x * 2);
                
                // Use results...
            }
            finally
            {
                System.Buffers.ArrayPool<int>.Shared.Return(buffer);
            }
        }
    }
}
