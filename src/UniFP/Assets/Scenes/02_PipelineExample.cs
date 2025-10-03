using UnityEngine;
using UniFP;

namespace UniFP.Examples
{
    /// <summary>
    /// Pipeline chaining example.
    /// Builds pipelines using Then, Map, Filter, Do, and related operators.
    /// </summary>
    public class PipelineExample : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("=== Pipeline Example ===");

            // Step 1: Then (Bind) - chain Result-returning functions
            ThenExample();

            // Step 2: Map - transform values
            MapExample();

            // Step 3: Filter - validate conditions
            FilterExample();

            // Step 4: Do - perform side effects
            DoExample();

            // Step 5: assemble a composite pipeline
            ComplexPipelineExample();
        }

        #region Then Example

        void ThenExample()
        {
            Debug.Log("\n--- Then Example ---");

            var result = Result.FromValue("100")
                .Then(ParseInt)
                .Then(ValidatePositive)
                .Then(Double);

            result.Match(
                onSuccess: value => Debug.Log($"✓ Result: {value}"),
                onFailure: (ErrorCode error) => Debug.LogWarning($"✗ Failed: {error}")
            );
        }

        Result<int> ParseInt(string s)
        {
            if (int.TryParse(s, out var value))
                return Result<int>.Success(value);
            return Result<int>.Failure(ErrorCode.InvalidInput);
        }

        Result<int> ValidatePositive(int value)
        {
            if (value > 0)
                return Result<int>.Success(value);
            return Result<int>.Failure(ErrorCode.ValidationFailed);
        }

        Result<int> Double(int value)
        {
            return Result<int>.Success(value * 2);
        }

        #endregion

        #region Map Example

        void MapExample()
        {
            Debug.Log("\n--- Map Example ---");

            var result = Result.FromValue(10)
                .Map(x => x * 2)
                .Map(x => x + 5)
                .Map(x => $"Result: {x}");

            Debug.Log($"✓ {result.Value}");
        }

        #endregion

        #region Filter Example

        void FilterExample()
        {
            Debug.Log("\n--- Filter Example ---");

            var result = Result.FromValue(15)
                .Filter(x => x > 0, ErrorCode.ValidationFailed)
                .Filter(x => x < 100, ErrorCode.ValidationFailed)
                .Filter(x => x % 5 == 0, ErrorCode.ValidationFailed);

            if (result.IsSuccess)
            {
                Debug.Log($"✓ Valid value: {result.Value}");
            }
        }

        #endregion

        #region Do Example

        void DoExample()
        {
            Debug.Log("\n--- Do Example ---");

            var result = Result.FromValue(10)
                .Do(x => Debug.Log($"Step 1: {x}"))
                .Map(x => x * 2)
                .Do(x => Debug.Log($"Step 2: {x}"))
                .Map(x => x + 5)
                .Do(x => Debug.Log($"Step 3: {x}"));

            Debug.Log($"✓ Final: {result.Value}");
        }

        #endregion

        #region Complex Pipeline

        void ComplexPipelineExample()
        {
            Debug.Log("\n--- Complex Pipeline Example ---");

            var userInput = "42";

            var result = Result.FromValue(userInput)
                .Do(x => Debug.Log($"Input value: {x}"))
                .Then(ParseInt)
                .Do(x => Debug.Log($"Parsed value: {x}"))
                .Filter(x => x > 0, ErrorCode.ValidationFailed)
                .Do(x => Debug.Log($"Positive validation passed"))
                .Map(x => x * 2)
                .Do(x => Debug.Log($"Doubled value: {x}"))
                .Filter(x => x < 1000, ErrorCode.ValidationFailed)
                .Do(x => Debug.Log($"Range check passed"));

            result.Match(
                onSuccess: value => Debug.Log($"✓ Final result: {value}"),
                onFailure: (ErrorCode error) => Debug.LogWarning($"✗ Failed: {error}")
            );
        }

        #endregion
    }
}
