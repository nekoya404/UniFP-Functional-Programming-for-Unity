using System;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine;

namespace UniFP
{
    /// <summary>
    /// Pipeline debugging utilities
    /// </summary>
    public static class PipelineDebug
    {

        /// <summary>Trace Result at this point in pipeline</summary>
        public static Result<T> Trace<T>(
            this Result<T> result,
            string label = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            var location = $"{Path.GetFileName(file)}:{line}";
            var prefix = string.IsNullOrEmpty(label) ? "" : $"[{label}] ";
            
            if (result.IsSuccess)
                Debug.Log($"✓ {location} {prefix}Success: {result.Value}");
            else
                Debug.LogWarning($"✗ {location} {prefix}Failure: {result.ErrorCode}");
            
            return result;
        }

        /// <summary>Trace with custom formatter</summary>
        public static Result<T> TraceWith<T>(
            this Result<T> result,
            Func<T, string> formatter,
            string label = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            var location = $"{Path.GetFileName(file)}:{line}";
            var prefix = string.IsNullOrEmpty(label) ? "" : $"[{label}] ";
            
            if (result.IsSuccess)
                Debug.Log($"✓ {location} {prefix}{formatter(result.Value)}");
            else
                Debug.LogWarning($"✗ {location} {prefix}Failure: {result.ErrorCode}");
            
            return result;
        }

        /// <summary>Assert condition in pipeline</summary>
        public static Result<T> Assert<T>(
            this Result<T> result,
            Func<T, bool> condition,
            string message = "Assertion failed",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (result.IsSuccess && !condition(result.Value))
            {
                var location = $"{Path.GetFileName(file)}:{line}";
                Debug.LogError($"⚠ {location} ASSERT: {message}");
            }
            return result;
        }

        /// <summary>Breakpoint in pipeline (editor only)</summary>
        public static Result<T> Breakpoint<T>(
            this Result<T> result,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            #if UNITY_EDITOR
            if (result.IsFailure)
            {
                var location = $"{Path.GetFileName(file)}:{line}";
                Debug.LogWarning($"🔴 BREAKPOINT: {location}");
                Debug.Break();
            }
            #endif
            return result;
        }



        /// <summary>Trace Option at this point in pipeline</summary>
        public static Option<T> Trace<T>(
            this Option<T> option,
            string label = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            var location = $"{Path.GetFileName(file)}:{line}";
            var prefix = string.IsNullOrEmpty(label) ? "" : $"[{label}] ";
            
            if (option.IsSome)
                Debug.Log($"✓ {location} {prefix}Some: {option.Value}");
            else
                Debug.LogWarning($"○ {location} {prefix}None");
            
            return option;
        }



        /// <summary>Measure execution time</summary>
        public static Result<T> TraceTime<T>(
            this Result<T> result,
            string label = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            // Time is measured when this is chained with operations
            var location = $"{Path.GetFileName(file)}:{line}";
            var prefix = string.IsNullOrEmpty(label) ? "" : $"[{label}] ";
            Debug.Log($"⏱ {location} {prefix}Checkpoint");
            return result;
        }

        /// <summary>Wrap function with timing</summary>
        public static Func<T, Result<TResult>> Timed<T, TResult>(
            Func<T, Result<TResult>> func,
            string label = "")
        {
            return input =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var result = func(input);
                sw.Stop();
                
                var prefix = string.IsNullOrEmpty(label) ? "" : $"[{label}] ";
                Debug.Log($"⏱ {prefix}Execution time: {sw.ElapsedMilliseconds}ms");
                
                return result;
            };
        }



        /// <summary>Trace only on failure</summary>
        public static Result<T> TraceOnFailure<T>(
            this Result<T> result,
            string label = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (result.IsFailure)
            {
                var location = $"{Path.GetFileName(file)}:{line}";
                var prefix = string.IsNullOrEmpty(label) ? "" : $"[{label}] ";
                Debug.LogError($"✗ {location} {prefix}Failure: {result.ErrorCode}");
            }
            return result;
        }

        /// <summary>Trace only on success</summary>
        public static Result<T> TraceOnSuccess<T>(
            this Result<T> result,
            string label = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (result.IsSuccess)
            {
                var location = $"{Path.GetFileName(file)}:{line}";
                var prefix = string.IsNullOrEmpty(label) ? "" : $"[{label}] ";
                Debug.Log($"✓ {location} {prefix}Success: {result.Value}");
            }
            return result;
        }

    }
}
