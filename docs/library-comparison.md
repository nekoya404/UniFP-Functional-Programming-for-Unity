# Library Comparison

## Table of Contents

- [UniFP vs Unity-NOPE](#unifp-vs-unity-nope)
  - [Performance Comparison](#performance-comparison)
  - [Feature Comparison](#feature-comparison)
  - [Detailed Method Comparison](#detailed-method-comparison)
  - [Error Typing: Unnecessary in 99% of Cases](#error-typing-unnecessary-in-99-of-cases)
- [UniFP vs language-ext](#unifp-vs-language-ext)
  - [Why Not Use language-ext Directly in Unity?](#why-not-use-language-ext-directly-in-unity)
  - [Feature Comparison](#feature-comparison-1)

---

## UniFP vs Unity-NOPE

### Performance Comparison

UniFP improves upon NOPE's performance issues.

**1. Zero-GC Struct Design**
- UniFP: All core types are `readonly struct` allocated on stack
- Unity-NOPE: `Result<T,E>` is `readonly struct`, but generic error type `E` can cause boxing

**2. Delegate Caching**
- UniFP: `DelegateCache` reuses frequently-used lambdas → prevents heap allocation
- Unity-NOPE: No delegate caching → repeated creation in Update loops

**3. ResultPool & ListPool**
- UniFP: Built-in object pooling for high-frequency scenarios
- Unity-NOPE: No pooling mechanism

### Feature Comparison

UniFP implements all core features from NOPE, but with C#-friendly naming.
UniFP's `Then` = NOPE's `Bind`, UniFP's `Filter` = NOPE's `Ensure`

**High-Level Feature Comparison**

| Feature | UniFP | Unity-NOPE |
|---------|-------|------------|
| Result Monad | `Result<T>` (single type) | `Result<T,E>` (typed errors) |
| Option Monad | `Option<T>` | `Maybe<T>` |
| Async Support | UniTask + Awaitable | UniTask + Awaitable |
| Error Type | `ErrorCode` (struct, efficient) | `E` (generic, flexible but can box) |
| Pipeline Operations | Then, Map, Filter, Recover, Do... | Bind, Map, Ensure, Tap, Finally... |
| Retry Logic | Retry, RetryWithBackoff, Repeat | Not supported |
| Result Combination | ResultCombinators (Combine, Zip...) | Result.Combine, CombineValues |
| Collection Traversal | SelectResults, CombineAll, Partition | Limited |
| Performance Optimization | DelegateCache, Pools, Span extensions | Basic structs only |
| Debugging Tools | Trace, Breakpoint, SafeExecutor | Basic Match only |

### Detailed Method Comparison

| Method Category | UniFP | Unity-NOPE | Description |
|----------------|-------|------------|-------------|
| **Basic Transformations** |
| `Map` | ✅ | ✅ | Transform value on success (T → U) |
| `Bind` (Then) | ✅ `Then` | ✅ `Bind` | Chain functions returning Result (T → Result\<U\>) |
| `Filter` | ✅ | ⚠️ `Ensure` | Conditional validation (fails to Failure) |
| **Error Handling** |
| `MapError` | ⚠️ ErrorCode only | ✅ | Transform error type |
| `Recover` | ✅ | ⚠️ `OrElse` | Replace failure with default value |
| `IfFailed` | ✅ | ⚠️ `Or` | Provide alternative Result on failure |
| `Catch` | ✅ | ❌ | Intercept specific errors for recovery |
| **Side Effects** |
| `Do` | ✅ | ⚠️ `Tap` | Execute side effect on success (no value change) |
| `DoStrict` | ✅ | ❌ | Abort pipeline if side effect fails |
| `IfFailed(Action)` | ✅ | ❌ | Execute side effect only on failure |
| **Conditional Execution** |
| `ThenIf` | ✅ | ❌ | Conditional Then |
| `MapIf` | ✅ | ❌ | Conditional Map |
| `Where` | ⚠️ Option only | ✅ Maybe only | Condition filtering |
| **Result Inspection** |
| `Match` | ✅ | ✅ | Execute different functions based on success/failure |
| `Finally` | ⚠️ Similar to `Match` | ✅ | Chain termination and final processing |
| `Assert` | ✅ | ⚠️ Similar to `Ensure` | Condition validation (for debugging) |
| **Async (UniTask/Awaitable)** |
| `ThenAsync` | ✅ | ⚠️ `Bind` overload | Async Result chaining |
| `MapAsync` | ✅ | ⚠️ `Map` overload | Async value transformation |
| `FilterAsync` | ✅ | ❌ | Async condition validation |
| `DoAsync` | ✅ | ❌ | Async side effects |
| `TryAsync` | ✅ | ⚠️ `Of` | Convert exceptions to Result (async) |
| **Resilience** |
| `Retry` | ✅ | ❌ | Retry on failure |
| `RetryAsync` | ✅ | ❌ | Async retry |
| `RetryWithBackoff` | ✅ | ❌ | Exponential backoff retry |
| `Repeat` | ✅ | ❌ | Require N consecutive successes |
| **Result Combination** |
| `Combine` | ✅ | ✅ | Combine multiple Results |
| `Zip` | ✅ | ⚠️ `CombineValues` | Combine multiple Results into tuple |
| `CombineAll` | ✅ | ❌ | List\<Result\> → Result\<List\> |
| `Partition` | ✅ | ❌ | Separate success/failure |
| **Collection Extensions** |
| `SelectResults` | ✅ | ❌ | Collection → List\<Result\>, abort on failure |
| `FilterResults` | ✅ | ❌ | Conditional filtering + Result return |
| `Fold` | ✅ | ❌ | Collection aggregation (returns Result) |
| `AggregateResults` | ✅ | ❌ | Complex aggregation logic |
| **Creation Helpers** |
| `Success` | ✅ | ✅ | Create success Result |
| `Failure` | ✅ | ✅ | Create failure Result |
| `FromValue` | ✅ | ⚠️ implicit | Create Result from value |
| `SuccessIf` | ⚠️ Similar to `Filter` | ✅ | Conditional success/failure creation |
| `FailureIf` | ⚠️ Opposite of `Filter` | ✅ | Conditional failure/success creation |
| `Of` | ⚠️ `Try` | ✅ | Exception → Result conversion |
| **Safe Operations** |
| `BindSafe` | ❌ | ✅ | Bind with exception handling |
| `MapSafe` | ❌ | ✅ | Map with exception handling |
| `TapSafe` | ❌ | ✅ | Tap with exception handling |
| **Debugging** |
| `Trace` | ✅ | ❌ | Trace pipeline steps |
| `TraceWith` | ✅ | ❌ | Trace with custom message |
| `TraceOnFailure` | ✅ | ❌ | Trace only on failure |
| `Breakpoint` | ✅ | ❌ | Set debugger breakpoint |

**Legend:**
- ✅ Fully supported
- ⚠️ Partially supported or provided with different name
- ❌ Not supported

### Error Typing: Unnecessary in 99% of Cases

Unity-NOPE allows error typing with `Result<T,E>`, but this is **over-engineering** for most Unity game development:

**Why Typed Errors Are Unnecessary:**
- Unity game logic mainly cares about "Did it succeed? Did it fail?"
- Error **messages** are more useful than **types** (for debugging/logging)
- Typed errors increase generic parameters → code complexity rises
- Most failures are simple categories like "resource load failed", "validation failed"

**UniFP's Approach: ErrorCode Struct**
```csharp
// UniFP: Efficient and clear error categorization
var result = LoadAsset()
    .Filter(x => x != null, ErrorCode.NotFound)
    .Then(ValidateAsset);  // Can return ErrorCode.ValidationFailed

if (result.IsFailure)
{
    Debug.LogError($"[{result.ErrorCode.Category}] {result.Error}");
    // [Resource] Asset not found: player_model.prefab
}
```

**The 1% Case: When Type-Safe Errors Are Needed**

For complex domain logic where typed errors are truly necessary:

```csharp
// Method 1: Custom ErrorCode
public static class PaymentErrors
{
    public static readonly ErrorCode InsufficientFunds = ErrorCode.Custom(1001, "Payment");
    public static readonly ErrorCode InvalidCard = ErrorCode.Custom(1002, "Payment");
    public static readonly ErrorCode NetworkTimeout = ErrorCode.Custom(1003, "Payment");
}

var paymentResult = ProcessPayment()
    .Recover(code => code == PaymentErrors.NetworkTimeout 
        ? RetryPayment() 
        : RefundUser());

// Method 2: Discriminated Union Pattern (C# 9.0+)
public record PaymentError
{
    public record InsufficientFunds(decimal Required, decimal Available) : PaymentError;
    public record InvalidCard(string CardNumber) : PaymentError;
    public record NetworkTimeout(int Attempts) : PaymentError;
}

// Serialize to Result's Error message
var result = payment switch
{
    PaymentError.InsufficientFunds e => 
        Result<Payment>.Failure(ErrorCode.Custom(1001, "Payment"), 
                                $"Insufficient: {e.Required - e.Available} more needed"),
    // ...
};
```

---

## UniFP vs language-ext

### Why Not Use language-ext Directly in Unity?

language-ext is the best functional library in the .NET ecosystem, but it's not suitable for Unity.

**1. No Unity Runtime Optimization**
- language-ext is designed for general .NET
- Many types are class-based → increased GC pressure
- Potential compatibility issues with Unity's IL2CPP AOT compilation

**2. Overwhelming Feature Complexity**
- 100+ monads and transformers
- Higher-kinded types simulation (complex generic patterns)
- Unnecessary for game dev: Parsec, Lenses, Free monads, etc.

**3. Learning Curve**
- Haskell-style naming conventions (`camelCase` static functions)
- Complex abstractions in Trait system
- Excessive functional concepts unfamiliar to Unity developers

**4. Performance Overhead**
- Indirect calls due to high abstraction
- Difficult to identify hot paths in Unity Profiler

### Feature Comparison

| Category | language-ext | UniFP | Unity Game Dev Perspective |
|----------|--------------|-------|---------------------------|
| **Core Monads** | Option, Either, Try, Validation, Fin | Result, Option, NonEmpty | UniFP provides Unity-specific minimal set ✅ |
| **Immutable Collections** | Arr, Lst, Seq, Map, HashMap, Set... | Standard C# collections + extensions | language-ext superior but excessive for Unity ⚠️ |
| **Async** | IO monad, Eff, Pipes, StreamT | AsyncResult (UniTask/Awaitable) | UniFP better Unity ecosystem integration ✅ |
| **Error Handling** | Either<L,R>, Validation<E,S>, Fin<A> | Result<T> + ErrorCode | UniFP simpler and clearer ✅ |
| **Parser Combinators** | Parsec (full implementation) | Not supported | Unnecessary for games (language-ext wins) ❌ |
| **Lenses & Optics** | Full support | Not supported | Excessive for games (Unreal's FProperty more suitable) ❌ |
| **Atomic Concurrency** | Atom, Ref, AtomHashMap | Not supported | Unity is single-threaded focused, use C# standard if needed ⚠️ |
| **Performance** | Overhead from high abstraction | Zero-GC structs, pooling optimization | UniFP optimized for Unity ✅ |
| **Learning Curve** | Steep (requires Haskell background) | Gentle (C# LINQ experience sufficient) | UniFP better accessibility ✅ |
