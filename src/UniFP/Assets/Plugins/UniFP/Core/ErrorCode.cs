using System;

namespace UniFP
{
    /// <summary>
    /// Extensible error code structure (Zero GC)
    /// Allows both built-in and custom error codes
    /// </summary>
    public readonly struct ErrorCode : IEquatable<ErrorCode>
    {
        private readonly int _code;
        private readonly string _category;

        private ErrorCode(int code, string category = null)
        {
            _code = code;
            _category = category;
        }

        // Built-in error codes (0-999 reserved)
        public static readonly ErrorCode None = new ErrorCode(0, "System");
        public static readonly ErrorCode Unknown = new ErrorCode(1, "System");
        public static readonly ErrorCode NotFound = new ErrorCode(100, "Validation");
        public static readonly ErrorCode InvalidInput = new ErrorCode(101, "Validation");
        public static readonly ErrorCode ValidationFailed = new ErrorCode(102, "Validation");
        public static readonly ErrorCode Unauthorized = new ErrorCode(103, "Security");
        public static readonly ErrorCode Forbidden = new ErrorCode(107, "Security");
        public static readonly ErrorCode InvalidOperation = new ErrorCode(108, "Validation");
        public static readonly ErrorCode AlreadyExists = new ErrorCode(104, "Validation");
        public static readonly ErrorCode InsufficientResources = new ErrorCode(105, "Resource");
        public static readonly ErrorCode Capacity = new ErrorCode(106, "Resource");
        public static readonly ErrorCode DatabaseError = new ErrorCode(200, "Database");
        public static readonly ErrorCode NetworkError = new ErrorCode(201, "Network");
        public static readonly ErrorCode FileError = new ErrorCode(202, "IO");
        public static readonly ErrorCode Timeout = new ErrorCode(203, "Network");
        public static readonly ErrorCode Cancelled = new ErrorCode(204, "System");
        public static readonly ErrorCode OperationCanceled = new ErrorCode(204, "System");
        public static readonly ErrorCode OutOfMemory = new ErrorCode(300, "System");
        public static readonly ErrorCode NullReference = new ErrorCode(301, "System");
        public static readonly ErrorCode IndexOutOfRange = new ErrorCode(302, "System");

        /// <summary>
        /// Create a custom error code (use codes >= 1000 to avoid conflicts)
        /// </summary>
        public static ErrorCode Custom(int code, string category = "Custom")
        {
            if (code < 1000)
                throw new ArgumentException("Custom error codes must be >= 1000 to avoid conflicts with built-in codes");
            return new ErrorCode(code, category);
        }

        public int Code => _code;
        public string Category => _category ?? "Unknown";

        public bool Equals(ErrorCode other)
        {
            return _code == other._code;
        }

        public override bool Equals(object obj)
        {
            return obj is ErrorCode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _code;
        }

        public static bool operator ==(ErrorCode left, ErrorCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ErrorCode left, ErrorCode right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return _category != null ? $"{_category}:{_code}" : _code.ToString();
        }

#if UNITY_EDITOR || UNIFP_DEBUG
        public string ToDisplayString()
        {
            return (_code, _category) switch
            {
                (0, _) => "No error",
                (1, _) => "Unknown error",
                (100, _) => "Resource not found",
                (101, _) => "Invalid input",
                (102, _) => "Validation failed",
                (103, _) => "Unauthorized",
                (104, _) => "Already exists",
                (105, _) => "Insufficient resources",
                (106, _) => "Capacity exceeded",
                (107, _) => "Forbidden",
                (108, _) => "Invalid operation",
                (200, _) => "Database error",
                (201, _) => "Network error",
                (202, _) => "File error",
                (203, _) => "Timeout",
                (204, _) => "Cancelled",
                (300, _) => "Out of memory",
                (301, _) => "Null reference",
                (302, _) => "Index out of range",
                _ => _category != null ? $"{_category} error (code: {_code})" : $"Error code: {_code}"
            };
        }
#endif
    }
}
