using System.Collections.Generic;
using System.Linq;

namespace UniFP
{
    /// <summary>
    /// Query operations for Result collections
    /// </summary>
    public static partial class ResultCollectionExtensions
    {
        /// <summary>
        /// Selects only successful values from Results
        /// </summary>
        public static IEnumerable<T> SelectSuccesses<T>(this IEnumerable<Result<T>> results)
        {
            return results.Where(r => r.IsSuccess).Select(r => r.Value);
        }

        /// <summary>
        /// Selects only error codes from failed Results
        /// </summary>
        public static IEnumerable<ErrorCode> SelectFailures<T>(this IEnumerable<Result<T>> results)
        {
            return results.Where(r => r.IsFailure).Select(r => r.ErrorCode);
        }
    }
}
