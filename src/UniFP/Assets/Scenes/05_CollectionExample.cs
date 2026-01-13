using UnityEngine;
using UniFP;
using System.Collections.Generic;
using System.Linq;

namespace UniFP.Examples
{
    /// <summary>
    /// Collection processing example.
    /// Demonstrates SelectResults, CombineAll, Partition, and other collection operations.
    /// </summary>
    public class CollectionExample : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("=== Collection Example ===");

            // Step 1: SelectResults - convert each item to a Result
            SelectResultsExample();

            // Step 2: CombineAll - aggregate all results
            CombineAllExample();

            // Step 3: Partition - split successes and failures
            PartitionExample();

            // Step 4: Fold - accumulate with validation
            FoldExample();

            // Step 5: FirstSuccess - locate the first success
            FirstSuccessExample();
        }

        #region SelectResults Example

        void SelectResultsExample()
        {
            Debug.Log("\n--- SelectResults Example ---");

            var userIds = new List<int> { 1, 2, 3, 4, 5 };

            // Convert all IDs into User results
            var result = userIds.SelectResults(LoadUser);

            result.Match(
                onSuccess: users =>
                {
                    Debug.Log($"✓ Loaded {users.Count} users:");
                    foreach (var user in users)
                        Debug.Log($"  - {user.Name}");
                },
                onFailure: (ErrorCode error) => Debug.LogWarning($"✗ Failed: {error}")
            );
        }

        Result<User> LoadUser(int id)
        {
            // Assume ID 3 fails to load
            if (id == 3)
                return Result<User>.Failure(ErrorCode.NotFound);

            return Result<User>.Success(new User { Id = id, Name = $"User{id}" });
        }

        class User { public int Id; public string Name; }

        #endregion

        #region CombineAll Example

        void CombineAllExample()
        {
            Debug.Log("\n--- CombineAll Example ---");

            var results = new List<Result<int>>
            {
                Result<int>.Success(10),
                Result<int>.Success(20),
                Result<int>.Success(30)
            };

            var combined = results.CombineAll();

            combined.Match(
                onSuccess: values =>
                {
                    var sum = values.Sum();
                    Debug.Log($"✓ Combined {values.Count} values, sum = {sum}");
                },
                onFailure: (ErrorCode error) => Debug.LogWarning($"✗ Failed: {error}")
            );
        }

        #endregion

        #region Partition Example

        void PartitionExample()
        {
            Debug.Log("\n--- Partition Example ---");

            var results = new List<Result<int>>
            {
                Result<int>.Success(10),
                Result<int>.Failure("Error 1"),
                Result<int>.Success(20),
                Result<int>.Failure("Error 2"),
                Result<int>.Success(30)
            };

            var (successes, failures) = results.Partition();

            Debug.Log($"✓ Successes: {string.Join(", ", successes)}");
            Debug.Log($"✗ Failures: {string.Join(", ", failures)}");
        }

        #endregion

        #region Fold Example

        void FoldExample()
        {
            Debug.Log("\n--- Fold Example ---");

            var numbers = new List<int> { 1, 2, 3, 4, 5 };

            // Validate each number while accumulating
            var result = numbers.Fold(
                seed: 0,
                folder: (acc, num) =>
                {
                    if (num < 0)
                        return Result<int>.Failure(ErrorCode.InvalidInput);
                    return Result<int>.Success(acc + num);
                }
            );

            result.Match(
                onSuccess: sum => Debug.Log($"✓ Sum: {sum}"),
                onFailure: (ErrorCode error) => Debug.LogWarning($"✗ Failed: {error}")
            );
        }

        #endregion

        #region FirstSuccess Example

        void FirstSuccessExample()
        {
            Debug.Log("\n--- FirstSuccess Example ---");

            // Attempt to load data from multiple sources
            var sources = new System.Func<Result<string>>[]
            {
                () => LoadFromCache(),
                () => LoadFromFile(),
                () => LoadFromNetwork()
            };

            var triedResults = new List<Result<string>>(sources.Length);
            foreach (var source in sources)
            {
                var attempt = source();
                triedResults.Add(attempt);
                if (attempt.IsSuccess)
                    break;
            }

            var result = Result.FirstSuccess<string>(triedResults.ToArray());

            result.Match(
                onSuccess: data => Debug.Log($"✓ Loaded from first available source: {data}"),
                onFailure: (ErrorCode error) => Debug.LogError($"✗ All sources failed: {error}")
            );
        }
        Result<string> LoadFromCache()
        {
            Debug.Log("Trying cache...");
            return Result<string>.Failure(ErrorCode.NotFound);
        }

        Result<string> LoadFromFile()
        {
            Debug.Log("Trying file...");
            return Result<string>.Success("Data from file");
        }

        Result<string> LoadFromNetwork()
        {
            Debug.Log("Trying network...");
            return Result<string>.Success("Data from network");
        }

        #endregion
    }
}
