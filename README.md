![C# Functional Programming for Unity Capsule Header](https://capsule-render.vercel.app/api?type=waving&height=220&color=0:5A2BFF,100:1FB5E9&text=C%23%20Functional%20Programming%20for%20Unity&fontAlign=50&fontAlignY=40&fontSize=46&fontColor=FFFFFF&desc=UniFP&descAlign=50&descAlignY=65&descSize=24)

[English](./README.md) · [한국어](./README.ko.md) · [简体中文](./README.zh-CN.md)

# UniFP — C# Functional Programming for Unity

[![Unity](https://img.shields.io/badge/Unity-2020.3%2B-000?logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-success.svg)](./LICENSE)
[![Release](https://img.shields.io/badge/version-v1.0.0-blue)](https://github.com/Nekoya-Jin/UniFP/releases)

UniFP is a GC zero-allocation C# functional programming framework for Unity, inspired by Rust, Haskell, and F#. It brings functional thinking and explicit error handling to game logic without hurting runtime performance.

Traditional C# functional libraries (for example, [language-ext](https://github.com/louthy/language-ext)) target general-purpose .NET environments. They ship extensive features and complex abstractions, with steep learning curves and often avoid using structs, leading to GC allocations and performance loss in Unity's runtime.

UniFP was developed as a lightweight alternative optimized for real-time applications, combining Rust's type system-based stability and performance-first philosophy with the railway-oriented programming paradigm from functional languages. The goal is to enable safe error handling and declarative pipelines in gameplay code without heavy dependencies.

`Result<T>` and `Option<T>` provide pipeline extensions that implement type-safe flow control instead of exceptions while minimizing GC burden.

> Every core type is provided as a struct. In the Editor or `UNIFP_DEBUG` environment, each operation automatically records file/line/method information. It can be used directly in Unity projects without additional configuration.

**What UniFP is NOT**

❌ Rewriting all Unity scripts in functional style 🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️

✅ Simplifying complex branching and error handling in existing logic with functional pipelines 🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Core Highlights](#core-highlights)
- [Getting Started](#getting-started)
  - [UPM Installation (Recommended)](#upm-installation-recommended)
  - [Manual Installation](#manual-installation)
- [Core Concepts](#core-concepts)
  - [`Result<T>` — Escape the if/else and try/catch Hell 🔥🔥🔥](#resultt--escape-the-ifelse-and-trycatch-hell-)
  - [`Option<T>` — Escape the Null Hell 🔥🔥🔥](#optiont--escape-the-null-hell-)
  - [Error Codes and Diagnostics](#error-codes-and-diagnostics)
  - [`NonEmpty<T>` — Collections Guaranteed to Have at Least One Item](#nonemptyt--collections-guaranteed-to-have-at-least-one-item)
- [Fluent Pipelines](#fluent-pipelines)
  - [Branching Control and Recovery](#branching-control-and-recovery)
  - [Combining Multiple Results](#combining-multiple-results)
  - [Collections & Traversal](#collections--traversal)
- [Async & UniTask Integration](#async--unitask-integration)
- [Resilience Utilities](#resilience-utilities)
- [Debugging & Observability](#debugging--observability)
- [Performance Toolkit](#performance-toolkit)
- [Sample Scenes & Tests](#sample-scenes--tests)
- [Documentation](#documentation)
- [Contributing](#contributing)
- [License](#license)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Core Highlights

- **`Result<T>` and `Option<T>` structs** implement explicit success/failure and null safety without heap allocation.
- **Railway-style extension methods** (`Then`, `Map`, `Filter`, `Recover`, `DoStrict`, `IfFailed`, etc.) provide highly readable pipelines.
- **UniTask-based async pipelines** with `.ThenAsync`, `.MapAsync`, `.FilterAsync`, and `AsyncResult.TryAsync()` utilities.
- **ResultCombinators and collection extensions** for combining multiple results or traversing lists/Span with conditional validation.
- **SafeExecutor instrumentation** automatically records operation types and call locations in Editor/debug environments.
- **Performance-focused utilities** like DelegateCache, ResultPool, and SpanExtensions suppress GC even in high-frequency code.
- **`Assets/Scenes` demos and `src/UniFP/Assets/Tests` unit tests** showcase real usage patterns you can verify immediately.

## Getting Started

### UPM Installation (Recommended)

1. Open **Window ▸ Package Manager** in Unity.
2. Select **Add package from git URL...** and enter the address below.

   ```text
   https://github.com/Nekoya-Jin/UniFP.git?path=src/UniFP/Assets/Plugins/UniFP
   ```

3. Unity will install the `com.unifp.core` package and add folders with examples and asmdef.

To modify `Packages/manifest.json` directly, add the following dependency:

```json
{
  "dependencies": {
    "com.unifp.core": "https://github.com/Nekoya-Jin/UniFP.git?path=src/UniFP/Assets/Plugins/UniFP"
  }
}
```

### Manual Installation

Copy the `src/UniFP/Assets/Plugins/UniFP` directory to your project under `Assets/Plugins/UniFP`. Include `UniFP.asmdef` to keep Unity build times fast.

## Core Concepts

### `Result<T>` — Escape the if/else and try/catch Hell 🔥🔥🔥

Have you ever encountered code like this? An if containing a try, which contains another if-else... Success logic, failure logic, exception handling, and default value assignments are tangled like spaghetti, making it hard to know where to start reading. The moment you add one more validation step, the hell gets deeper and eventually becomes code no one wants to touch.

#### Traditional C# Approach

```csharp
using UnityEngine;

public class LoginSample : MonoBehaviour
{
    void Start()
    {
        var userId = PlayerPrefs.GetString("userId");

        if (string.IsNullOrWhiteSpace(userId))
        {
            Debug.LogError("Login failed: input is empty");
            userId = "guest";
        }
        else
        {
            try
            {
                if (!ValidateAccount(userId))
                {
                    Debug.LogWarning("Login failed: user not found");
                    userId = "guest";
                }
                else
                {
                    Debug.Log($"Login succeeded: {userId}");
                    LogUser(userId);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Exception during login: {ex.Message}");
                userId = "guest";
            }
        }
    }

    bool ValidateAccount(string id) => id == "player42";

    void LogUser(string id) => Debug.Log($"Auth pipeline accepted {id}");
}
```

- State checks, exception handling, and default value recovery logic are scattered across if/else and try/catch, making the branching complex.
- As failure cases increase, the number of branches grows exponentially, making maintenance difficult.

#### Refactoring with UniFP `Result<T>`

Now let's solve this problem with UniFP. UniFP places all branches and exception handling on a single conveyor belt, showing explicit success/failure paths. Data flows straight along the success highway, and if a problem arises anywhere, it immediately exits to the failure lane. Code reads from top to bottom like flowing water, and each step clearly shows what it does.

```csharp
// Good example: All steps are clearly chained.
using UniFP;
using UnityEngine;

public class LoginSample : MonoBehaviour
{
    void Start()
    {
        var loginResult = Result.FromValue(PlayerPrefs.GetString("userId"))
            // 1. Is the input valid? (If not, jump to InvalidInput failure lane)
            .Filter(DelegateCache.IsNotNullOrWhitespace, ErrorCode.InvalidInput)
            // 2. Does the account exist? (If not, jump to NotFound failure lane)
            .Then(id => ValidateAccount(id)
                ? Result<string>.Success(id)
                : Result<string>.Failure(ErrorCode.NotFound))
            // 3. (Only while on the success highway) Log the user
            .Do(LogUser)
            // 🚨 If we exited to the failure lane, the final destination is "guest"
            .Recover(_ => "guest");

        // Final processing based on result
        loginResult.Match(
            onSuccess: id => Debug.Log($"Login succeeded: {id}"),
            onFailure: code => Debug.LogError($"Login failed: {code}"));
    }

    bool ValidateAccount(string id) => id == "player42";
    void LogUser(string id) => Debug.Log($"Auth pipeline accepted {id}");
}
```

- Each step is explicitly chained, making the success/failure flow obvious at a glance.
- On failure, the pipeline automatically routes to `Recover`, cleanly separating exception and default value recovery logic.
- Additional validations or async calls can be easily extended with `Then`, `Filter`, `ThenAsync`, etc.

### `Option<T>` — Escape the Null Hell 🔥🔥🔥

In Unity projects, you sometimes encounter code with dozens of lines of `null` checks. `if (foo == null)` → `else if (foo.Bar == null)` → `else if (foo.Bar.Length == 0)` ... Error logs pop up everywhere, and you waste time tracking down which branch threw a `NullReferenceException`.

#### Traditional C# Approach

```csharp
public class UserProfileLoader
{
    public void Load()
    {
        var raw = PlayerPrefs.GetString("profile");

        if (string.IsNullOrEmpty(raw))
        {
            Debug.LogWarning("No profile: applying defaults");
            ApplyDefaults();
            return;
        }

        var profile = JsonUtility.FromJson<UserProfile>(raw);
        if (profile == null || profile.Items == null || profile.Items.Length == 0)
        {
            Debug.LogError("Profile corrupted: attempting recovery");
            ApplyDefaults();
            return;
        }

        Debug.Log($"Profile loaded: {profile.Name}");
        Apply(profile);
    }
}
```

- Null-guarding logic is scattered everywhere, obscuring the core flow.
- As additional conditions are added, `if` blocks multiply, and missing one step immediately causes exceptions.
- Indentation from `if` statements makes it hard to see at a glance which is the core logic flow and which is null handling.

#### Refactoring with UniFP `Option<T>`

`Option<T>` represents values as `Some` when present or `None` when absent. When `None`, subsequent pipeline steps are automatically skipped, naturally organizing null checks.

```csharp
using UniFP;

public class UserProfileLoader
{
    public void Load()
    {
        var profileOption = Option<string>.From(PlayerPrefs.GetString("profile"))
            // 1. If the string is empty, treat it as None
            .Filter(DelegateCache.IsNotNullOrWhitespace)
            // 2. Parse JSON and elevate the result to Option
            .Map(raw => JsonUtility.FromJson<UserProfile>(raw))
            .Filter(result => result is { Items: { Length: > 0 } });

        profileOption.Match(
            onSome: Apply,
            onNone: ApplyDefaults);
    }
}
```

- Following the pipeline makes it easy to spot which step failed.
- Adding validation logic is as simple as inserting another `Filter` step.
- One final `Match` clearly separates normal and fallback flows.

### Error Codes and Diagnostics

UniFP uses `ErrorCode` enums to categorize failures without strings. In the Editor or debug builds, `SafeExecutor` automatically records operation types and call-site details.

```csharp
public enum ErrorCode
{
    None,
    InvalidInput,
    NotFound,
    NetworkError,
    Timeout
}
```

### `NonEmpty<T>` — Collections Guaranteed to Have at Least One Item

Use when you need collections that cannot be empty. Suitable for domains like party composition and required slots.

```csharp
var squad = NonEmpty.Create("Leader", "Support", "Tank");
var upper = squad.Map(role => role.ToUpperInvariant());
```

## Fluent Pipelines

Import the `UniFP` namespace to utilize all extension methods. The railway pattern cleanly separates success and failure paths.

```csharp
var pipeline = Result.FromValue(request)
    .Filter(req => req.IsValid, ErrorCode.ValidationFailed)
    .Then(Persist)
    .DoStrict(SendAnalyticsEvent)
    .IfFailed(() => LoadCachedResult())
    .Trace("Purchase");
```

### Branching Control and Recovery

- `Recover(Func<ErrorCode, T>)` replaces failures with default values.
- `IfFailed(Func<Result<T>>)` executes an alternative pipeline.
- `ThenIf` and `MapIf` conditionally perform additional work.
- `DoStrict` is suitable for side effects that need to propagate failures (e.g., analytics events, database logging).

### Combining Multiple Results

`ResultCombinators` can bundle independent operations into a single result.

```csharp
var stats = ResultCombinators.Combine(
    LoadLevelProgress(),
    LoadInventory());

var snapshot = stats.Zip(
    CalculateRewards(),
    (progress, inventory, rewards) => new PlayerSnapshot(progress, inventory, rewards));
```

### Collections & Traversal

- `SelectResults` traverses collections and halts immediately on failure.
- `CombineAll` gathers multiple `Result<T>` into `Result<List<T>>`.
- `FilterResults`, `Partition`, `Fold`, `AggregateResults`, etc. perform list validation and aggregation.
- `SpanExtensions` operates on `Span<T>` without additional allocation, even in Burst-sensitive code.

## Async & UniTask Integration

UniTask and Result pipelines combine naturally.

```csharp
async UniTask<Result<PlayerData>> FetchPlayer(int id)
{
    return await Result.TryFromResult(() => ValidateId(id))
        .ThenAsync(async _ => await Api.GetPlayer(id))
        .MapAsync(payload => payload.ToPlayerData())
        .FilterAsync(data => UniTask.FromResult(data.IsActive), "Player is not active");
}

var cached = await FetchPlayer(42).DoAsync(data => Cache.Save(data));
```

Async operations that throw exceptions can be wrapped with `AsyncResult.TryAsync` to automatically convert them to `Result` failures.

## Resilience Utilities

- `Retry` and `RetryAsync` perform retries up to the specified number of attempts.
- `RetryWithBackoff` applies exponential backoff delays for handling unstable services.
- `Repeat` and `RepeatAsync` handle scenarios requiring N consecutive successes.
- `Catch` intercepts specific failure messages and executes alternative logic.

```csharp
var response = await RetryExtensions.RetryWithBackoff(
    () => Api.SendAsync(payload),
    maxAttempts: 5,
    initialDelayMilliseconds: 200,
    backoffMultiplier: 2.5f);
```

## Debugging & Observability

- `SafeExecutor` records the location and type of each operation.
- `PipelineDebug.Trace`, `TraceWith`, `TraceOnFailure`, and `Breakpoint` track pipeline state in the console.
- The `OperationType` enum immediately shows which step (`Map`, `Filter`, `Then`, etc.) failed.

```csharp
var result = LoadConfig()
    .Trace("Config")
    .Assert(cfg => cfg.Version >= 2, "Config version too old")
    .Breakpoint();
```

## Performance Toolkit

While basic usage has no major issues, if you're working with logic that runs every frame (like in Update methods), the following optimizations are necessary:

- **DelegateCache**: Reuses frequently-used lambdas with a static cache.
- **ResultPool & ListPool<T>**: Pools result collections to eliminate GC in high-frequency loops.
- **SpanExtensions**: Provides stack or pooled buffer-based transformations.
- **Zero-allocation error flow**: `ErrorCode`, `OperationType`, and struct monads suppress heap usage.

These utilities run in sample scenes like inventory handling, combat resolution, and shop purchases, operating stably without GC allocation.

## Sample Scenes & Tests

- `Assets/Scenes`
  - `01_BasicResultExample` — Result fundamentals
  - `02_PipelineExample` — Chaining patterns
  - `04_AsyncExample` — UniTask integration and async flow
  - `06_PerformanceExample` — Zero-allocation techniques
  - `10_RealWorld_UserLogin` — Robust login pipeline
  - `11_RealWorld_ItemPurchase` — Railway handling across services
- Tests are located in `src/UniFP/Assets/Tests`, covering validation, async failures, retry scenarios, and other major edge cases.

To run tests from the repository root, use the following command:

```bash
dotnet test src/UniFP/UniFP.Tests.csproj
```

## Documentation

Extended guides are available in the [`docs`](./docs) folder:

- [Getting Started](./docs/getting-started.md)
- [API Reference](./docs/api-reference.md)
- [Examples](./docs/examples.md)
- [Best Practices](./docs/best-practices.md)

## Contributing

Issues and pull requests are always welcome. Please write tests and examples before submitting changes.

## License

UniFP is distributed under the [MIT License](./LICENSE).
