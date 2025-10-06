using System;
using Cysharp.Threading.Tasks;

namespace UniFP
{
    /// <summary>
    /// Asynchronous retry operations for Result
    /// </summary>
    public static partial class RetryExtensions
    {
        /// <summary>Retry async operation with delay</summary>
        public static async UniTask<Result<T>> RetryAsync<T>(
            Func<UniTask<Result<T>>> operation,
            int maxAttempts,
            int delayMilliseconds = 0)
        {
            if (maxAttempts <= 0)
                throw new ArgumentException("maxAttempts must be greater than 0", nameof(maxAttempts));

            Result<T> lastResult = default;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                lastResult = await operation();

                if (lastResult.IsSuccess)
                    return lastResult;

                if (attempt < maxAttempts - 1 && delayMilliseconds > 0)
                    await UniTask.Delay(delayMilliseconds);
            }

            return lastResult;
        }

        /// <summary>Retry async operation with exponential backoff</summary>
        public static async UniTask<Result<T>> RetryWithBackoff<T>(
            Func<UniTask<Result<T>>> operation,
            int maxAttempts,
            int initialDelayMilliseconds = 100,
            float backoffMultiplier = 2.0f)
        {
            if (maxAttempts <= 0)
                throw new ArgumentException("maxAttempts must be greater than 0", nameof(maxAttempts));

            Result<T> lastResult = default;
            int delay = initialDelayMilliseconds;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                lastResult = await operation();

                if (lastResult.IsSuccess)
                    return lastResult;

                if (attempt < maxAttempts - 1)
                {
                    await UniTask.Delay(delay);
                    delay = (int)(delay * backoffMultiplier);
                }
            }

            return lastResult;
        }
    }
}
