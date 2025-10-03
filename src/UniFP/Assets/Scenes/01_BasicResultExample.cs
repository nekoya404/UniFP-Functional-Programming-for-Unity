using UnityEngine;
using UniFP;

namespace UniFP.Examples
{
    /// <summary>
    /// Basic usage example for Result.
    /// Demonstrates creating Success/Failure instances and the core usage patterns.
    /// </summary>
    public class BasicResultExample : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("=== Basic Result Example ===");

            // Step 1: create a Success result
            SuccessExample();

            // Step 2: create a Failure result
            FailureExample();

            // Step 3: create a result conditionally
            ConditionalExample();

            // Step 4: apply the try pattern
            TryExample();
        }

        #region Success Example

        void SuccessExample()
        {
            Debug.Log("\n--- Success Example ---");

            // Approach 1: use FromValue (recommended)
            var result1 = Result.FromValue(42);
            Debug.Log($"FromValue: {result1.Value}");

            // Approach 2: call Success directly
            var result2 = Result<int>.Success(100);
            Debug.Log($"Success: {result2.Value}");

            // Check the value
            if (result1.IsSuccess)
            {
                Debug.Log($"✓ Success! Value = {result1.Value}");
            }
        }

        #endregion

        #region Failure Example

        void FailureExample()
        {
            Debug.Log("\n--- Failure Example ---");

            // Approach 1: use ErrorCode (zero GC, recommended)
            var result1 = Result.FromError<int>(ErrorCode.InvalidInput);
            Debug.Log($"ErrorCode: {result1.ErrorCode}");

            // Approach 2: custom message (for debugging)
            var result2 = Result<int>.Failure("Something went wrong");
            Debug.Log($"Error Message: {result2.ErrorMessage}");

            // Check the error state
            if (result1.IsFailure)
            {
                Debug.LogWarning($"✗ Failed! ErrorCode = {result1.ErrorCode}");
            }
        }

        #endregion

        #region Conditional Example

        void ConditionalExample()
        {
            Debug.Log("\n--- Conditional Example ---");

            var age = 25;
            var result = ValidateAge(age);

            result.Match(
                onSuccess: validAge => Debug.Log($"✓ Valid age: {validAge}"),
                onFailure: (ErrorCode error) => Debug.LogWarning($"✗ Invalid: {error}")
            );
        }

        Result<int> ValidateAge(int age)
        {
            if (age < 0)
                return Result<int>.Failure(ErrorCode.InvalidInput);
            if (age > 150)
                return Result<int>.Failure(ErrorCode.ValidationFailed);

            return Result<int>.Success(age);
        }

        #endregion

        #region Try Example

        void TryExample()
        {
            Debug.Log("\n--- Try Example ---");

            // Try - convert exceptions into a Result
            var result = Result.TryFromValue(() =>
            {
                var json = "{\"name\":\"John\",\"age\":30}";
                return JsonUtility.FromJson<User>(json);
            });

            if (result.IsSuccess)
            {
                Debug.Log($"✓ Parsed: {result.Value.name}, {result.Value.age}");
            }
            else
            {
                Debug.LogError($"✗ Parse failed: {result.ErrorMessage}");
            }
        }

        [System.Serializable]
        class User
        {
            public string name;
            public int age;
        }

        #endregion
    }
}
