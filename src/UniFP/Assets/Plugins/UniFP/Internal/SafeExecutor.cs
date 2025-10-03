using System;
using System.IO;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace UniFP.Internal
{
    /// <summary>
    /// Internal safe executor - executes operations with exception handling and conditional debugging
    /// </summary>
    internal static class SafeExecutor
    {
#if UNITY_EDITOR || UNIFP_DEBUG
        /// <summary>Execute function safely and wrap exception with location info (editor-only)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Result<T> Execute<T>(
            Func<Result<T>> operation,
            OperationType operationType,
            string memberName,
            string filePath,
            int lineNumber)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                return HandleException<T>(ex, operationType, memberName, filePath, lineNumber);
            }
        }
#else
        /// <summary>Execute function safely (release mode - Zero GC)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Result<T> Execute<T>(
            Func<Result<T>> operation,
            OperationType operationType)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                // Release mode: return a minimal error message (operationType is ignored)
                return Result<T>.Failure(ex.Message);
            }
        }
#endif

#if UNITY_EDITOR || UNIFP_DEBUG
        /// <summary>Execute async function safely and wrap exception with location info (editor-only)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static async UniTask<Result<T>> ExecuteAsync<T>(
            Func<UniTask<Result<T>>> operation,
            OperationType operationType,
            string memberName,
            string filePath,
            int lineNumber)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                return HandleException<T>(ex, operationType, memberName, filePath, lineNumber);
            }
        }
#else
        /// <summary>Execute async function safely (release mode - Zero GC)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static async UniTask<Result<T>> ExecuteAsync<T>(
            Func<UniTask<Result<T>>> operation,
            OperationType operationType)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ex.Message);
            }
        }
#endif

#if UNITY_EDITOR || UNIFP_DEBUG
        /// <summary>Log validation failure (editor-only)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FormatValidationError(
            OperationType operationType,
            string message,
            string memberName,
            string filePath,
            int lineNumber)
        {
            var location = $"{Path.GetFileName(filePath)}:{lineNumber} in {memberName}";
            var errorMsg = $"[{location}] {operationType.ToDisplayString()} failed: {message}";
            Debug.LogWarning(errorMsg);
            return errorMsg;
        }
#else
        /// <summary>Format validation error (release mode - Zero GC)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FormatValidationError(
            OperationType operationType,
            string message)
        {
            // Release mode: ignore operationType and return a concise message
            return message;
        }
#endif

#if UNITY_EDITOR || UNIFP_DEBUG
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Result<T> HandleException<T>(
            Exception ex,
            OperationType operationType,
            string memberName,
            string filePath,
            int lineNumber)
        {
            var location = $"{Path.GetFileName(filePath)}:{lineNumber} in {memberName}";
            var errorMsg = $"[{location}] {operationType.ToDisplayString()} exception: {ex.Message}";
            Debug.LogError($"{errorMsg}\n{ex.StackTrace}");
            return Result<T>.Failure(errorMsg);
        }
#endif
    }
}
