![C# Functional Programming for Unity Capsule Header](https://capsule-render.vercel.app/api?type=waving&height=220&color=0:5A2BFF,100:1FB5E9&text=C%23%20Functional%20Programming%20for%20Unity&fontAlign=50&fontAlignY=40&fontSize=46&fontColor=FFFFFF&desc=UniFP&descAlign=50&descAlignY=65&descSize=24)

[English](./README.md) · [한국어](./README.ko.md) · [简体中文](./README.zh-CN.md) · [日本語](./README.ja.md)

# UniFP — C# Functional Programming for Unity

[![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-000?logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-success.svg)](./LICENSE)
[![Version](https://img.shields.io/github/package-json/v/Nekoya-Jin/UniFP?filename=src%2FUniFP%2FAssets%2FPlugins%2FUniFP%2Fpackage.json&label=version&color=blue)](https://github.com/Nekoya-Jin/UniFP/releases)

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
- [Comparing with Other Libraries](#comparing-with-other-libraries)
- [Getting Started](#getting-started)
  - [UPM Installation (Recommended)](#upm-installation-recommended)
  - [Manual Installation](#manual-installation)
  - [Optional Dependencies](#optional-dependencies)
- [Core Concepts](#core-concepts)
  - [`Result<T>` Usage](#resultt-usage)
    - [Creating a Result](#creating-a-result)
    - [Core Methods: Then, Map, Filter](#core-methods-then-map-filter)
    - [Error Handling and Recovery](#error-handling-and-recovery)
    - [Side Effects](#side-effects)
    - [Conditional Execution](#conditional-execution)
    - [Async Result (UniTask / Awaitable)](#async-result-unitask--awaitable)
  - [`Option<T>` Usage](#optiont-usage)
    - [Creating an Option](#creating-an-option)
    - [Core Option Methods](#core-option-methods)
    - [Option and Result Conversion](#option-and-result-conversion)
    - [Branching with Match](#branching-with-match)
    - [Collection Helpers](#collection-helpers)
    - [LINQ Integration](#linq-integration)
  - [`NonEmpty<T>` Usage](#nonemptyt-usage)
    - [Creating a NonEmpty](#creating-a-nonempty)
    - [NonEmpty Methods](#nonempty-methods)
    - [Usage Examples](#usage-examples)
  - [Error Codes and Diagnostics](#error-codes-and-diagnostics)
    - [Built-in ErrorCode](#built-in-errorcode)
    - [Custom ErrorCode](#custom-errorcode)
    - [ErrorCode Properties](#errorcode-properties)
    - [Diagnostic Information (Debug Mode)](#diagnostic-information-debug-mode)
- [Fluent Pipelines](#fluent-pipelines)
  - [Branching Control and Recovery](#branching-control-and-recovery)
  - [Combining Multiple Results](#combining-multiple-results)
  - [Collections & Traversal](#collections--traversal)
- [Async Support (UniTask / Awaitable)](#async-support-unitask--awaitable)
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
- **Async support for both UniTask and Unity Awaitable** with `.ThenAsync`, `.MapAsync`, `.FilterAsync`, and `AsyncResult.TryAsync()` utilities.
- **ResultCombinators and collection extensions** for combining multiple results or traversing lists/Span with conditional validation.
- **SafeExecutor instrumentation** automatically records operation types and call locations in Editor/debug environments.
- **Performance-focused utilities** like DelegateCache, ResultPool, and SpanExtensions suppress GC even in high-frequency code.
- **`Assets/Scenes` demos and `src/UniFP/Assets/Tests` unit tests** showcase real usage patterns you can verify immediately.

## Comparing with Other Libraries

### UniFP vs Unity-NOPE

**Performance Comparison:**
- ✅ Zero-GC struct design (all core types stack-allocated)
- ✅ Delegate caching for Update loop optimization
- ✅ Built-in ResultPool & ListPool

**Feature Comparison:**
- UniFP's `Then` = NOPE's `Bind` (C#-friendly naming)
- Additional features: Retry, RetryWithBackoff, Trace, Breakpoint
- Advanced collection extensions: SelectResults, CombineAll, Partition

**Error Typing:**
UniFP uses `ErrorCode` struct, optimized for 99% of Unity game scenarios.
For complex domain logic requiring type-safe errors, custom ErrorCode patterns are available.

**➡️ Detailed Comparison: [Library Comparison Documentation](./docs/library-comparison.md)**

---

### UniFP vs language-ext

**Why Not Use language-ext Directly in Unity?**
- ❌ No Unity runtime optimization (class-based, GC pressure)
- ❌ Overwhelming feature complexity (100+ monads, Higher-kinded types)
- ❌ Steep learning curve (Haskell background required)
- ✅ UniFP: Unity-specific minimal set, start with C# LINQ experience only

**Quick Comparison:**

| Category | language-ext | UniFP |
|----------|--------------|-------|
| Core Monads | Option, Either, Try, Validation, Fin | Result, Option, NonEmpty |
| Performance | High abstraction overhead | Zero-GC structs, pooling optimization |
| Learning Curve | Steep (Haskell) | Gentle (C# LINQ) |
| Unity Integration | Limited | UniTask/Awaitable native support |

**➡️ Detailed Comparison: [Library Comparison Documentation](./docs/library-comparison.md)**

---

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

### Optional Dependencies

UniFP works standalone, but you can enhance async functionality by installing one of the following:

**Option 1: UniTask** (Recommended for Unity 2022.3+)
- More features and better performance than Unity's Awaitable
- Install via UPM:
  ```text
  https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
  ```
- Enables `AsyncResult.ThenAsync`, `MapAsync`, `FilterAsync`, `DoAsync`, `TryAsync`

**Option 2: Unity Awaitable** (Unity 6.0+)
- Built into Unity 6.0+, no installation needed
- Automatically detected via `versionDefines` in UniFP.asmdef
- Provides the same async API as UniTask

**Without async support:**
- All synchronous `Result<T>` features work perfectly
- Async extensions won't be available

## Core Concepts

### `Result<T>` Usage

`Result<T>` expresses **success** (Success) or **failure** (Failure) as types, freeing you from if/else and try/catch hell.

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

#### Creating a Result

```csharp
using UniFP;

// 1. Create directly with Success / Failure
var success = Result<int>.Success(42);
var failure = Result<int>.Failure(ErrorCode.NotFound);
var failureWithMsg = Result<int>.Failure(ErrorCode.ValidationFailed, "Age must be greater than 0");

// 2. Promote a value to Result with FromValue
var fromValue = Result.FromValue(userId);

// 3. Convert exceptions to Result with Try
var parseResult = Result.Try(() => int.Parse(input));
var parseWithCode = Result.Try(() => int.Parse(input), ErrorCode.InvalidInput);
```

#### Core Methods: Then, Map, Filter

```csharp
// Then: Chain Result-returning functions
Result<User> LoadUser(int id) => /* ... */;
var result = Result.FromValue(42)
    .Then(LoadUser);  // int -> Result<User>

// Map: Transform regular value-returning functions
var doubled = Result.FromValue(10)
    .Map(x => x * 2);  // int -> int (automatically wrapped in Result<int>)

// Filter: Conditional validation (Failure on condition fail)
var validated = Result.FromValue(age)
    .Filter(x => x >= 18, ErrorCode.ValidationFailed, "Adults only");
```

> **💡 Tip: Then vs Map**
> - `Then` is used for functions that return Result (operations that can fail)
> - `Map` is used for functions that return regular values (simple transformations)

#### Error Handling and Recovery

```csharp
// Recover: Replace failure with default value
var withDefault = LoadConfig()
    .Recover(code => DefaultConfig);

// IfFailed: Execute alternative pipeline on failure
var cached = LoadFromServer()
    .IfFailed(() => LoadFromCache());

// Catch: Intercept and recover from specific errors
var result = LoadResource()
    .Catch(ErrorCode.NotFound, () => CreateDefault());

// Match: Different handling based on success/failure
result.Match(
    onSuccess: user => Debug.Log($"Welcome, {user.Name}"),
    onFailure: code => Debug.LogError($"Load failed: {code}"));
```

#### Side Effects

```csharp
// Do: Execute side effect only on success (skip on failure)
var result = LoadUser(id)
    .Do(user => Analytics.Track("UserLoaded", user.Id))
    .Do(user => Debug.Log($"Loaded: {user.Name}"));

// DoStrict: Abort pipeline if side effect fails
var saved = CreateUser(data)
    .DoStrict(user => SaveToDatabase(user));  // Entire operation fails if DB save fails

// IfFailed: Execute side effect only on failure
var result = Process()
    .IfFailed(code => Debug.LogError($"Processing failed: {code}"));
```

#### Conditional Execution

```csharp
// ThenIf / MapIf: Selectively transform based on condition
var result = LoadUser(id)
    .ThenIf(
        condition: user => user.IsPremium,
        thenFunc: user => LoadPremiumData(user),
        elseFunc: user => Result<UserData>.Success(user.BasicData));

var processed = Result.FromValue(input)
    .MapIf(
        condition: x => x > 100,
        thenFunc: x => x / 2,
        elseFunc: x => x);
```

#### Async Result (UniTask / Awaitable)

```csharp
using Cysharp.Threading.Tasks;  // or using UnityEngine; (Awaitable)

// ThenAsync: Async Result chaining
async UniTask<Result<User>> LoadUserAsync(int id)
{
    return await Result.FromValue(id)
        .Filter(x => x > 0, ErrorCode.InvalidInput)
        .ThenAsync(async id => await FetchFromAPI(id))
        .MapAsync(json => ParseUser(json))
        .FilterAsync(user => UniTask.FromResult(user.IsActive), "Inactive user");
}

// TryAsync: Convert exception-throwing async work to Result
var result = await AsyncResult.TryAsync(async () => 
{
    var response = await httpClient.GetAsync(url);
    return await response.Content.ReadAsStringAsync();
}, ErrorCode.NetworkError);

// DoAsync: Async side effects
var saved = await LoadUser(id)
    .DoAsync(async user => await SaveToCloud(user));
```

---

### `Option<T>` Usage

`Option<T>` expresses **has value** (Some) or **no value** (None) as types, freeing you from null hell.

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

#### Creating an Option

```csharp
using UniFP;

// 1. Create directly with Some / None
var some = Option<int>.Some(42);
var none = Option<int>.None();

// 2. Convert nullable values to Option with From (null becomes None)
var fromValue = Option<string>.From(PlayerPrefs.GetString("username"));  // null becomes None
var fromNullable = Option<int>.From(nullableInt);

// 3. Conditional conversion with Where (None on condition fail)
var adult = Option<int>.From(age)
    .Where(x => x >= 18);
```

#### Core Option Methods

```csharp
// Map: Transform value (skip if None)
var doubled = Option<int>.Some(10)
    .Map(x => x * 2);  // Some(20)

var stillNone = Option<int>.None()
    .Map(x => x * 2);  // None

// Bind: Chain Option-returning functions
Option<User> FindUser(string name) => /* ... */;
var user = Option<string>.From(username)
    .Bind(FindUser);

// Filter: Conditional validation (None on fail)
var valid = Option<int>.From(input)
    .Filter(x => x > 0)
    .Filter(x => x < 100);

// Or / OrElse: Provide alternative value when None
var withDefault = Option<string>.None()
    .Or(Option<string>.Some("default value"));

var fromFunc = Option<int>.None()
    .OrElse(() => Option<int>.Some(GetDefaultValue()));

// GetValueOrDefault: Extract value from Option
var value = someOption.GetValueOrDefault(defaultValue);
var valueOrNull = someOption.GetValueOrDefault();
```

#### Option and Result Conversion

```csharp
// Option -> Result: Convert None to error
var result = Option<User>.From(FindUser(id))
    .ToResult(ErrorCode.NotFound, "User not found");

// Result -> Option: Convert failure to None (ignore error)
var option = LoadConfig()
    .ToOption();  // Success -> Some, Failure -> None
```

#### Branching with Match

```csharp
// Match: Different handling based on Some/None
var message = Option<User>.From(user).Match(
    onSome: u => $"Welcome, {u.Name}",
    onNone: () => "Guest mode");

// IfSome / IfNone: Handle only one case
Option<Config>.From(config)
    .IfSome(c => ApplyConfig(c))
    .IfNone(() => UseDefaults());
```

#### Collection Helpers

```csharp
using System.Linq;

var items = new[] { 1, 2, 3, 4, 5 };

// TryFirst / TryLast: First/last element as Option
var first = items.TryFirst();  // Some(1)
var firstEven = items.TryFirst(x => x % 2 == 0);  // Some(2)
var empty = Array.Empty<int>().TryFirst();  // None

// TryFind: Find element matching condition
var found = items.TryFind(x => x > 3);  // Some(4)

// Choose: Extract only Some from Option collection
var options = new[] 
{ 
    Option<int>.Some(1), 
    Option<int>.None(), 
    Option<int>.Some(3) 
};
var values = options.Choose();  // [1, 3]
```

#### LINQ Integration

```csharp
using System.Linq;

// Select: Same as Map
var doubled = Option<int>.Some(10)
    .Select(x => x * 2);  // Some(20)

// Where: Same as Filter
var filtered = Option<int>.Some(42)
    .Where(x => x > 18);  // Some(42)

// SelectMany: Same as Bind (LINQ query syntax support)
var result = 
    from name in Option<string>.From(username)
    from user in FindUser(name)
    from profile in LoadProfile(user.Id)
    select profile;
```

---

### `NonEmpty<T>` Usage

`NonEmpty<T>` is a collection that **guarantees at least one element**. Suitable for domains where emptiness is not allowed, such as party composition or required slots.

#### Creating a NonEmpty

```csharp
using UniFP;

// Create: Create with at least one element
var squad = NonEmpty.Create("Leader", "Support", "Tank");
var single = NonEmpty.Create(42);

// FromList: Convert from list (fail if empty)
var list = new List<string> { "A", "B", "C" };
var nonEmpty = NonEmpty.FromList(list);  // Result<NonEmpty<string>>

var emptyList = new List<string>();
var failed = NonEmpty.FromList(emptyList);  // Failure (empty)
```

#### NonEmpty Methods

```csharp
// Head / Tail: First element and rest
var squad = NonEmpty.Create("Leader", "Tank", "Healer");
var leader = squad.Head;  // "Leader" (always exists)
var others = squad.Tail;  // ["Tank", "Healer"] (IEnumerable)

// Map: Transform all elements
var upper = squad.Map(role => role.ToUpper());  // NonEmpty<string>

// Append / Prepend: Add elements
var expanded = squad.Append("Mage");  // NonEmpty (still at least one)
var withNewLeader = squad.Prepend("NewLeader");

// ToList / ToArray: Convert to regular collection
var list = squad.ToList();
var array = squad.ToArray();
```

#### Usage Examples

```csharp
// Party system: At least one leader required
public class Party
{
    private readonly NonEmpty<Player> _members;

    public Party(Player leader, params Player[] others)
    {
        _members = NonEmpty.Create(leader, others);
    }

    public Player Leader => _members.Head;
    public IEnumerable<Player> AllMembers => _members;

    public void Buff()
    {
        // Compile-time guarantee of at least one member
        _members.Map(p => p.ApplyBuff());
    }
}

// Configuration: At least one server address required
var servers = NonEmpty.Create(
    "https://primary.server.com",
    "https://backup1.server.com",
    "https://backup2.server.com"
);

var primary = servers.Head;
var fallbacks = servers.Tail;
```

---

### Error Codes and Diagnostics

UniFP provides **Zero-GC error classification** with the `ErrorCode` struct.

#### Built-in ErrorCode

```csharp
// 0-999: UniFP reserved range
ErrorCode.None              // 0: No error
ErrorCode.Unknown           // 1: Unknown error
ErrorCode.InvalidInput      // 100: Invalid input
ErrorCode.ValidationFailed  // 101: Validation failed
ErrorCode.NotFound          // 102: Not found
ErrorCode.Unauthorized      // 103: Unauthorized
ErrorCode.OperationFailed   // 104: Operation failed
ErrorCode.Timeout           // 105: Timeout
ErrorCode.NetworkError      // 106: Network error
ErrorCode.Forbidden         // 107: Forbidden
ErrorCode.InvalidOperation  // 108: Invalid operation
```

#### Custom ErrorCode

```csharp
// 1000+: User-defined error codes
public static class GameErrors
{
    public static readonly ErrorCode InsufficientGold = 
        ErrorCode.Custom(1001, "Economy");
    
    public static readonly ErrorCode InventoryFull = 
        ErrorCode.Custom(1002, "Inventory");
    
    public static readonly ErrorCode QuestNotAvailable = 
        ErrorCode.Custom(1003, "Quest");
}

// Usage example
var result = PurchaseItem(itemId, price)
    .Filter(success => player.Gold >= price, GameErrors.InsufficientGold, 
            $"Insufficient gold: need {price - player.Gold} more");
```

#### ErrorCode Properties

```csharp
var error = ErrorCode.NotFound;

error.Code;       // 102
error.Category;   // "Resource"
error.IsCustom;   // false (built-in code)

var custom = ErrorCode.Custom(2001, "Payment");
custom.Code;      // 2001
custom.Category;  // "Payment"
custom.IsCustom;  // true
```

#### Diagnostic Information (Debug Mode)

```csharp
// Automatically recorded in Editor or UNIFP_DEBUG environment
var result = LoadAsset(path)
    .Filter(asset => asset != null, ErrorCode.NotFound);

if (result.IsFailure)
{
    // Information automatically recorded on failure
    Debug.LogError($"[{result.ErrorCode.Category}] {result.Error}");
    Debug.LogError($"Location: {result.FilePath}:{result.LineNumber}");
    Debug.LogError($"Method: {result.MemberName}");
    Debug.LogError($"Operation type: {result.OperationType}");
    
    // Example output:
    // [Resource] Asset not found: player_model.prefab
    // Location: Assets/Scripts/AssetLoader.cs:42
    // Method: LoadPlayerModel
    // Operation type: Filter
}
```

---

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

## Async Support (UniTask / Awaitable)

UniFP supports async operations with both **UniTask** (recommended) and **Unity Awaitable** (Unity 6.0+).

**With UniTask installed:**
```csharp
using Cysharp.Threading.Tasks;

async UniTask<Result<PlayerData>> FetchPlayer(int id)
{
    return await Result.TryFromResult(() => ValidateId(id))
        .ThenAsync(async _ => await Api.GetPlayer(id))
        .MapAsync(payload => payload.ToPlayerData())
        .FilterAsync(data => UniTask.FromResult(data.IsActive), "Player is not active");
}
```

**With Unity 6.0+ (Awaitable):**
```csharp
using UnityEngine;

async Awaitable<Result<PlayerData>> FetchPlayer(int id)
{
    return await Result.TryFromResult(() => ValidateId(id))
        .ThenAsync(async _ => await Api.GetPlayer(id))
        .MapAsync(payload => payload.ToPlayerData())
        .FilterAsync(data => Awaitable.FromResult(data.IsActive), "Player is not active");
}
```

Both provide the same API - just swap the async type!

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
