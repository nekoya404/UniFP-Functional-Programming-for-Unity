using UnityEngine;
using UniFP;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace UniFP.Examples
{
    /// <summary>
    /// Asynchronous workflow example.
    /// Demonstrates ThenAsync, MapAsync, and FilterAsync in combination with UniTask.
    /// </summary>
    public class AsyncExample : MonoBehaviour
    {
        async void Start()
        {
            Debug.Log("=== Async Example ===");

            // Step 1: ThenAsync - asynchronous bind
            await ThenAsyncExample();

            // Step 2: MapAsync - asynchronous map
            await MapAsyncExample();

            // Step 3: FilterAsync - asynchronous filter
            await FilterAsyncExample();

            // Step 4: DoAsync - asynchronous side effects
            await DoAsyncExample();

            // Step 5: composite asynchronous pipeline
            await ComplexAsyncExample();
        }

        #region ThenAsync Example

        async UniTask ThenAsyncExample()
        {
            Debug.Log("\n--- ThenAsync Example ---");

            var result = await Result.FromValue(1)
                .ThenAsync(FetchUserAsync)
                .ThenAsync(FetchPostsAsync);

            result.Match(
                onSuccess: posts => Debug.Log($"✓ Fetched {posts.Length} posts"),
                onFailure: (ErrorCode error) => Debug.LogWarning($"✗ Failed: {error}")
            );
        }

        async UniTask<Result<User>> FetchUserAsync(int userId)
        {
            Debug.Log($"Fetching user {userId}...");
            await UniTask.Delay(100); // Simulate an API call

            return Result<User>.Success(new User { Id = userId, Name = "John" });
        }

        async UniTask<Result<Post[]>> FetchPostsAsync(User user)
        {
            Debug.Log($"Fetching posts for {user.Name}...");
            await UniTask.Delay(100);

            var posts = new[]
            {
                new Post { Id = 1, Title = "First Post" },
                new Post { Id = 2, Title = "Second Post" }
            };

            return Result<Post[]>.Success(posts);
        }

        class User { public int Id; public string Name; }
        class Post { public int Id; public string Title; }

        #endregion

        #region MapAsync Example

        async UniTask MapAsyncExample()
        {
            Debug.Log("\n--- MapAsync Example ---");

            var result = await Result.FromValue(10)
                .MapAsync(DoubleAsync)
                .MapAsync(async x => await FormatAsync(x));

            Debug.Log($"✓ Result: {result.Value}");
        }

        async UniTask<int> DoubleAsync(int value)
        {
            await UniTask.Delay(50);
            return value * 2;
        }

        async UniTask<string> FormatAsync(int value)
        {
            await UniTask.Delay(50);
            return $"Value: {value}";
        }

        #endregion

        #region FilterAsync Example

        async UniTask FilterAsyncExample()
        {
            Debug.Log("\n--- FilterAsync Example ---");

            var result = await Result.FromValue(42)
                .FilterAsync(IsValidAsync, "Validation failed");

            result.Match(
                onSuccess: value => Debug.Log($"✓ Valid: {value}"),
                onFailure: (ErrorCode error) => Debug.LogWarning($"✗ {error.ToDisplayString()}")
            );
        }

        async UniTask<bool> IsValidAsync(int value)
        {
            Debug.Log("Validating async...");
            await UniTask.Delay(100);
            return value > 0 && value < 100;
        }

        #endregion

        #region DoAsync Example

        async UniTask DoAsyncExample()
        {
            Debug.Log("\n--- DoAsync Example ---");

            var result = await Result.FromValue(5)
                .DoAsync(async x =>
                {
                    Debug.Log($"Step 1: {x}");
                    await UniTask.Delay(50);
                })
                .MapAsync(async x =>
                {
                    await UniTask.Delay(50);
                    return x * 2;
                })
                .DoAsync(async x =>
                {
                    Debug.Log($"Step 2: {x}");
                    await UniTask.Delay(50);
                });

            Debug.Log($"✓ Final: {result.Value}");
        }

        #endregion

        #region Complex Async Example

        async UniTask ComplexAsyncExample()
        {
            Debug.Log("\n--- Complex Async Pipeline ---");

            var userId = 1;
            var cancellationToken = this.GetCancellationTokenOnDestroy();

            var result = await FetchAndProcessUserData(userId, cancellationToken);

            result.Match(
                onSuccess: data => Debug.Log($"✓ Processed: {data}"),
                onFailure: (ErrorCode error) => Debug.LogError($"✗ Failed: {error}")
            );
        }

        async UniTask<Result<string>> FetchAndProcessUserData(int userId, CancellationToken ct)
        {
            return await Result.FromValue(userId)
                .ThenAsync(async id =>
                {
                    Debug.Log($"1. Fetching user {id}...");
                    await UniTask.Delay(100, cancellationToken: ct);
                    return Result<UserData>.Success(new UserData { Id = id, Name = "Alice" });
                })
                .ThenAsync(async user =>
                {
                    Debug.Log($"2. Fetching profile for {user.Name}...");
                    await UniTask.Delay(100, cancellationToken: ct);
                    return Result<Profile>.Success(new Profile { Bio = "Developer" });
                })
                .ThenAsync(async profile =>
                {
                    Debug.Log($"3. Processing profile...");
                    await UniTask.Delay(100, cancellationToken: ct);
                    return Result<string>.Success($"Profile: {profile.Bio}");
                });
        }

        class UserData { public int Id; public string Name; }
        class Profile { public string Bio; }

        #endregion
    }
}
