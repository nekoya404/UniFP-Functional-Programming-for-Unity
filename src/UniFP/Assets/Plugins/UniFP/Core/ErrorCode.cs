namespace UniFP
{
    /// <summary>
    /// Performance-optimized error codes (zero allocation alternative to string errors)
    /// Use this for hot paths where GC allocation is critical
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>No error</summary>
        None = 0,
        
        /// <summary>Generic error</summary>
        Unknown = 1,
        
        /// <summary>Resource not found</summary>
        NotFound = 100,
        
        /// <summary>Invalid input or argument</summary>
        InvalidInput = 101,
        
        /// <summary>Validation failed</summary>
        ValidationFailed = 102,
        
        /// <summary>Operation not permitted</summary>
        Unauthorized = 103,
        
        /// <summary>Resource already exists</summary>
        AlreadyExists = 104,
        
        /// <summary>Insufficient resources (e.g., not enough currency)</summary>
        InsufficientResources = 105,
        
        /// <summary>Capacity exceeded (e.g., inventory full)</summary>
        Capacity = 106,
        
        /// <summary>Database error</summary>
        DatabaseError = 200,
        
        /// <summary>Network error</summary>
        NetworkError = 201,
        
        /// <summary>File I/O error</summary>
        FileError = 202,
        
        /// <summary>Timeout occurred</summary>
        Timeout = 203,
        
        /// <summary>Operation cancelled</summary>
        Cancelled = 204,
        
        /// <summary>Out of memory</summary>
        OutOfMemory = 300,
        
        /// <summary>Null reference</summary>
        NullReference = 301,
        
        /// <summary>Index out of range</summary>
        IndexOutOfRange = 302,
    }

#if UNITY_EDITOR || UNIFP_DEBUG
    /// <summary>
    /// Extension methods for ErrorCode (only available in Editor/Debug builds for debugging)
    /// </summary>
    public static class ErrorCodeExtensions
    {
        /// <summary>Convert ErrorCode to display string (allocates, only in Editor/Debug)</summary>
        public static string ToDisplayString(this ErrorCode errorCode)
        {
            return errorCode switch
            {
                ErrorCode.None => "No error",
                ErrorCode.Unknown => "Unknown error",
                ErrorCode.NotFound => "Resource not found",
                ErrorCode.InvalidInput => "Invalid input",
                ErrorCode.ValidationFailed => "Validation failed",
                ErrorCode.Unauthorized => "Unauthorized",
                ErrorCode.AlreadyExists => "Already exists",
                ErrorCode.InsufficientResources => "Insufficient resources",
                ErrorCode.Capacity => "Capacity exceeded",
                ErrorCode.DatabaseError => "Database error",
                ErrorCode.NetworkError => "Network error",
                ErrorCode.FileError => "File error",
                ErrorCode.Timeout => "Timeout",
                ErrorCode.Cancelled => "Cancelled",
                ErrorCode.OutOfMemory => "Out of memory",
                ErrorCode.NullReference => "Null reference",
                ErrorCode.IndexOutOfRange => "Index out of range",
                _ => $"Unknown error code: {(int)errorCode}"
            };
        }
    }
#endif
}
