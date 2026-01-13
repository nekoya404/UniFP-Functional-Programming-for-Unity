using System;
using System.Runtime.CompilerServices;
#if UNIFP_UNITASK
using Cysharp.Threading.Tasks;
#elif UNIFP_AWAITABLE
using UnityEngine;
#endif
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Async Result extensions - TryAsync operations
    /// Supports both UniTask (UNIFP_UNITASK) and Unity Awaitable (UNIFP_AWAITABLE)
    /// </summary>
    public static partial class AsyncResult
    {
#if UNIFP_UNITASK
        /// <summary>Convert async exceptions to Result (UniTask version)</summary>
        public static UniTask<Result<T>> TryAsync<T>(
            Func<UniTask<T>> asyncFunc,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return SafeExecutor.ExecuteAsync(
                async () => Result<T>.Success(await asyncFunc()),
                OperationType.Then,
                memberName,
                filePath,
                lineNumber);
        }
#elif UNIFP_AWAITABLE
        /// <summary>Convert async exceptions to Result (Unity Awaitable version - fallback)</summary>
        public static Awaitable<Result<T>> TryAsync<T>(
            Func<Awaitable<T>> asyncFunc,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            return SafeExecutor.ExecuteAsync(
                async () => Result<T>.Success(await asyncFunc()),
                OperationType.Then,
                memberName,
                filePath,
                lineNumber);
        }
#endif
    }
}
