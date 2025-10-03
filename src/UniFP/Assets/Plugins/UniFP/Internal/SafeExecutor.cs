using System;
using System.IO;
using System.Runtime.CompilerServices;
#if UNIFP_UNITASK
using Cysharp.Threading.Tasks;
#elif UNIFP_AWAITABLE
using UnityEngine;
#endif
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace UniFP.Internal
{
    /// <summary>
    /// Internal safe executor - executes operations with exception handling and conditional debugging
    /// Supports both UniTask (UNIFP_UNITASK) and Unity Awaitable (UNIFP_AWAITABLE)
    /// </summary>
    internal static class SafeExecutor
    {
        /// <summary>Execute function safely and wrap exception with location info</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Result<T> Execute<T>(
            Func<Result<T>> operation,
            OperationType operationType,
            string memberName = "",
            string filePath = "",
            int lineNumber = 0)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || UNIFP_DEBUG
                var location = $"{Path.GetFileName(filePath)}:{lineNumber} in {memberName}";
                var errorMsg = $"[{location}] {operationType.ToDisplayString()} exception: {ex.Message}";
                Debug.LogError($"{errorMsg}\n{ex.StackTrace}");
#endif
                return Result<T>.Failure(ErrorCode.Unknown);
            }
        }

#if UNIFP_UNITASK
        /// <summary>Execute async function safely and wrap exception with location info (UniTask version - preferred)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static async UniTask<Result<T>> ExecuteAsync<T>(
            Func<UniTask<Result<T>>> operation,
            OperationType operationType,
            string memberName = "",
            string filePath = "",
            int lineNumber = 0)
        {
            try
            {
                return await operation();
            }
            catch (OperationCanceledException)
            {
                return Result<T>.Failure(ErrorCode.OperationCanceled);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || UNIFP_DEBUG
                var location = $"{Path.GetFileName(filePath)}:{lineNumber} in {memberName}";
                var errorMsg = $"[{location}] {operationType.ToDisplayString()} exception: {ex.Message}";
                Debug.LogError($"{errorMsg}\n{ex.StackTrace}");
#endif
                return Result<T>.Failure(ErrorCode.Unknown);
            }
        }
#elif UNIFP_AWAITABLE
        /// <summary>Execute async function safely and wrap exception with location info (Unity Awaitable version - fallback)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static async Awaitable<Result<T>> ExecuteAsync<T>(
            Func<Awaitable<Result<T>>> operation,
            OperationType operationType,
            string memberName = "",
            string filePath = "",
            int lineNumber = 0)
        {
            try
            {
                return await operation();
            }
            catch (OperationCanceledException)
            {
                return Result<T>.Failure(ErrorCode.OperationCanceled);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || UNIFP_DEBUG
                var location = $"{Path.GetFileName(filePath)}:{lineNumber} in {memberName}";
                var errorMsg = $"[{location}] {operationType.ToDisplayString()} exception: {ex.Message}";
                Debug.LogError($"{errorMsg}\n{ex.StackTrace}");
#endif
                return Result<T>.Failure(ErrorCode.Unknown);
            }
        }
#endif

        /// <summary>Log validation failure with location info</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FormatValidationError(
            OperationType operationType,
            string message,
            string memberName = "",
            string filePath = "",
            int lineNumber = 0)
        {
#if UNITY_EDITOR || UNIFP_DEBUG
            var location = $"{Path.GetFileName(filePath)}:{lineNumber} in {memberName}";
            var errorMsg = $"[{location}] {operationType.ToDisplayString()} failed: {message}";
            Debug.LogWarning(errorMsg);
            return errorMsg;
#else
            return message;
#endif
        }
    }
}
