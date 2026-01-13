![C# Functional Programming for Unity Capsule Header](https://capsule-render.vercel.app/api?type=waving&height=220&color=0:5A2BFF,100:1FB5E9&text=C%23%20Functional%20Programming%20for%20Unity&fontAlign=50&fontAlignY=40&fontSize=46&fontColor=FFFFFF&desc=UniFP&descAlign=50&descAlignY=65&descSize=24)

[English](./README.md) · [한국어](./README.ko.md) · [简体中文](./README.zh-CN.md) · [日本語](./README.ja.md)

# UniFP — C# Functional Programming for Unity

[![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-000?logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-success.svg)](./LICENSE)
[![Version](https://img.shields.io/github/package-json/v/Nekoya-Jin/UniFP?filename=src%2FUniFP%2FAssets%2FPlugins%2FUniFP%2Fpackage.json&label=version&color=blue)](https://github.com/Nekoya-Jin/UniFP/releases)

UniFP는 Rust과 Haskell, F# 영감을 받아, Unity 게임 로직에 함수형 사고방식과 명시적 에러 처리를 도입하는 GC ZERO allocation C# 함수형 프로그래밍 프레임워크입니다.

기존 C# 함수형 라이브러리(예: [language-ext](https://github.com/louthy/language-ext))는 범용 .NET 환경을 겨냥해 방대한 기능과 복잡한 추상화를 포함하고 있으며, 그만큼 학습 곡선이 가파르고 구조체를 활용하지 않는 경우가 많아 Unity 런타임에서 GC 할당과 성능 손실이 발생하기 쉽습니다. 

이에 Rust 언어가 보여주는 타입 시스템 기반의 안정성과 성능 중심 철학, 함수형 프로그래밍 언어들의 레일웨이 프로그래밍 패러다임을 Unity C# 환경에 접목하여, 무거운 의존성 없이도 게임 플레이 코드에서 안전한 오류 처리와 선언적 파이프라인을 활용할 수 있도록 실시간 애플리케이션에 최적화된 경량 대안을 목표로 UniFP를 개발했습니다.

`Result<T>`와 `Option<T>`를 기반으로 한 파이프라인 확장을 제공하여 예외 대신 타입 안전한 흐름 제어를 구현하면서도 GC 부담은 최소화합니다.

> 모든 핵심 타입은 구조체로 제공되며, Editor 또는 `UNIFP_DEBUG` 환경에서는 각 연산이 발생한 파일/라인/메서드 정보를 자동으로 기록합니다. 별도의 설정 없이 Unity 프로젝트에 바로 붙여 사용할 수 있습니다.


 **UniFP에서 오해하지 말아야할 점**

 ❌ 유니티 전역 스크립트를 전부 함수형으로 재작성하기🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️
 ✅ 기존 로직 중 복잡한 분기·에러 처리를 함수형 파이프라인으로 단순화하기🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## 목차

- [핵심 하이라이트](#핵심-하이라이트)
- [다른 라이브러리와의 비교](#다른-라이브러리와의-비교)
- [시작하기](#시작하기)
  - [UPM 설치 (권장)](#upm-설치-권장)
  - [수동 설치](#수동-설치)
  - [선택적 의존성](#선택적-의존성)
- [핵심 개념](#핵심-개념)
  - [`Result<T>` 사용법](#resultt-사용법)
    - [Result 생성하기](#result-생성하기)
    - [핵심 메서드: Then, Map, Filter](#핵심-메서드-then-map-filter)
    - [에러 처리 및 복구](#에러-처리-및-복구)
    - [부수 효과 (Side Effects)](#부수-효과-side-effects)
    - [조건부 실행](#조건부-실행)
    - [비동기 Result (UniTask / Awaitable)](#비동기-result-unitask--awaitable)
  - [`Option<T>` 사용법](#optiont-사용법)
    - [Option 생성하기](#option-생성하기)
    - [핵심 Option 메서드](#핵심-option-메서드)
    - [Option과 Result 변환](#option과-result-변환)
    - [Match로 분기 처리](#match로-분기-처리)
    - [컬렉션 헬퍼](#컬렉션-헬퍼)
    - [LINQ 통합](#linq-통합)
  - [`NonEmpty<T>` 사용법](#nonemptyt-사용법)
    - [NonEmpty 생성하기](#nonempty-생성하기)
    - [NonEmpty 메서드](#nonempty-메서드)
    - [사용 예시](#사용-예시)
  - [오류 코드와 진단](#오류-코드와-진단)
    - [내장 ErrorCode](#내장-errorcode)
    - [커스텀 ErrorCode](#커스텀-errorcode)
    - [ErrorCode 속성](#errorcode-속성)
    - [진단 정보 (Debug 모드)](#진단-정보-debug-모드)
- [플루언트 파이프라인](#플루언트-파이프라인)
  - [분기 제어와 복구](#분기-제어와-복구)
  - [여러 결과 결합](#여러-결과-결합)
  - [컬렉션 & 순회](#컬렉션--순회)
- [비동기 지원 (UniTask / Awaitable)](#비동기-지원-unitask--awaitable)
- [복원력 유틸리티](#복원력-유틸리티)
- [디버깅 & 가시성](#디버깅--가시성)
- [성능 툴킷](#성능-툴킷)
- [샘플 씬 & 테스트](#샘플-씬--테스트)
- [문서](#문서)
- [기여하기](#기여하기)
- [라이선스](#라이선스)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 핵심 하이라이트

- **`Result<T>`와 `Option<T>` 구조체**는 힙 할당 없이 명시적인 성공/실패 및 null 안전성을 구현합니다.
- **철도 스타일 확장 메서드**(`Then`, `Map`, `Filter`, `Recover`, `DoStrict`, `IfFailed` 등)를 통해 매우 읽기 쉬운 파이프라인을 제공합니다.
- **UniTask와 Unity Awaitable 모두 지원**하는 비동기 파이프라인(`.ThenAsync`, `.MapAsync`, `.FilterAsync`, `AsyncResult.TryAsync()`)을 제공합니다.
- **ResultCombinators와 컨렉션 확장**을 통해 여러 Result를 결합하거나 리스트/Span을 조건부 검증과 함께 순회할 수 있습니다.
- **SafeExecutor 계측**으로 Editor/디버그 환경에서 연산 타입과 호출 위치를 자동으로 기록합니다.
- **DelegateCache, ResultPool, SpanExtensions** 등 성능 중심 유틸리티로 고빈도 코드에서도 GC를 억제합니다.
- **`Assets/Scenes` 데모와 `src/UniFP/Assets/Tests` 단위 테스트**를 통해 실제 사용 패턴을 바로 확인할 수 있습니다.

## 다른 라이브러리와의 비교

### UniFP vs Unity-NOPE

**성능 비교:**
- ✅ Zero-GC 구조체 설계 (모든 핵심 타입이 스택 할당)
- ✅ 델리게이트 캐싱으로 Update 루프 최적화
- ✅ ResultPool & ListPool 내장

**기능 비교:**
- UniFP의 `Then` = NOPE의 `Bind` (C# 친화적 명명)
- 추가 기능: Retry, RetryWithBackoff, Trace, Breakpoint 등
- 고급 컬렉션 확장: SelectResults, CombineAll, Partition

**에러 타입화:**
UniFP는 `ErrorCode` 구조체를 사용하여 99%의 Unity 게임 시나리오에 최적화되어 있습니다.
복잡한 도메인 로직에서 타입 안전한 에러가 필요한 경우 커스텀 ErrorCode로 대응 가능합니다.

**➡️ 상세 비교: [라이브러리 비교 문서](./docs/library-comparison.ko.md)**

---

### UniFP vs language-ext

**왜 language-ext를 Unity에 바로 쓰지 않는가?**
- ❌ Unity 런타임 최적화 부재 (클래스 기반, GC 압박)
- ❌ 과도한 기능 복잡도 (100+ 모나드, Higher-kinded types)
- ❌ 가파른 학습 곡선 (Haskell 배경 필요)
- ✅ UniFP: Unity에 특화된 최소 집합, C# LINQ 경험만으로 시작 가능

**간단 비교:**

| 분류 | language-ext | UniFP | 
|------|--------------|-------|
| 핵심 모나드 | Option, Either, Try, Validation, Fin | Result, Option, NonEmpty |
| 성능 | 고도 추상화 오버헤드 | Zero-GC 구조체, 풀링 최적화 |
| 학습 곡선 | 가파름 (Haskell) | 완만함 (C# LINQ) |
| Unity 통합 | 제한적 | UniTask/Awaitable 네이티브 지원 |

**➡️ 상세 비교: [라이브러리 비교 문서](./docs/library-comparison.ko.md)**

---

## 시작하기

### UPM 설치 (권장)

1. Unity에서 **Window ▸ Package Manager**를 엽니다.
2. **Add package from git URL...**을 선택한 뒤 아래 주소를 입력합니다.

   ```text
   https://github.com/Nekoya-Jin/UniFP.git?path=src/UniFP/Assets/Plugins/UniFP
   ```

3. Unity가 `com.unifp.core` 패키지를 설치하고 예제 및 asmdef를 포함한 폴더를 추가합니다.

`Packages/manifest.json`을 직접 수정하려면 다음 의존성을 추가하세요.

```json
{
  "dependencies": {
    "com.unifp.core": "https://github.com/Nekoya-Jin/UniFP.git?path=src/UniFP/Assets/Plugins/UniFP"
  }
}
```

### 선택적 의존성

UniFP는 독립적으로 작동하지만, 다음 중 하나를 설치하면 비동기 기능을 향상시킬 수 있습니다:

**옵션 1: UniTask** (Unity 2022.3+ 권장)
- Unity Awaitable보다 더 많은 기능과 우수한 성능
- UPM 설치:
  ```text
  https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
  ```
- `AsyncResult.ThenAsync`, `MapAsync`, `FilterAsync`, `DoAsync`, `TryAsync` 활성화

**옵션 2: Unity Awaitable** (Unity 6.0+)
- Unity 6.0+에 내장, 별도 설치 불필요
- UniFP.asmdef의 `versionDefines`를 통해 자동 감지
- UniTask와 동일한 비동기 API 제공

**비동기 지원 없이:**
- 모든 동기 `Result<T>` 기능은 완벽하게 작동
- 비동기 확장 메서드는 사용 불가

## 핵심 개념

### `Result<T>` 사용법

`Result<T>`는 **성공**(Success) 또는 **실패**(Failure)를 타입으로 표현하여 if/else와 try/catch 지옥에서 해방시켜줍니다.

혹시 이런 코드를 본 적 있나요? if 안에 try가 있고, 그 안에 또 if-else가 있는 코드...
성공 로직, 실패 로직, 예외 처리, 기본값 할당이 스파게티처럼 얽혀있어 어디서부터 읽어야 할지 막막합니다. 새로운 검증 로직 하나를 추가하는 순간, 지옥은 점점 더 깊어지고 결국 아무도 건드리고 싶지 않은 코드가 탄생하죠.

#### 전통적인 C# 방식

```csharp
using UnityEngine;

public class LoginSample : MonoBehaviour
{
    void Start()
    {
        var userId = PlayerPrefs.GetString("userId");

        if (string.IsNullOrWhiteSpace(userId))
        {
            Debug.LogError("로그인 실패: 입력이 비었습니다");
            userId = "guest";
        }
        else
        {
            try
            {
                if (!ValidateAccount(userId))
                {
                    Debug.LogWarning("로그인 실패: 존재하지 않는 사용자");
                    userId = "guest";
                }
                else
                {
                    Debug.Log($"로그인 성공: {userId}");
                    LogUser(userId);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"로그인 중 예외 발생: {ex.Message}");
                userId = "guest";
            }
        }
    }

    bool ValidateAccount(string id) => id == "player42";

    void LogUser(string id) => Debug.Log($"인증 파이프라인이 {id}를 허용했습니다");
}
```

- 상태 체크, 예외 처리, 기본 값 복구 로직이 if/else와 try/catch에 흩어져 있어 분기가 복잡합니다.
- 실패 케이스가 늘어나면 분기 수가 기하급수적으로 증가하여 유지보수가 어려워집니다.


#### UniFP `Result<T>`으로 리팩터링

이제 UniFP로 이 문제를 해결해보죠. UniFP는 모든 분기와 예외 처리를 하나의 컨베이어 벨트 위로 올려 명시적인 성공/실패를 보여줍니다.
데이터는 성공이라는 고속도로를 따라 직진하고, 어느 한 곳에서라도 문제가 생기면 즉시 실패라는 비상 차선으로 빠져나옵니다. 코드는 위에서 아래로 물 흐르듯 읽히고, 각 단계가 무슨 일을 하는지 명확하게 보입니다.

```csharp
// 좋은 예시: 모든 단계가 명확하게 체이닝됩니다.
using UniFP;
using UnityEngine;

public class LoginSample : MonoBehaviour
{
    void Start()
    {
        var loginResult = Result.FromValue(PlayerPrefs.GetString("userId"))
            // 1. 유효한 입력인가? (아니면 InvalidInput 비상 차선으로)
            .Filter(DelegateCache.IsNotNullOrWhitespace, ErrorCode.InvalidInput)
            // 2. 계정이 존재하는가? (아니면 NotFound 비상 차선으로)
            .Then(id => ValidateAccount(id)
                ? Result<string>.Success(id)
                : Result<string>.Failure(ErrorCode.NotFound))
            // 3. (성공 고속도로를 달리는 중에만) 로그를 남긴다.
            .Do(LogUser)
            // 🚨 비상 차선으로 빠졌다면, 최종 목적지는 "guest"다.
            .Recover(_ => "guest");

        // 최종 결과에 따라 마지막 처리
        loginResult.Match(
            onSuccess: id => Debug.Log($"로그인 성공: {id}"),
            onFailure: code => Debug.LogError($"로그인 실패: {code}"));
    }

    bool ValidateAccount(string id) => id == "player42";
    void LogUser(string id) => Debug.Log($"인증 파이프라인이 {id}를 허용했습니다");
}
```

- 각 단계가 명시적으로 체이닝되어 성공/실패 흐름을 한눈에 파악할 수 있습니다.
- 실패 시 자동으로 `Recover` 분기로 이동하므로 예외와 기본 값 복구 로직이 분류됩니다.
- 추가 검증이나 비동기 호출도 `Then`, `Filter`, `ThenAsync` 등을 통해 쉽게 확장할 수 있습니다.

#### Result 생성하기

```csharp
using UniFP;

// 1. Success / Failure로 직접 생성
var success = Result<int>.Success(42);
var failure = Result<int>.Failure(ErrorCode.NotFound);
var failureWithMsg = Result<int>.Failure(ErrorCode.ValidationFailed, "나이는 0보다 커야 합니다");

// 2. FromValue로 값을 Result로 승격
var fromValue = Result.FromValue(userId);

// 3. Try로 예외를 Result로 변환
var parseResult = Result.Try(() => int.Parse(input));
var parseWithCode = Result.Try(() => int.Parse(input), ErrorCode.InvalidInput);
```

#### 핵심 메서드: Then, Map, Filter

```csharp
// Then: Result<T>를 반환하는 함수 체이닝
Result<User> LoadUser(int id) => /* ... */;
var result = Result.FromValue(42)
    .Then(LoadUser);  // int -> Result<User>

// Map: 일반 값을 반환하는 함수 변환
var doubled = Result.FromValue(10)
    .Map(x => x * 2);  // int -> int (자동으로 Result<int>로 래핑)

// Filter: 조건 검증 (조건 실패 시 Failure)
var validated = Result.FromValue(age)
    .Filter(x => x >= 18, ErrorCode.ValidationFailed, "성인만 가능합니다");
```

> **💡 Tip: Then vs Map**
> - `Then`은 Result를 반환하는 함수에 사용 (실패할 수 있는 연산)
> - `Map`은 일반 값을 반환하는 함수에 사용 (단순 변환)

#### 에러 처리 및 복구

```csharp
// Recover: 실패를 기본값으로 복구
var withDefault = LoadConfig()
    .Recover(code => DefaultConfig);

// IfFailed: 실패 시 대체 파이프라인 실행
var cached = LoadFromServer()
    .IfFailed(() => LoadFromCache());

// Catch: 특정 에러만 가로채서 복구
var result = LoadResource()
    .Catch(ErrorCode.NotFound, () => CreateDefault());

// Match: 성공/실패에 따라 다른 처리
result.Match(
    onSuccess: user => Debug.Log($"환영합니다, {user.Name}"),
    onFailure: code => Debug.LogError($"로드 실패: {code}"));
```

#### 부수 효과 (Side Effects)

```csharp
// Do: 성공 시만 부수 효과 실행 (실패 시 스킵)
var result = LoadUser(id)
    .Do(user => Analytics.Track("UserLoaded", user.Id))
    .Do(user => Debug.Log($"로드됨: {user.Name}"));

// DoStrict: 부수 효과가 실패하면 파이프라인 중단
var saved = CreateUser(data)
    .DoStrict(user => SaveToDatabase(user));  // DB 저장 실패 시 전체 실패

// IfFailed: 실패 시만 부수 효과 실행
var result = Process()
    .IfFailed(code => Debug.LogError($"처리 실패: {code}"));
```

#### 조건부 실행

```csharp
// ThenIf / MapIf: 조건에 따라 선택적으로 변환
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

#### 비동기 Result (UniTask / Awaitable)

```csharp
using Cysharp.Threading.Tasks;  // 또는 using UnityEngine; (Awaitable)

// ThenAsync: 비동기 Result 체이닝
async UniTask<Result<User>> LoadUserAsync(int id)
{
    return await Result.FromValue(id)
        .Filter(x => x > 0, ErrorCode.InvalidInput)
        .ThenAsync(async id => await FetchFromAPI(id))
        .MapAsync(json => ParseUser(json))
        .FilterAsync(user => UniTask.FromResult(user.IsActive), "비활성 사용자");
}

// TryAsync: 예외를 던지는 비동기 작업을 Result로 변환
var result = await AsyncResult.TryAsync(async () => 
{
    var response = await httpClient.GetAsync(url);
    return await response.Content.ReadAsStringAsync();
}, ErrorCode.NetworkError);

// DoAsync: 비동기 부수 효과
var saved = await LoadUser(id)
    .DoAsync(async user => await SaveToCloud(user));
```

---

### `Option<T>` 사용법

`Option<T>`는 **값이 있음**(Some) 또는 **값이 없음**(None)을 타입으로 표현하여 null 지옥에서 해방시켜줍니다. 

Unity 프로젝트를 하다 보면 `null` 체크만 수십 줄인 코드와 마주칠 때가 있습니다.
`if (foo == null)` → `else if (foo.Bar == null)` → `else if (foo.Bar.Length == 0)` …
에러 로그는 여기저기서 튀어나오고, 어느 분기에서 `NullReferenceException`이 터졌는지 찾느라 시간을 낭비하게 되죠.

#### 전통적인 C# 방식

```csharp
public class UserProfileLoader
{
    public void Load()
    {
        var raw = PlayerPrefs.GetString("profile");

        if (string.IsNullOrEmpty(raw))
        {
            Debug.LogWarning("프로필 없음: 기본값으로 대체");
            ApplyDefaults();
            return;
        }

        var profile = JsonUtility.FromJson<UserProfile>(raw);
        if (profile == null || profile.Items == null || profile.Items.Length == 0)
        {
            Debug.LogError("프로필 손상: 복구 시도");
            ApplyDefaults();
            return;
        }

        Debug.Log($"프로필 로드 성공: {profile.Name}");
        Apply(profile);
    }
}
```

- `null` 대비 로직이 곳곳에 흩어져 있어 핵심 흐름이 잘 보이지 않습니다.
- 추가 조건이 붙을수록 `if` 블록이 늘어나며, 실수로 한 단계를 빼먹으면 즉시 예외가 발생합니다.
- `if`의 들여쓰기로 인해 어디가 결국 핵심적인 로직의 흐름이며 눌의 처리인지 한눈에 보기가 어렵습니다.

#### UniFP의 `Option<T>`로 리팩터링

`Option<T>`는 값이 있으면 `Some`, 없으면 `None`으로 표현합니다. `None`인 경우 이후 파이프라인은 자동으로 스킵되므로 널 체크가 자연스럽게 정리됩니다.

```csharp
using UniFP;

public class UserProfileLoader
{
    public void Load()
    {
        var profileOption = Option<string>.From(PlayerPrefs.GetString("profile"))
            // 1. 비어있는 문자열이면 None 처리
            .Filter(DelegateCache.IsNotNullOrWhitespace)
            // 2. Json 파싱 결과를 Option으로 승격
            .Map(raw => JsonUtility.FromJson<UserProfile>(raw))
            .Filter(result => result is { Items: { Length: > 0 } });

        profileOption.Match(
            onSome: Apply,
            onNone: ApplyDefaults);
    }
}
```

- 파이프라인을 따라 값을 흘려보내면서 어느 단계에서 실패했는지 금방 눈에 띕니다.
- `Filter` 조건을 추가하는 것만으로 새로운 검증 로직을 안전하게 덧붙일 수 있습니다.
- 마지막에 `Match` 한 번이면 정상/기본 흐름이 또렷하게 분리됩니다.

#### Option 생성하기

```csharp
using UniFP;

// 1. Some / None으로 직접 생성
var some = Option<int>.Some(42);
var none = Option<int>.None();

// 2. From으로 nullable 값을 Option으로 변환 (null이면 None)
var fromValue = Option<string>.From(PlayerPrefs.GetString("username"));  // null이면 None
var fromNullable = Option<int>.From(nullableInt);

// 3. Where로 조건부 변환 (조건 실패 시 None)
var adult = Option<int>.From(age)
    .Where(x => x >= 18);
```

#### 핵심 Option 메서드

```csharp
// Map: 값 변환 (None이면 스킵)
var doubled = Option<int>.Some(10)
    .Map(x => x * 2);  // Some(20)

var stillNone = Option<int>.None()
    .Map(x => x * 2);  // None

// Bind: Option을 반환하는 함수 체이닝
Option<User> FindUser(string name) => /* ... */;
var user = Option<string>.From(username)
    .Bind(FindUser);

// Filter: 조건 검증 (실패 시 None)
var valid = Option<int>.From(input)
    .Filter(x => x > 0)
    .Filter(x => x < 100);

// Or / OrElse: None일 때 대체값 제공
var withDefault = Option<string>.None()
    .Or(Option<string>.Some("기본값"));

var fromFunc = Option<int>.None()
    .OrElse(() => Option<int>.Some(GetDefaultValue()));

// GetValueOrDefault: Option에서 값 추출
var value = someOption.GetValueOrDefault(defaultValue);
var valueOrNull = someOption.GetValueOrDefault();
```

#### Option과 Result 변환

```csharp
// Option -> Result: None을 에러로 변환
var result = Option<User>.From(FindUser(id))
    .ToResult(ErrorCode.NotFound, "사용자를 찾을 수 없습니다");

// Result -> Option: 실패를 None으로 변환 (에러 무시)
var option = LoadConfig()
    .ToOption();  // 성공 -> Some, 실패 -> None
```

#### Match로 분기 처리

```csharp
// Match: Some/None에 따라 다른 처리
var message = Option<User>.From(user).Match(
    onSome: u => $"환영합니다, {u.Name}",
    onNone: () => "게스트 모드");

// IfSome / IfNone: 한쪽 케이스만 처리
Option<Config>.From(config)
    .IfSome(c => ApplyConfig(c))
    .IfNone(() => UseDefaults());
```

#### 컬렉션 헬퍼

```csharp
using System.Linq;

var items = new[] { 1, 2, 3, 4, 5 };

// TryFirst / TryLast: 첫/마지막 요소를 Option으로
var first = items.TryFirst();  // Some(1)
var firstEven = items.TryFirst(x => x % 2 == 0);  // Some(2)
var empty = Array.Empty<int>().TryFirst();  // None

// TryFind: 조건에 맞는 요소 찾기
var found = items.TryFind(x => x > 3);  // Some(4)

// Choose: Option 컬렉션에서 Some만 추출
var options = new[] 
{ 
    Option<int>.Some(1), 
    Option<int>.None(), 
    Option<int>.Some(3) 
};
var values = options.Choose();  // [1, 3]
```

#### LINQ 통합

```csharp
using System.Linq;

// Select: Map과 동일
var doubled = Option<int>.Some(10)
    .Select(x => x * 2);  // Some(20)

// Where: Filter와 동일
var filtered = Option<int>.Some(42)
    .Where(x => x > 18);  // Some(42)

// SelectMany: Bind와 동일 (LINQ 쿼리 구문 지원)
var result = 
    from name in Option<string>.From(username)
    from user in FindUser(name)
    from profile in LoadProfile(user.Id)
    select profile;
```

---

### `NonEmpty<T>` 사용법

`NonEmpty<T>`는 **최소 1개의 요소**를 보장하는 컬렉션입니다. 파티 구성, 필수 슬롯 등 비어있으면 안 되는 도메인에 적합합니다.

#### NonEmpty 생성하기

```csharp
using UniFP;

// Create: 최소 1개 요소로 생성
var squad = NonEmpty.Create("Leader", "Support", "Tank");
var single = NonEmpty.Create(42);

// FromList: 리스트에서 변환 (비어있으면 실패)
var list = new List<string> { "A", "B", "C" };
var nonEmpty = NonEmpty.FromList(list);  // Result<NonEmpty<string>>

var emptyList = new List<string>();
var failed = NonEmpty.FromList(emptyList);  // Failure (비어있음)
```

#### NonEmpty 메서드

```csharp
// Head / Tail: 첫 요소와 나머지
var squad = NonEmpty.Create("Leader", "Tank", "Healer");
var leader = squad.Head;  // "Leader" (항상 존재)
var others = squad.Tail;  // ["Tank", "Healer"] (IEnumerable)

// Map: 모든 요소 변환
var upper = squad.Map(role => role.ToUpper());  // NonEmpty<string>

// Append / Prepend: 요소 추가
var expanded = squad.Append("Mage");  // NonEmpty (여전히 최소 1개)
var withNewLeader = squad.Prepend("NewLeader");

// ToList / ToArray: 일반 컬렉션으로 변환
var list = squad.ToList();
var array = squad.ToArray();
```

#### 사용 예시

```csharp
// 파티 시스템: 최소 1명의 리더 필수
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
        // 컴파일 타임에 최소 1명 보장
        _members.Map(p => p.ApplyBuff());
    }
}

// 설정: 최소 1개의 서버 주소 필수
var servers = NonEmpty.Create(
    "https://primary.server.com",
    "https://backup1.server.com",
    "https://backup2.server.com"
);

var primary = servers.Head;
var fallbacks = servers.Tail;
```

---

### 오류 코드와 진단

UniFP는 `ErrorCode` 구조체로 **Zero-GC 에러 분류**를 제공합니다.

#### 내장 ErrorCode

```csharp
// 0-999: UniFP 예약 범위
ErrorCode.None              // 0: 에러 없음
ErrorCode.Unknown           // 1: 알 수 없는 에러
ErrorCode.InvalidInput      // 100: 잘못된 입력
ErrorCode.ValidationFailed  // 101: 검증 실패
ErrorCode.NotFound          // 102: 찾을 수 없음
ErrorCode.Unauthorized      // 103: 인증 필요
ErrorCode.OperationFailed   // 104: 작업 실패
ErrorCode.Timeout           // 105: 시간 초과
ErrorCode.NetworkError      // 106: 네트워크 오류
ErrorCode.Forbidden         // 107: 권한 없음
ErrorCode.InvalidOperation  // 108: 유효하지 않은 작업
```

#### 커스텀 ErrorCode

```csharp
// 1000+: 사용자 정의 에러 코드
public static class GameErrors
{
    public static readonly ErrorCode InsufficientGold = 
        ErrorCode.Custom(1001, "Economy");
    
    public static readonly ErrorCode InventoryFull = 
        ErrorCode.Custom(1002, "Inventory");
    
    public static readonly ErrorCode QuestNotAvailable = 
        ErrorCode.Custom(1003, "Quest");
}

// 사용 예시
var result = PurchaseItem(itemId, price)
    .Filter(success => player.Gold >= price, GameErrors.InsufficientGold, 
            $"골드 부족: {price - player.Gold} 더 필요");
```

#### ErrorCode 속성

```csharp
var error = ErrorCode.NotFound;

error.Code;       // 102
error.Category;   // "Resource"
error.IsCustom;   // false (내장 코드)

var custom = ErrorCode.Custom(2001, "Payment");
custom.Code;      // 2001
custom.Category;  // "Payment"
custom.IsCustom;  // true
```

#### 진단 정보 (Debug 모드)

```csharp
// Editor 또는 UNIFP_DEBUG 환경에서 자동 기록
var result = LoadAsset(path)
    .Filter(asset => asset != null, ErrorCode.NotFound);

if (result.IsFailure)
{
    // 실패 시 자동으로 기록된 정보
    Debug.LogError($"[{result.ErrorCode.Category}] {result.Error}");
    Debug.LogError($"발생 위치: {result.FilePath}:{result.LineNumber}");
    Debug.LogError($"메서드: {result.MemberName}");
    Debug.LogError($"연산 타입: {result.OperationType}");
    
    // 출력 예시:
    // [Resource] Asset not found: player_model.prefab
    // 발생 위치: Assets/Scripts/AssetLoader.cs:42
    // 메서드: LoadPlayerModel
    // 연산 타입: Filter
}
```

---

## 플루언트 파이프라인
```

---

## 플루언트 파이프라인

`UniFP` 네임스페이스를 import 하면 모든 확장 메서드를 활용할 수 있습니다. 레일웨이 패턴으로 성공 경로와 실패 경로를 깔끔하게 분리합니다.

```csharp
var pipeline = Result.FromValue(request)
    .Filter(req => req.IsValid, ErrorCode.ValidationFailed)
    .Then(Persist)
    .DoStrict(SendAnalyticsEvent)
    .IfFailed(() => LoadCachedResult())
    .Trace("Purchase");
```

### 분기 제어와 복구

- `Recover(Func<ErrorCode, T>)`는 실패를 기본 값으로 대체합니다.
- `IfFailed(Func<Result<T>>)`는 대체 파이프라인을 실행합니다.
- `ThenIf`, `MapIf`는 조건부로 추가 작업을 수행합니다.
- `DoStrict`는 실패를 전파해야 하는 부수 효과(예: 분석 이벤트, 데이터베이스 기록)에 적합합니다.

### 여러 결과 결합

`ResultCombinators`로 독립적인 연산을 하나의 결과로 묶을 수 있습니다.

```csharp
var stats = ResultCombinators.Combine(
    LoadLevelProgress(),
    LoadInventory());

var snapshot = stats.Zip(
    CalculateRewards(),
    (progress, inventory, rewards) => new PlayerSnapshot(progress, inventory, rewards));
```

### 컬렉션 & 순회

- `SelectResults`는 컬렉션을 순회하며 실패 시 즉시 중단합니다.
- `CombineAll`은 여러 `Result<T>`를 `Result<List<T>>`로 모읍니다.
- `FilterResults`, `Partition`, `Fold`, `AggregateResults` 등으로 리스트 검증과 집계를 수행합니다.
- `SpanExtensions`는 `Span<T>` 기반 연산으로 Burst 민감 코드에서도 추가 할당 없이 동작합니다.

## 비동기 지원 (UniTask / Awaitable)

UniFP는 **UniTask**(권장) 및 **Unity Awaitable**(Unity 6.0+) 모두에서 비동기 작업을 지원합니다.

**UniTask 설치 시:**
```csharp
using Cysharp.Threading.Tasks;

async UniTask<Result<PlayerData>> FetchPlayer(int id)
{
    return await Result.TryFromResult(() => ValidateId(id))
        .ThenAsync(async _ => await Api.GetPlayer(id))
        .MapAsync(payload => payload.ToPlayerData())
        .FilterAsync(data => UniTask.FromResult(data.IsActive), "활성화된 플레이어가 아닙니다");
}
```

**Unity 6.0+ (Awaitable) 사용 시:**
```csharp
using UnityEngine;

async Awaitable<Result<PlayerData>> FetchPlayer(int id)
{
    return await Result.TryFromResult(() => ValidateId(id))
        .ThenAsync(async _ => await Api.GetPlayer(id))
        .MapAsync(payload => payload.ToPlayerData())
        .FilterAsync(data => Awaitable.FromResult(data.IsActive), "활성화된 플레이어가 아닙니다");
}
```

두 방식 모두 동일한 API를 제공합니다 - 비동기 타입만 바꾸면 됩니다!
}

var cached = await FetchPlayer(42).DoAsync(data => Cache.Save(data));
```

예외를 던지는 비동기 작업은 `AsyncResult.TryAsync`로 감싸면 자동으로 `Result` 실패로 변환됩니다.

## 복원력 유틸리티

- `Retry`, `RetryAsync`는 지정된 횟수만큼 재시도를 수행합니다.
- `RetryWithBackoff`는 지수 백오프 지연을 적용해 불안정한 서비스를 다룹니다.
- `Repeat`, `RepeatAsync`는 N번 연속 성공해야 하는 시나리오를 처리합니다.
- `Catch`는 특정 실패 메시지를 가로채 대체 로직을 실행합니다.

```csharp
var response = await RetryExtensions.RetryWithBackoff(
    () => Api.SendAsync(payload),
    maxAttempts: 5,
    initialDelayMilliseconds: 200,
    backoffMultiplier: 2.5f);
```

## 디버깅 & 가시성

- `SafeExecutor`는 각 연산의 위치와 종류를 기록합니다.
- `PipelineDebug.Trace`, `TraceWith`, `TraceOnFailure`, `Breakpoint`로 파이프라인 상태를 콘솔에서 추적합니다.
- `OperationType` 열거형을 통해 어떤 단계(`Map`, `Filter`, `Then` 등)에서 실패했는지 즉시 확인할 수 있습니다.

```csharp
var result = LoadConfig()
    .Trace("Config")
    .Assert(cfg => cfg.Version >= 2, "Config version too old")
    .Breakpoint();
```

## 성능 툴킷

기본적인 사용에선 큰 문제는 없지만, Update구문처럼 매 프레임마다 실행되야하는 로직이라면 아래의 최적화가 필요합니다.

- **DelegateCache**: 자주 사용하는 람다를 정적 캐시로 재사용합니다.
- **ResultPool & ListPool<T>**: 결과 컬렉션을 풀링하여 고빈도 루프에서 GC를 제거합니다.
- **SpanExtensions**: 스택 또는 풀 버퍼 기반 변환을 제공합니다.
- **Zero-allocation 오류 흐름**: `ErrorCode`, `OperationType`, 구조체 모나드로 힙 사용량을 억제합니다.

## 샘플 씬 & 테스트

- `Assets/Scenes`
  - `01_BasicResultExample` — Result 기본기
  - `02_PipelineExample` — 체이닝 패턴
  - `04_AsyncExample` — UniTask 연계 비동기 흐름
  - `06_PerformanceExample` — 제로 할당 기법
  - `10_RealWorld_UserLogin` — 견고한 로그인 파이프라인
  - `11_RealWorld_ItemPurchase` — 서비스 간 레일웨이 처리
- 테스트는 `src/UniFP/Assets/Tests`에 위치하며 검증, 비동기 실패, 재시도 시나리오 등 주요 엣지 케이스를 다룹니다.

리포지토리 루트에서 테스트를 실행하려면 다음 명령어를 사용하세요.

```bash
dotnet test src/UniFP/UniFP.Tests.csproj
```

## 문서

확장 가이드는 [`docs`](./docs) 폴더에서 확인할 수 있습니다.

- [빠르게 시작하기](./docs/getting-started.md)
- [API 레퍼런스](./docs/api-reference.md)
- [예제 모음](./docs/examples.md)
- [베스트 프랙티스](./docs/best-practices.md)

## 기여하기

이슈 등록과 풀 리퀘스트는 언제나 환영합니다. 변경 사항 제출 전에는 테스트와 Example도 작성해주세요

## 라이선스

UniFP는 [MIT License](./LICENSE)를 따릅니다.
