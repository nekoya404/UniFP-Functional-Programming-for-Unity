using System;
#if UNIFP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace UniFP
{
    /// <summary>
    /// Repeat operations for Result
    /// </summary>
    public static partial class RetryExtensions
    {
        /// <summary>Repeat operation N times</summary>
        public static Result<T> Repeat<T>(
            Func<Result<T>> operation,
            int count)
        {
            if (count <= 0)
                throw new ArgumentException("count must be greater than 0", nameof(count));

            Result<T> result = default;

            for (int i = 0; i < count; i++)
            {
                result = operation();
                if (result.IsFailure)
                    return result;
            }

            return result;
        }

#if UNIFP_UNITASK
        /// <summary>Repeat async operation N times</summary>
        public static async UniTask<Result<T>> RepeatAsync<T>(
            Func<UniTask<Result<T>>> operation,
            int count)
        {
            if (count <= 0)
                throw new ArgumentException("count must be greater than 0", nameof(count));

            Result<T> result = default;

            for (int i = 0; i < count; i++)
            {
                result = await operation();
                if (result.IsFailure)
                    return result;
            }

            return result;
        }
#endif
    }
}
