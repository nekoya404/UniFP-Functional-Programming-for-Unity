using UnityEngine;
using UniFP;
using UniFP.Performance;
using System;
using System.Diagnostics;

namespace UniFP.Examples
{
    /// <summary>
    /// Zero GC performance optimization example.
    /// Highlights optimizations using ErrorCode, DelegateCache, Span, and related patterns.
    /// </summary>
    public class PerformanceExample : MonoBehaviour
    {
        void Start()
        {
            UnityEngine.Debug.Log("=== Performance Example ===");

            // Step 1: ErrorCode - zero GC error handling
            ErrorCodeExample();

            // Step 2: DelegateCache - cache lambdas
            DelegateCacheExample();

            // Step 3: Span - zero-allocation collections
            SpanExample();

            // Step 4: performance comparison
            PerformanceComparisonExample();
        }

        #region ErrorCode Example

        void ErrorCodeExample()
        {
            UnityEngine.Debug.Log("\n--- ErrorCode (Zero GC) Example ---");

            // ✅ Zero GC - use ErrorCode
            var result1 = Result.FromError<int>(ErrorCode.InvalidInput);
            UnityEngine.Debug.Log($"ErrorCode: {result1.ErrorCode}");

            // ⚠️ Allocates - string message (for debugging)
            var result2 = Result<int>.Failure("Custom error message");
            UnityEngine.Debug.Log($"Error Message: {result2.GetErrorMessage()}");

            // Branch based on ErrorCode (use if-else for struct comparison)
            result1.Match(
                onSuccess: v => UnityEngine.Debug.Log($"Success: {v}"),
                onFailure: (ErrorCode error) =>
                {
                    if (error.Equals(ErrorCode.InvalidInput))
                    {
                        UnityEngine.Debug.LogWarning("✓ Handled InvalidInput (Zero GC)");
                    }
                    else if (error.Equals(ErrorCode.NotFound))
                    {
                        UnityEngine.Debug.LogWarning("Not found");
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"Unknown: {error}");
                    }
                }
            );
        }

        #endregion

        #region DelegateCache Example

        void DelegateCacheExample()
        {
            UnityEngine.Debug.Log("\n--- DelegateCache Example ---");

            // ❌ Bad - allocate a new lambda each time (GC pressure)
            var bad = Result.FromValue(10)
                .Filter(x => x > 0, ErrorCode.ValidationFailed)  // new lambda
                .Filter(x => x < 100, ErrorCode.ValidationFailed);  // new lambda

            // ✅ Good - reuse cached delegates (zero GC)
            var lessThan100 = DelegateCache.GetOrAdd<Func<int, bool>>("LessThan100", () => value => value < 100);
            var good = Result.FromValue(10)
                .Filter(DelegateCache.IsPositive, ErrorCode.ValidationFailed)  // cached lambda
                .Filter(lessThan100, ErrorCode.ValidationFailed);  // cached lambda

            UnityEngine.Debug.Log($"✓ DelegateCache result: {good.Value}");

            // Create a custom cache entry
            var cachedFilter = DelegateCache.GetOrAdd<Func<int, bool>>("IsEven", () => value => value % 2 == 0);
            var evenResult = Result.FromValue(20)
                .Filter(cachedFilter, ErrorCode.ValidationFailed);

            UnityEngine.Debug.Log($"✓ Custom cached filter: {evenResult.Value}");
        }

        #endregion

        #region Span Example

        void SpanExample()
        {
            UnityEngine.Debug.Log("\n--- Span (Zero Allocation) Example ---");

            // ❌ Bad - use List (heap allocation)
            var list = new System.Collections.Generic.List<int> { 1, 2, 3, 4, 5 };

            // ✅ Good - use Span (stack allocation)
            Span<int> numbers = stackalloc int[5] { 1, 2, 3, 4, 5 };
            Span<int> results = stackalloc int[5];

            // Process via Span combinators
            var processedCount = SpanExtensions.SelectResults(
                numbers,
                results,
                x => x > 0 ? Result<int>.Success(x * 2) : Result<int>.Failure(ErrorCode.InvalidInput)
            );

            if (processedCount.IsSuccess)
            {
                UnityEngine.Debug.Log($"✓ Processed {processedCount.Value} items (Zero GC):");
                for (int i = 0; i < processedCount.Value; i++)
                {
                    UnityEngine.Debug.Log($"  - {results[i]}");
                }
            }
        }

        #endregion

        #region Performance Comparison

        void PerformanceComparisonExample()
        {
            UnityEngine.Debug.Log("\n--- Performance Comparison ---");

            const int iterations = 10000;

            // 1. String Error (Allocates)
            //    Generating string error messages incurs GC cost.
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = Result<int>.Failure("error");
                var recovered = result.Recover(_ => 0);
            }
            sw1.Stop();

            // 2. ErrorCode (Zero GC)
            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = Result.FromError<int>(ErrorCode.Unknown);
                var recovered = result.Recover((ErrorCode e) => 0);
            }
            sw2.Stop();

            UnityEngine.Debug.Log($"String Error: {sw1.ElapsedMilliseconds}ms (allocates)");
            UnityEngine.Debug.Log($"ErrorCode:    {sw2.ElapsedMilliseconds}ms (Zero GC)");
            UnityEngine.Debug.Log($"✓ ErrorCode is {sw1.ElapsedMilliseconds / (float)sw2.ElapsedMilliseconds:F1}x faster!");

            // 3. Lambda vs DelegateCache
            var sw3 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = Result.FromValue(10)
                    .Filter(x => x > 0, ErrorCode.ValidationFailed);  // new lambda
            }
            sw3.Stop();

            var sw4 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = Result.FromValue(10)
                    .Filter(DelegateCache.IsPositive, ErrorCode.ValidationFailed);  // cached lambda
            }
            sw4.Stop();

            UnityEngine.Debug.Log($"\nLambda:        {sw3.ElapsedMilliseconds}ms (allocates)");
            UnityEngine.Debug.Log($"DelegateCache: {sw4.ElapsedMilliseconds}ms (Zero GC)");
            UnityEngine.Debug.Log($"✓ DelegateCache is {sw3.ElapsedMilliseconds / (float)sw4.ElapsedMilliseconds:F1}x faster!");
        }

        #endregion
    }
}
