using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UniFP.Internal;

namespace UniFP
{
    /// <summary>
    /// Async Result extensions - TryAsync operations
    /// </summary>
    public static partial class AsyncResult
    {
        /// <summary>Convert async exceptions to Result</summary>
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
    }
}
