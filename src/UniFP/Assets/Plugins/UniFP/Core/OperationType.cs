namespace UniFP
{
    /// <summary>
    /// Result operation types (enum optimized for zero GC)
    /// </summary>
    public enum OperationType
    {
        /// <summary>Unknown operation</summary>
        Unknown = 0,

        // Factory Methods
        /// <summary>Result.FromValue</summary>
        FromValue,
        /// <summary>Result.FromError</summary>
        FromError,
        /// <summary>Result.TryFromValue</summary>
        TryFromValue,
        /// <summary>Result.TryFromResult</summary>
        TryFromResult,

        // Core Extensions
        /// <summary>Then</summary>
        Then,
        /// <summary>Map</summary>
        Map,
        /// <summary>Filter</summary>
        Filter,
        /// <summary>Do</summary>
        Do,
        /// <summary>Recover (formerly OrElse)</summary>
        Recover,
        /// <summary>IfFailed</summary>
        IfFailed,
        /// <summary>Match</summary>
        Match,

        // Railway Extensions
        /// <summary>Tap</summary>
        Tap,
        /// <summary>TapError</summary>
        TapError,
        /// <summary>ThenIf</summary>
        ThenIf,
        /// <summary>Ensure</summary>
        Ensure,

        // Collection Extensions
        /// <summary>SelectResults (formerly Traverse)</summary>
        SelectResults,
        /// <summary>CombineAll (formerly Sequence)</summary>
        CombineAll,
        /// <summary>FirstSuccess</summary>
        FirstSuccess,

        // Async Extensions
        /// <summary>ThenAsync</summary>
        ThenAsync,
        /// <summary>MapAsync</summary>
        MapAsync,
        /// <summary>FilterAsync</summary>
        FilterAsync,

        // Retry Extensions
        /// <summary>Retry</summary>
        Retry,
        /// <summary>RetryAsync</summary>
        RetryAsync,

        // Combinators
        /// <summary>Combine2</summary>
        Combine2,
        /// <summary>Combine3</summary>
        Combine3,
        /// <summary>Combine4</summary>
        Combine4,
    }

#if UNITY_EDITOR || UNIFP_DEBUG
    /// <summary>
    /// OperationType extension methods (debug only)
    /// </summary>
    public static class OperationTypeExtensions
    {
        /// <summary>
        /// Convert OperationType to display string (Editor/Debug only)
        /// </summary>
        public static string ToDisplayString(this OperationType operation)
        {
            return operation switch
            {
                OperationType.FromValue => "FromValue",
                OperationType.FromError => "FromError",
                OperationType.TryFromValue => "TryFromValue",
                OperationType.TryFromResult => "TryFromResult",
                OperationType.Then => "Then",
                OperationType.Map => "Map",
                OperationType.Filter => "Filter",
                OperationType.Do => "Do",
                OperationType.Recover => "Recover",
                OperationType.IfFailed => "IfFailed",
                OperationType.Match => "Match",
                OperationType.Tap => "Tap",
                OperationType.TapError => "TapError",
                OperationType.ThenIf => "ThenIf",
                OperationType.Ensure => "Ensure",
                OperationType.SelectResults => "SelectResults",
                OperationType.CombineAll => "CombineAll",
                OperationType.FirstSuccess => "FirstSuccess",
                OperationType.ThenAsync => "ThenAsync",
                OperationType.MapAsync => "MapAsync",
                OperationType.FilterAsync => "FilterAsync",
                OperationType.Retry => "Retry",
                OperationType.RetryAsync => "RetryAsync",
                OperationType.Combine2 => "Combine2",
                OperationType.Combine3 => "Combine3",
                OperationType.Combine4 => "Combine4",
                _ => "Unknown"
            };
        }
    }
#endif
}
