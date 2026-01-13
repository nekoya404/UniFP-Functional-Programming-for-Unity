using System;

namespace UniFP
{
    /// <summary>
    /// Result core - Fields, Properties, and Constructor
    /// </summary>
    public readonly partial struct Result<T>
    {
        private readonly bool _isSuccess;
        private readonly T _value;
        private readonly ErrorCode _errorCode;
        private readonly string _customErrorMessage;

        /// <summary>Check if successful</summary>
        public bool IsSuccess => _isSuccess;

        /// <summary>Check if failed</summary>
        public bool IsFailure => !_isSuccess;

        /// <summary>Get value (throws on failure)</summary>
        public T Value
        {
            get
            {
                if (!_isSuccess)
                    throw new InvalidOperationException($"Cannot access Value in failure state. Error: {GetErrorMessage()}");
                return _value;
            }
        }

        /// <summary>Get error code (Zero GC)</summary>
        public ErrorCode ErrorCode
        {
            get
            {
                if (_isSuccess)
                    return ErrorCode.None;
                return _errorCode;
            }
        }

        private Result(bool isSuccess, T value, ErrorCode errorCode, string customErrorMessage)
        {
            _isSuccess = isSuccess;
            _value = value;
            _errorCode = errorCode;
            _customErrorMessage = customErrorMessage;
        }

        public static implicit operator bool(Result<T> result) => result.IsSuccess;

        public override string ToString()
        {
            return IsSuccess
                ? $"Success({_value})"
                : _customErrorMessage != null
                    ? $"Failure({_customErrorMessage})"
                    : $"Failure({_errorCode})";
        }
    }
}
