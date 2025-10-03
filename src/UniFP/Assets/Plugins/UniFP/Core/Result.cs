using System;
using UnityEngine;

namespace UniFP
{
    /// <summary>
    /// Represents either Success or Failure state with Zero GC error handling
    /// Uses ErrorCode enum internally for zero allocation
    /// </summary>
    public readonly struct Result<T>
    {
        #region Fields

        private readonly bool _isSuccess;
        private readonly T _value;
        private readonly ErrorCode _errorCode;
        private readonly string _customErrorMessage; // Only for custom string errors (allocates)

        #endregion

        #region Properties

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
                    throw new InvalidOperationException($"Cannot access Value in failure state. Error: {ErrorMessage}");
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

        /// <summary>Get error message (allocates string in Editor/Debug, returns enum int in Release)</summary>
        public string ErrorMessage
        {
            get
            {
                if (_isSuccess)
                    return null;

                // If custom message exists, use it
                if (_customErrorMessage != null)
                    return _customErrorMessage;

#if UNITY_EDITOR || UNIFP_DEBUG
                // Editor/Debug: Full display string
                return _errorCode.ToDisplayString();
#else
                // Release: Just the enum name (still allocates, but minimal)
                return _errorCode.ToString();
#endif
            }
        }

        /// <summary>
        /// LEGACY: Get error message as string (allocates)
        /// Kept for backward compatibility
        /// Use ErrorCode property for Zero GC
        /// </summary>
        [Obsolete("Use ErrorCode property for Zero GC. ErrorMessage allocates a string.")]
        public string Error => ErrorMessage;

        #endregion

        #region Construction

        private Result(bool isSuccess, T value, ErrorCode errorCode, string customErrorMessage)
        {
            _isSuccess = isSuccess;
            _value = value;
            _errorCode = errorCode;
            _customErrorMessage = customErrorMessage;
        }

        /// <summary>Create success result (Zero GC)</summary>
        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, ErrorCode.None, null);
        }

        /// <summary>Create failure result with error code (Zero GC)</summary>
        public static Result<T> Failure(ErrorCode errorCode)
        {
            return new Result<T>(false, default, errorCode, null);
        }

        /// <summary>
        /// Create failure result with custom error message (allocates string)
        /// Use ErrorCode overload for Zero GC
        /// </summary>
        public static Result<T> Failure(string customErrorMessage)
        {
            if (string.IsNullOrWhiteSpace(customErrorMessage))
                throw new ArgumentNullException(nameof(customErrorMessage), "Error message cannot be null or empty.");
            
            return new Result<T>(false, default, ErrorCode.Unknown, customErrorMessage);
        }

        #endregion

        #region Monad Operations

        /// <summary>Transform value on success</summary>
        public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            return IsSuccess
                ? Result<TResult>.Success(mapper(_value))
                : Result<TResult>.Failure(_errorCode, _customErrorMessage);
        }

        /// <summary>Chain with Result-returning function</summary>
        public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
        {
            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            return IsSuccess
                ? binder(_value)
                : Result<TResult>.Failure(_errorCode, _customErrorMessage);
        }

        /// <summary>Match success or failure case (Zero GC with ErrorCode callback)</summary>
        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<ErrorCode, TResult> onFailure)
        {
            if (onSuccess == null)
                throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null)
                throw new ArgumentNullException(nameof(onFailure));

            return IsSuccess
                ? onSuccess(_value)
                : onFailure(_errorCode);
        }

        /// <summary>Match success or failure case (allocates string)</summary>
        [Obsolete("Use Match(Func<T, TResult>, Func<ErrorCode, TResult>) for Zero GC")]
        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<string, TResult> onFailure)
        {
            if (onSuccess == null)
                throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null)
                throw new ArgumentNullException(nameof(onFailure));

            return IsSuccess
                ? onSuccess(_value)
                : onFailure(ErrorMessage);
        }

        /// <summary>Match success or failure with side effects (Zero GC)</summary>
        public void Match(Action<T> onSuccess, Action<ErrorCode> onFailure)
        {
            if (onSuccess == null)
                throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null)
                throw new ArgumentNullException(nameof(onFailure));

            if (IsSuccess)
                onSuccess(_value);
            else
                onFailure(_errorCode);
        }

        /// <summary>Match success or failure with side effects (allocates string)</summary>
        [Obsolete("Use Match(Action<T>, Action<ErrorCode>) for Zero GC")]
        public void Match(Action<T> onSuccess, Action<string> onFailure)
        {
            if (onSuccess == null)
                throw new ArgumentNullException(nameof(onSuccess));
            if (onFailure == null)
                throw new ArgumentNullException(nameof(onFailure));

            if (IsSuccess)
                onSuccess(_value);
            else
                onFailure(ErrorMessage);
        }

        #endregion

        #region Internal Helpers

        /// <summary>Internal: Create failure with both ErrorCode and custom message</summary>
        internal static Result<T> Failure(ErrorCode errorCode, string customErrorMessage)
        {
            return new Result<T>(false, default, errorCode, customErrorMessage);
        }

        #endregion

        #region Utility Methods

        /// <summary>Get value or default (Zero GC)</summary>
        public T GetValueOrDefault(T defaultValue = default)
        {
            return IsSuccess ? _value : defaultValue;
        }

        /// <summary>Get value or execute fallback (Zero GC if fallback doesn't allocate)</summary>
        public T GetValueOrElse(Func<ErrorCode, T> fallback)
        {
            if (fallback == null)
                throw new ArgumentNullException(nameof(fallback));

            return IsSuccess ? _value : fallback(_errorCode);
        }

        /// <summary>Get value or execute fallback (allocates string)</summary>
        [Obsolete("Use GetValueOrElse(Func<ErrorCode, T>) for Zero GC")]
        public T GetValueOrElse(Func<string, T> fallback)
        {
            if (fallback == null)
                throw new ArgumentNullException(nameof(fallback));

            return IsSuccess ? _value : fallback(ErrorMessage);
        }

        /// <summary>Execute action on failure (Zero GC)</summary>
        public Result<T> OnFailure(Action<ErrorCode> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (IsFailure)
                action(_errorCode);

            return this;
        }

        /// <summary>Execute action on failure (allocates string)</summary>
        [Obsolete("Use OnFailure(Action<ErrorCode>) for Zero GC")]
        public Result<T> OnFailure(Action<string> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (IsFailure)
                action(ErrorMessage);

            return this;
        }

        /// <summary>Execute action on success (Zero GC)</summary>
        public Result<T> OnSuccess(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (IsSuccess)
                action(_value);

            return this;
        }

        #endregion

        #region Operators

        public static implicit operator bool(Result<T> result) => result.IsSuccess;

        #endregion

        #region ToString

        public override string ToString()
        {
            return IsSuccess
                ? $"Success({_value})"
                : _customErrorMessage != null
                    ? $"Failure({_customErrorMessage})"
                    : $"Failure({_errorCode})";
        }

        #endregion
    }

    /// <summary>
    /// Factory methods for creating Result instances
    /// </summary>
    public static class Result
    {
        /// <summary>Create a Result from a value (Zero GC)</summary>
        public static Result<T> FromValue<T>(T value)
        {
            return Result<T>.Success(value);
        }

        /// <summary>Create a Result from an ErrorCode (Zero GC)</summary>
        public static Result<T> FromError<T>(ErrorCode errorCode)
        {
            return Result<T>.Failure(errorCode);
        }

        /// <summary>Create a Result from an error message (allocates string)</summary>
        [Obsolete("Use FromError(ErrorCode) for Zero GC")]
        public static Result<T> FromError<T>(string error)
        {
            return Result<T>.Failure(error);
        }

        /// <summary>Try to get a value from an operation, converting exceptions to Result</summary>
        public static Result<T> TryFromValue<T>(
            Func<T> func,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return Internal.SafeExecutor.Execute(
                () => Result<T>.Success(func()),
                OperationType.TryFromValue,
                memberName,
                filePath,
                lineNumber);
        }

        /// <summary>Try to get a Result from an operation, catching exceptions</summary>
        public static Result<T> TryFromResult<T>(
            Func<Result<T>> func,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return Internal.SafeExecutor.Execute(
                func,
                OperationType.TryFromResult,
                memberName,
                filePath,
                lineNumber);
        }
    }
}
