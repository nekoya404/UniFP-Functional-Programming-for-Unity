![C# Functional Programming for Unity Capsule Header](https://capsule-render.vercel.app/api?type=waving&height=220&color=0:5A2BFF,100:1FB5E9&text=C%23%20Functional%20Programming%20for%20Unity&fontAlign=50&fontAlignY=40&fontSize=46&fontColor=FFFFFF&desc=UniFP&descAlign=50&descAlignY=65&descSize=24)

[English](./README.md) · [한국어](./README.ko.md) · [简体中文](./README.zh-CN.md)

# UniFP — C# Functional Programming for Unity

[![Unity](https://img.shields.io/badge/Unity-2020.3%2B-000?logo=unity)](https://unity.com/)
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
- [시작하기](#시작하기)
  - [UPM 설치 (권장)](#upm-설치-권장)
  - [수동 설치](#수동-설치)
  - [의존성](#의존성)
- [핵심 개념](#핵심-개념)
  - [`Result<T>` — **if/else와 try/catch 지옥**에서 해방 🔥🔥🔥](#resultt--ifelse와-trycatch-지옥에서-해방-)
  - [`Option<T>` — **Null지옥**에서 해방 🔥🔥🔥](#optiont--null지옥에서-해방-)
  - [오류 코드와 진단](#오류-코드와-진단)
  - [`NonEmpty<T>` — 최소 1개 보장 컬렉션](#nonemptyt--최소-1개-보장-컬렉션)
- [플루언트 파이프라인](#플루언트-파이프라인)
  - [분기 제어와 복구](#분기-제어와-복구)
  - [여러 결과 결합](#여러-결과-결합)
  - [컬렉션 & 순회](#컬렉션--순회)
- [비동기 & UniTask 통합](#비동기--unitask-통합)
- [회복력 유틸리티](#회복력-유틸리티)
- [디버깅 & 가시성](#디버깅--가시성)
- [성능 툴킷](#성능-툴킷)
- [샘플 씬 & 테스트](#샘플-씬--테스트)
- [문서](#문서)
- [기여하기](#기여하기)
- [라이선스](#라이선스)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 핵심 하이라이트

- **`Result<T>` · `Option<T>` 구조체**로 힙 할당 없이 명시적 성공/실패, Null 안전을 구현합니다.
- **레일웨이 스타일 확장 메서드** (`Then`, `Map`, `Filter`, `Recover`, `DoStrict`, `IfFailed` 등) 가독성 높은 파이프라인을 제공합니다.
- **UniTask 기반 비동기 파이프라인**을 위한 `.ThenAsync`, `.MapAsync`, `.FilterAsync`, `AsyncResult.TryAsync()` 유틸리티를 제공합니다.
- **ResultCombinators · 컬렉션 확장**으로 여러 결과를 결합하거나 리스트/Span을 순회하며 조건 검증을 수행할 수 있습니다.
- **SafeExecutor 계측**으로 Editor/디버그 환경에서 연산 타입과 호출 위치를 자동으로 기록합니다.
- **DelegateCache, ResultPool, SpanExtensions** 등 성능 중심 유틸리티로 고빈도 코드에서도 GC를 억제합니다.
- **`Assets/Scenes` 데모와 `src/UniFP/Assets/Tests` 단위 테스트**를 통해 실제 사용 패턴을 바로 확인할 수 있습니다.

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

### 수동 설치

`src/UniFP/Assets/Plugins/UniFP` 디렉터리를 프로젝트의 `Assets/Plugins/UniFP` 아래로 복사합니다. `UniFP.asmdef`를 포함해야 Unity 빌드 타임이 빠르게 유지됩니다.

### 의존성

UniFP는 **UniTask**를 필요로 합니다. UPM 설치 시 자동으로 설치되지만, 수동 설치 시에는 별도로 설치해야 합니다:

```text
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
```

## 핵심 개념

### `Result<T>` — **if/else와 try/catch 지옥**에서 해방 🔥🔥🔥 

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


### `Option<T>` — **Null지옥**에서 해방 🔥🔥🔥 

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

### `NonEmpty<T>` — 최소 1개 보장 컬렉션

비어 있을 수 없는 컬렉션이 필요할 때 사용합니다. 파티 구성, 필수 슬롯과 같은 도메인에 적합합니다.

```csharp
var squad = NonEmpty.Create("Leader", "Support", "Tank");
var upper = squad.Map(role => role.ToUpperInvariant());
```

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

## 비동기 & UniTask 통합

UniTask와 Result 파이프라인을 자연스럽게 결합합니다.

```csharp
async UniTask<Result<PlayerData>> FetchPlayer(int id)
{
    return await Result.TryFromResult(() => ValidateId(id))
        .ThenAsync(async _ => await Api.GetPlayer(id))
        .MapAsync(payload => payload.ToPlayerData())
        .FilterAsync(data => UniTask.FromResult(data.IsActive), "활성화된 플레이어가 아닙니다");
}

var cached = await FetchPlayer(42).DoAsync(data => Cache.Save(data));
```

예외를 던지는 비동기 작업은 `AsyncResult.TryAsync`로 감싸면 자동으로 `Result` 실패로 변환됩니다.

## 회복력 유틸리티

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
