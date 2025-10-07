# 다른 라이브러리와의 비교

## 목차

- [UniFP vs Unity-NOPE](#unifp-vs-unity-nope)
  - [성능 비교](#성능-비교)
  - [기능 비교](#기능-비교)
  - [메서드별 상세 비교](#메서드별-상세-비교)
  - [에러 타입화: 99%의 경우 불필요합니다](#에러-타입화-99의-경우-불필요합니다)
- [UniFP vs language-ext](#unifp-vs-language-ext)
  - [왜 language-ext를 Unity에 바로 쓰지 않는가?](#왜-language-ext를-unity에-바로-쓰지-않는가)
  - [기능 비교](#기능-비교-1)

---

## UniFP vs Unity-NOPE

### 성능 비교

NOPE의 성능문제를 개선했습니다.

**1. Zero-GC 구조체 설계**
- UniFP: 모든 핵심 타입이 `readonly struct`로 스택 할당됨
- Unity-NOPE: `Result<T,E>`가 `readonly struct`지만, 제네릭 에러 타입 `E`가 박싱을 유발할 수 있음

**2. 델리게이트 캐싱**
- UniFP: `DelegateCache`로 자주 사용되는 람다 재사용 → 힙 할당 방지
- Unity-NOPE: 델리게이트 캐싱 없음 → Update 루프에서 반복 생성

**3. ResultPool & ListPool**
- UniFP: 고빈도 시나리오를 위한 객체 풀링 내장
- Unity-NOPE: 풀링 메커니즘 없음

### 기능 비교

NOPE에 핵심적인 기능은 UniFP에도 구현되어있습니다. 다만 이름은 UniFP에서 C#사용자에 더 친숙하게 명명되었습니다.
UniFP의 `Then` = NOPE의 `Bind`, UniFP의 `Filter` = NOPE의 `Ensure`

**고수준 기능 비교**

| 기능 | UniFP | Unity-NOPE |
|------|-------|------------|
| Result 모나드 | `Result<T>` (단일 타입) | `Result<T,E>` (타입화된 에러) |
| Option 모나드 | `Option<T>` | `Maybe<T>` |
| 비동기 지원 | UniTask + Awaitable | UniTask + Awaitable |
| 에러 타입 | `ErrorCode` (구조체, 효율적) | `E` (제네릭, 유연하지만 박싱 가능) |
| 파이프라인 연산 | Then, Map, Filter, Recover, Do... | Bind, Map, Ensure, Tap, Finally... |
| 재시도 로직 | Retry, RetryWithBackoff, Repeat | 미지원 |
| 결과 결합 | ResultCombinators (Combine, Zip...) | Result.Combine, CombineValues |
| 컬렉션 순회 | SelectResults, CombineAll, Partition | 제한적 |
| 성능 최적화 | DelegateCache, Pools, Span 확장 | 기본 구조체만 |
| 디버깅 도구 | Trace, Breakpoint, SafeExecutor | 기본 Match만 |

### 메서드별 상세 비교

| 메서드 카테고리 | UniFP | Unity-NOPE | 설명 |
|---------------|-------|------------|------|
| **기본 변환** |
| `Map` | ✅ | ✅ | 성공 시 값 변환 (T → U) |
| `Bind` (Then) | ✅ `Then` | ✅ `Bind` | Result를 반환하는 함수 체이닝 (T → Result\<U\>) |
| `Filter` | ✅ | ⚠️ `Ensure` | 조건부 검증 (조건 실패 시 Failure) |
| **에러 처리** |
| `MapError` | ⚠️ ErrorCode만 | ✅ | 에러 타입 변환 |
| `Recover` | ✅ | ⚠️ `OrElse` | 실패를 기본값으로 복구 |
| `IfFailed` | ✅ | ⚠️ `Or` | 실패 시 대체 Result 제공 |
| `Catch` | ✅ | ❌ | 특정 에러를 가로채서 복구 |
| **부수 효과** |
| `Do` | ✅ | ⚠️ `Tap` | 성공 시 부수 효과 실행 (값 변경 없음) |
| `DoStrict` | ✅ | ❌ | 부수 효과 실패 시 파이프라인 중단 |
| `IfFailed(Action)` | ✅ | ❌ | 실패 시만 부수 효과 실행 |
| **조건부 실행** |
| `ThenIf` | ✅ | ❌ | 조건부 Then |
| `MapIf` | ✅ | ❌ | 조건부 Map |
| `Where` | ⚠️ Option에만 | ✅ Maybe에만 | 조건 필터링 |
| **결과 검사** |
| `Match` | ✅ | ✅ | 성공/실패에 따라 다른 함수 실행 |
| `Finally` | ⚠️ `Match` 유사 | ✅ | 체인 종료 및 최종 처리 |
| `Assert` | ✅ | ⚠️ `Ensure` 유사 | 조건 검증 (디버그용) |
| **비동기 (UniTask/Awaitable)** |
| `ThenAsync` | ✅ | ⚠️ `Bind` 오버로드 | 비동기 Result 체이닝 |
| `MapAsync` | ✅ | ⚠️ `Map` 오버로드 | 비동기 값 변환 |
| `FilterAsync` | ✅ | ❌ | 비동기 조건 검증 |
| `DoAsync` | ✅ | ❌ | 비동기 부수 효과 |
| `TryAsync` | ✅ | ⚠️ `Of` | 예외를 Result로 변환 (비동기) |
| **복원력** |
| `Retry` | ✅ | ❌ | 실패 시 재시도 |
| `RetryAsync` | ✅ | ❌ | 비동기 재시도 |
| `RetryWithBackoff` | ✅ | ❌ | 지수 백오프 재시도 |
| `Repeat` | ✅ | ❌ | N번 연속 성공 필요 |
| **결과 결합** |
| `Combine` | ✅ | ✅ | 여러 Result 결합 |
| `Zip` | ✅ | ⚠️ `CombineValues` | 여러 Result를 튜플로 결합 |
| `CombineAll` | ✅ | ❌ | List\<Result\> → Result\<List\> |
| `Partition` | ✅ | ❌ | 성공/실패 분리 |
| **컬렉션 확장** |
| `SelectResults` | ✅ | ❌ | 컬렉션 → List\<Result\>, 실패 시 중단 |
| `FilterResults` | ✅ | ❌ | 조건부 필터링 + Result 반환 |
| `Fold` | ✅ | ❌ | 컬렉션 집계 (Result 반환) |
| `AggregateResults` | ✅ | ❌ | 복잡한 집계 로직 |
| **생성 헬퍼** |
| `Success` | ✅ | ✅ | 성공 Result 생성 |
| `Failure` | ✅ | ✅ | 실패 Result 생성 |
| `FromValue` | ✅ | ⚠️ implicit | 값에서 Result 생성 |
| `SuccessIf` | ⚠️ `Filter` 유사 | ✅ | 조건부 성공/실패 생성 |
| `FailureIf` | ⚠️ `Filter` 반대 | ✅ | 조건부 실패/성공 생성 |
| `Of` | ⚠️ `Try` | ✅ | 예외 → Result 변환 |
| **안전 연산** |
| `BindSafe` | ❌ | ✅ | 예외 처리가 포함된 Bind |
| `MapSafe` | ❌ | ✅ | 예외 처리가 포함된 Map |
| `TapSafe` | ❌ | ✅ | 예외 처리가 포함된 Tap |
| **디버깅** |
| `Trace` | ✅ | ❌ | 파이프라인 단계 추적 |
| `TraceWith` | ✅ | ❌ | 커스텀 메시지 추적 |
| `TraceOnFailure` | ✅ | ❌ | 실패 시만 추적 |
| `Breakpoint` | ✅ | ❌ | 디버거 중단점 설정 |

**범례:**
- ✅ 완전 지원
- ⚠️ 부분 지원 또는 다른 이름으로 제공
- ❌ 미지원

### 에러 타입화: 99%의 경우 불필요합니다

Unity-NOPE는 `Result<T,E>`로 에러를 타입화할 수 있지만, Unity 게임 개발에서는 **대부분 오버엔지니어링**입니다:

**왜 타입화된 에러가 불필요한가?**
- Unity 게임 로직은 주로 "성공했나? 실패했나?"만 중요
- 에러의 **종류**보다 **메시지**가 더 유용 (디버깅/로깅 시)
- 타입화된 에러는 제네릭 파라미터 증가 → 코드 복잡도 상승
- 대부분의 실패는 "리소스 로드 실패", "유효성 검사 실패" 등 단순한 범주

**UniFP의 접근: ErrorCode 구조체**
```csharp
// UniFP: 효율적이고 명확한 에러 분류
var result = LoadAsset()
    .Filter(x => x != null, ErrorCode.NotFound)
    .Then(ValidateAsset);  // ErrorCode.ValidationFailed 반환 가능

if (result.IsFailure)
{
    Debug.LogError($"[{result.ErrorCode.Category}] {result.Error}");
    // [Resource] Asset not found: player_model.prefab
}
```

**1%의 경우: 타입 안전한 에러가 필요할 때**

복잡한 도메인 로직에서 정말로 타입화된 에러가 필요하다면?

```csharp
// 방법 1: 커스텀 ErrorCode 사용
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

// 방법 2: 판별된 유니온 패턴 (C# 9.0+)
public record PaymentError
{
    public record InsufficientFunds(decimal Required, decimal Available) : PaymentError;
    public record InvalidCard(string CardNumber) : PaymentError;
    public record NetworkTimeout(int Attempts) : PaymentError;
}

// Result의 Error 메시지에 직렬화하여 저장
var result = payment switch
{
    PaymentError.InsufficientFunds e => 
        Result<Payment>.Failure(ErrorCode.Custom(1001, "Payment"), 
                                $"부족: {e.Required - e.Available}원"),
    // ...
};
```

---

## UniFP vs language-ext

### 왜 language-ext를 Unity에 바로 쓰지 않는가?

language-ext는 .NET 생태계 최고의 함수형 라이브러리이지만, Unity에는 적합하지 않습니다

**1. Unity 런타임 최적화 부재**
- language-ext는 범용 .NET을 위해 설계됨
- 많은 타입이 클래스 기반 → GC 압박 증가
- Unity의 IL2CPP AOT 컴파일과 호환성 이슈 가능

**2. 압도적인 기능 복잡도**
- 100개 이상의 모나드와 트랜스포머
- Higher-kinded types 시뮬레이션 (복잡한 제네릭 패턴)
- 게임 개발에 불필요한 Parsec, Lenses, Free monads 등

**3. 학습 곡선**
- Haskell 스타일 명명 규칙 (`camelCase` 정적 함수)
- Trait 시스템의 복잡한 추상화
- Unity 개발자에게 낯선 함수형 개념 과다

**4. 성능 오버헤드**
- 고도의 추상화로 인한 간접 호출
- Unity 프로파일러에서 핫패스 식별 어려움

### 기능 비교

| 분류 | language-ext | UniFP | Unity 게임 개발 관점 |
|------|--------------|-------|---------------------|
| **핵심 모나드** | Option, Either, Try, Validation, Fin | Result, Option, NonEmpty | UniFP가 Unity에 특화된 최소 집합 제공 ✅ |
| **불변 컬렉션** | Arr, Lst, Seq, Map, HashMap, Set... | 기본 C# 컬렉션 + 확장 메서드 | language-ext 우수하나 Unity에는 과다 ⚠️ |
| **비동기** | IO monad, Eff, Pipes, StreamT | AsyncResult (UniTask/Awaitable) | UniFP가 Unity 생태계 통합 우수 ✅ |
| **에러 처리** | Either<L,R>, Validation<E,S>, Fin<A> | Result<T> + ErrorCode | UniFP가 단순하고 명확 ✅ |
| **파서 컴비네이터** | Parsec (완전 구현) | 미지원 | 게임에 불필요 (language-ext 승) ❌ |
| **Lenses & Optics** | 완전 지원 | 미지원 | 게임에 과다 (언리얼 FProperty 등이 더 적합) ❌ |
| **Atomic 동시성** | Atom, Ref, AtomHashMap | 미지원 | Unity는 단일 스레드 중심, 필요 시 C# 표준 사용 ⚠️ |
| **성능** | 고도 추상화로 오버헤드 | Zero-GC 구조체, 풀링 최적화 | UniFP가 Unity에 최적화 ✅ |
| **학습 곡선** | 가파름 (Haskell 배경 필요) | 완만함 (C# LINQ 경험만으로 시작 가능) | UniFP가 접근성 우수 ✅ |
