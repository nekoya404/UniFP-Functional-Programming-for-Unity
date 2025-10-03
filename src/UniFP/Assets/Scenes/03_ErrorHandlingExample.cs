using UnityEngine;
using UniFP;

namespace UniFP.Examples
{
    /// <summary>
    /// Error handling example.
    /// Demonstrates Railway-Oriented Programming with Recover, IfFailed, Match, and related operators.
    /// </summary>
    public class ErrorHandlingExample : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("=== Error Handling Example ===");

            // Step 1: Recover - restore with a default value
            RecoverExample();

            // Step 2: IfFailed - provide an alternative
            IfFailedExample();

            // Step 3: Match - perform pattern matching
            MatchExample();

            // Step 4: Catch - handle targeted errors
            CatchExample();

            // Step 5: Railway pattern flow
            RailwayExample();
        }

        #region Recover Example

        void RecoverExample()
        {
            Debug.Log("\n--- Recover Example ---");

            // ErrorCode variant (zero GC)
            var result1 = Result.FromError<int>(ErrorCode.NotFound)
                .Recover((ErrorCode error) =>
                {
                    Debug.LogWarning($"Error occurred: {error}, using default");
                    return 0;
                });

            Debug.Log($"✓ Recovered value: {result1}");

            // String variant (helpful for debugging)
            var result2 = Result<int>.Failure("File not found")
                .Recover(error =>
                {
                    Debug.LogWarning($"Error: {error.ToDisplayString()}");
                    return -1;
                });

            Debug.Log($"✓ Recovered value: {result2}");
        }

        #endregion

        #region IfFailed Example

        void IfFailedExample()
        {
            Debug.Log("\n--- IfFailed Example ---");

            var primary = LoadConfigFromFile();
            var fallback = LoadDefaultConfig();

            var config = primary.IfFailed(fallback);

            Debug.Log($"✓ Config loaded: {config.Value.version}");
        }

        Result<Config> LoadConfigFromFile()
        {
            // Simulate a file load failure
            return Result<Config>.Failure(ErrorCode.NotFound);
        }

        Result<Config> LoadDefaultConfig()
        {
            return Result<Config>.Success(new Config { version = "1.0.0" });
        }

        class Config
        {
            public string version;
        }

        #endregion

        #region Match Example

        void MatchExample()
        {
            Debug.Log("\n--- Match Example ---");

            var result = DivideNumbers(10, 0);

            // ErrorCode variant
            var message = result.Match(
                onSuccess: value => $"Result: {value}",
                onFailure: (ErrorCode error) => $"Error: {error}"
            );

            Debug.Log(message);

            // Void variant (side effects only)
            result.Match(
                onSuccess: value => Debug.Log($"✓ Success: {value}"),
                onFailure: (ErrorCode error) => Debug.LogError($"✗ Failed: {error}")
            );
        }

        Result<int> DivideNumbers(int a, int b)
        {
            if (b == 0)
                return Result<int>.Failure(ErrorCode.InvalidInput);
            return Result<int>.Success(a / b);
        }

        #endregion

        #region Catch Example

        void CatchExample()
        {
            Debug.Log("\n--- Catch Example ---");

            var result = Result<int>.Failure("Network timeout")
                .Catch(
                    errorPredicate: msg => msg.Contains("timeout"),
                    handler: msg =>
                    {
                        Debug.LogWarning($"Retrying after timeout: {msg}");
                        return Result<int>.Success(42); // Retry succeeded
                    }
                );

            Debug.Log($"✓ Result after catch: {result.Value}");
        }

        #endregion

        #region Railway Pattern

        void RailwayExample()
        {
            Debug.Log("\n--- Railway Pattern Example ---");

            ProcessUser("john@example.com", "25");
        }

        void ProcessUser(string email, string ageStr)
        {
            var result = ValidateEmail(email)
                .Then(validEmail => ParseAge(ageStr)
                    .Map(age => new User { Email = validEmail, Age = age }))
                .Then(SaveUser)
                .Then(SendWelcomeEmail);

            result.Match(
                onSuccess: user => Debug.Log($"✓ User processed: {user.Email}"),
                onFailure: (ErrorCode error) => Debug.LogError($"✗ Processing failed: {error}")
            );
        }

        Result<string> ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return Result<string>.Failure(ErrorCode.InvalidInput);
            if (!email.Contains("@"))
                return Result<string>.Failure(ErrorCode.ValidationFailed);
            return Result<string>.Success(email);
        }

        Result<int> ParseAge(string ageStr)
        {
            if (!int.TryParse(ageStr, out var age))
                return Result<int>.Failure(ErrorCode.InvalidInput);
            if (age < 0 || age > 150)
                return Result<int>.Failure(ErrorCode.ValidationFailed);
            return Result<int>.Success(age);
        }

        Result<User> SaveUser(User user)
        {
            Debug.Log($"Saving user: {user.Email}");
            // Persist to database (placeholder)
            return Result<User>.Success(user);
        }

        Result<User> SendWelcomeEmail(User user)
        {
            Debug.Log($"Sending welcome email to: {user.Email}");
            // Send email (placeholder)
            return Result<User>.Success(user);
        }

        class User
        {
            public string Email;
            public int Age;
        }

        #endregion
    }
}
