using System;
#if UNIFP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace UniFP
{
    /// <summary>
    /// Asynchronous retry operations for Result
    /// Supports both UniTask (UNIFP_UNITASK) and Unity Awaitable (UNIFP_AWAITABLE)
    /// </summary>
    public static partial class RetryExtensions
    {
#if UNIFP_UNITASK
        /// <summary>Retry async operation with delay (UniTask version)</summary>
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

        /// <summary>Retry async operation with exponential backoff (UniTask version)</summary>
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
#elif UNIFP_AWAITABLE
        /// <summary>Retry async operation with delay (Unity Awaitable version - fallback)</summary>
        public static async Awaitable<Result<T>> RetryAsync<T>(
            Func<Awaitable<Result<T>>> operation,
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
                    await Awaitable.WaitForSecondsAsync(delayMilliseconds / 1000f);
            }

            return lastResult;
        }

        /// <summary>Retry async operation with exponential backoff (Awaitable version)</summary>
        public static async Awaitable<Result<T>> RetryWithBackoff<T>(
            Func<Awaitable<Result<T>>> operation,
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
                    await Awaitable.WaitForSecondsAsync(delay / 1000f);
                    delay = (int)(delay * backoffMultiplier);
                }
            }

            return lastResult;
        }
#endif
    }
}

