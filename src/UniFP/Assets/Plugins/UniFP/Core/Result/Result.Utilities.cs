using System;

namespace UniFP
{
    /// <summary>
    /// Result utility methods - GetValueOr, OnSuccess, OnFailure, GetErrorMessage
    /// </summary>
    public readonly partial struct Result<T>
    {
        /// <summary>
        /// Get error message as string (ALLOCATES - use ErrorCode for zero-allocation)
        /// </summary>
        public string GetErrorMessage()
        {
            if (_isSuccess)
                return null;

            if (_customErrorMessage != null)
                return _customErrorMessage;

#if UNITY_EDITOR || UNIFP_DEBUG
            return _errorCode.ToDisplayString();
#else
            return _errorCode.ToString();
#endif
        }

        /// <summary>
        /// Try to format error message into a Span (Zero allocation)
        /// Returns false if buffer is too small
        /// </summary>
        public bool TryFormatError(Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;

            if (_isSuccess)
                return true;

            if (_customErrorMessage != null)
            {
                if (_customErrorMessage.Length > destination.Length)
                    return false;

                _customErrorMessage.AsSpan().CopyTo(destination);
                charsWritten = _customErrorMessage.Length;
                return true;
            }

            var errorCodeString = _errorCode.ToString();
            if (errorCodeString.Length > destination.Length)
                return false;

            errorCodeString.AsSpan().CopyTo(destination);
            charsWritten = errorCodeString.Length;
            return true;
        }

        /// <summary>Get value or default (Zero GC)</summary>
        public T GetValueOrDefault(T defaultValue = default)
        {
            return IsSuccess ? _value : defaultValue;
        }

        /// <summary>Get value or execute fallback (Zero GC if fallback doesn't allocate)</summary>
        public T GetValueOrElse(Func<ErrorCode, T> fallback)
        {
            return IsSuccess ? _value : fallback(_errorCode);
        }

        /// <summary>Execute action on failure (Zero GC)</summary>
        public Result<T> OnFailure(Action<ErrorCode> action)
        {
            if (IsFailure)
                action(_errorCode);

            return this;
        }

        /// <summary>Execute action on success (Zero GC)</summary>
        public Result<T> OnSuccess(Action<T> action)
        {
            if (IsSuccess)
                action(_value);

            return this;
        }
    }
}
