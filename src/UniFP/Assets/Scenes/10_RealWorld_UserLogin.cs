using UnityEngine;
using UniFP;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System;

namespace UniFP.Examples.RealWorld
{
    /// <summary>
    /// Real-world example: user login and data-loading system.
    /// Combines API calls, validation, caching, and error handling in a single flow.
    /// </summary>
    public class UserLoginExample : MonoBehaviour
    {
        [SerializeField] private string _testEmail = "user@example.com";
        [SerializeField] private string _testPassword = "password123";

        private Dictionary<string, UserData> _userCache = new Dictionary<string, UserData>();
        private CancellationTokenSource _cts;

        async void Start()
        {
            _cts = new CancellationTokenSource();

            Debug.Log("=== Real World Example: User Login System ===\n");

            // Execute the complete login flow
            await LoginAndLoadUserData(_testEmail, _testPassword);
        }

        void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        #region Main Flow

        /// <summary>
        /// Complete login flow.
        /// 1. Validate input
        /// 2. Authenticate
        /// 3. Load user data (cache-first)
        /// 4. Load profile
        /// 5. Display welcome message
        /// </summary>
        async UniTask LoginAndLoadUserData(string email, string password)
        {
            var result = await ValidateCredentials(email, password)
                .ThenAsync(creds => AuthenticateUser(creds, _cts.Token))
                .ThenAsync(token => LoadUserData(token, _cts.Token))
                .ThenAsync(user => LoadUserProfile(user, _cts.Token))
                .ThenAsync(profile => ShowWelcomeMessage(profile, _cts.Token))
                .DoAsync(async profile =>
                {
                    Debug.Log($"\n✓ Login complete for {profile.User.Email}");
                    await CacheUserData(profile.User);
                });

            result.Match(
                onSuccess: profile =>
                {
                    Debug.Log($"\n🎉 Successfully logged in!");
                    Debug.Log($"User: {profile.User.Name} ({profile.User.Email})");
                    Debug.Log($"Level: {profile.Level}, XP: {profile.Experience}");
                },
                onFailure: (ErrorCode error) =>
                {
                    Debug.LogError($"\n❌ Login failed: {error}");
                    HandleLoginError(error);
                }
            );
        }

        #endregion

        #region 1. Validation

        /// <summary>
        /// Validate email and password input.
        /// </summary>
        Result<Credentials> ValidateCredentials(string email, string password)
        {
            Debug.Log("1️⃣ Validating credentials...");

            return Result.FromValue(email)
                .Filter(e => !string.IsNullOrWhiteSpace(e), ErrorCode.InvalidInput)
                .Filter(e => e.Contains("@"), ErrorCode.ValidationFailed)
                .Filter(e => e.Length <= 100, ErrorCode.ValidationFailed)
                .Then(validEmail => ValidatePassword(password)
                    .Map(validPwd => new Credentials
                    {
                        Email = validEmail,
                        Password = validPwd
                    }));
        }

        Result<string> ValidatePassword(string password)
        {
            return Result.FromValue(password)
                .Filter(p => !string.IsNullOrWhiteSpace(p), ErrorCode.InvalidInput)
                .Filter(p => p.Length >= 8, ErrorCode.ValidationFailed)
                .Filter(p => p.Length <= 50, ErrorCode.ValidationFailed);
        }

        #endregion

        #region 2. Authentication

        /// <summary>
        /// Authenticate the user (simulated API call).
        /// </summary>
        async UniTask<Result<AuthToken>> AuthenticateUser(
            Credentials credentials,
            CancellationToken ct)
        {
            Debug.Log($"2️⃣ Authenticating {credentials.Email}...");

            return await AsyncResult.TryAsync(async () =>
            {
                // Simulate API call latency
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: ct);

                // In production this would call the server API
                if (credentials.Email == _testEmail && credentials.Password == _testPassword)
                {
                    return new AuthToken
                    {
                        Token = "jwt_token_" + Guid.NewGuid().ToString(),
                        UserId = 1
                    };
                }

                throw new UnauthorizedAccessException("Invalid credentials");
            });
        }

        #endregion

        #region 3. Load User Data

        /// <summary>
        /// Load user data using a cache-first strategy.
        /// </summary>
        async UniTask<Result<UserData>> LoadUserData(AuthToken token, CancellationToken ct)
        {
            Debug.Log($"3️⃣ Loading user data for ID {token.UserId}...");

            // First attempt: load from cache
            var cachedResult = LoadFromCache(token.UserId);

            // Second attempt: load from the API if cache misses
            if (cachedResult.IsSuccess)
                return cachedResult;

            return await LoadFromApi(token.UserId, ct);
        }

        Result<UserData> LoadFromCache(int userId)
        {
            if (_userCache.TryGetValue(userId.ToString(), out var userData))
            {
                Debug.Log("  ✓ Loaded from cache");
                return Result<UserData>.Success(userData);
            }

            Debug.Log("  ✗ Cache miss");
            return Result<UserData>.Failure(ErrorCode.NotFound);
        }

        async UniTask<Result<UserData>> LoadFromApi(int userId, CancellationToken ct)
        {
            Debug.Log("  📡 Loading from API...");

            return await AsyncResult.TryAsync(async () =>
            {
                await UniTask.Delay(300, cancellationToken: ct);

                return new UserData
                {
                    Id = userId,
                    Email = _testEmail,
                    Name = "John Doe",
                    CreatedAt = DateTime.Now.AddDays(-30)
                };
            });
        }

        #endregion

        #region 4. Load Profile

        /// <summary>
        /// Load the user profile.
        /// </summary>
        async UniTask<Result<UserProfile>> LoadUserProfile(
            UserData user,
            CancellationToken ct)
        {
            Debug.Log($"4️⃣ Loading profile for {user.Name}...");

            return await AsyncResult.TryAsync(async () =>
            {
                await UniTask.Delay(200, cancellationToken: ct);

                return new UserProfile
                {
                    User = user,
                    Level = 42,
                    Experience = 12500,
                    Achievements = new List<string> { "First Login", "Tutorial Complete" }
                };
            });
        }

        #endregion

        #region 5. Welcome Message

        /// <summary>
        /// Display the welcome message.
        /// </summary>
        async UniTask<Result<UserProfile>> ShowWelcomeMessage(
            UserProfile profile,
            CancellationToken ct)
        {
            Debug.Log($"5️⃣ Showing welcome message...");

            await UniTask.Delay(100, cancellationToken: ct);

            var daysSinceJoin = (DateTime.Now - profile.User.CreatedAt).Days;
            Debug.Log($"\n👋 Welcome back, {profile.User.Name}!");
            Debug.Log($"   You've been with us for {daysSinceJoin} days");
            Debug.Log($"   Achievements: {string.Join(", ", profile.Achievements)}");

            return Result<UserProfile>.Success(profile);
        }

        #endregion

        #region Caching & Error Handling

        async UniTask CacheUserData(UserData user)
        {
            await UniTask.Yield();
            _userCache[user.Id.ToString()] = user;
            Debug.Log($"  💾 Cached user data for {user.Email}");
        }

        void HandleLoginError(ErrorCode error)
        {
            if (error.Equals(ErrorCode.InvalidInput))
            {
                Debug.LogError("Please check your email and password");
            }
            else if (error.Equals(ErrorCode.ValidationFailed))
            {
                Debug.LogError("Email or password format is invalid");
            }
            else if (error.Equals(ErrorCode.NotFound))
            {
                Debug.LogError("User not found");
            }
            else if (error.Equals(ErrorCode.NetworkError))
            {
                Debug.LogError("Network connection failed. Please try again");
            }
            else if (error.Equals(ErrorCode.Timeout))
            {
                Debug.LogError("Request timed out. Please check your connection");
            }
            else
            {
                Debug.LogError($"An unexpected error occurred: {error}");
            }
        }

        #endregion

        #region Data Models

        [Serializable]
        class Credentials
        {
            public string Email;
            public string Password;
        }

        [Serializable]
        class AuthToken
        {
            public string Token;
            public int UserId;
        }

        [Serializable]
        class UserData
        {
            public int Id;
            public string Email;
            public string Name;
            public DateTime CreatedAt;
        }

        [Serializable]
        class UserProfile
        {
            public UserData User;
            public int Level;
            public int Experience;
            public List<string> Achievements;
        }

        #endregion
    }
}
