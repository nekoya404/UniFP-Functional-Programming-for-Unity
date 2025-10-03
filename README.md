# UniFP — Unity 함수형 파이프라인

> **UniFP의 목표**
>
> ❌ 유니티 전역 스크립트를 전부 함수형으로 재작성하기🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️
>
> ✅ 기존 로직 중 복잡한 분기·에러 처리를 함수형 파이프라인으로 단순화하기🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️

[![Unity](https://img.shields.io/badge/Unity-2020.3%2B-000?logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-success.svg)](./LICENSE)
[![Release](https://img.shields.io/badge/version-v1.0.0-blue)](https://github.com/Nekoya-Jin/UniFP/releases)

UniFP는 Unity 게임 로직에 함수형 사고방식과 명시적 에러 처리를 도입하는 GC제로 C# 함수형 프로그래밍 프레임워크입니다.

기존 C# 함수형 라이브러리(예: [language-ext](https://github.com/louthy/language-ext))는 범용 .NET 환경을 겨냥해 방대한 기능과 복잡한 추상화를 포함하고 있으며, 그만큼 학습 곡선이 가파르고 구조체를 활용하지 않는 경우가 많아 Unity 런타임에서 GC 할당과 성능 손실이 발생하기 쉽습니다. 이러한 무거운 의존성 없이도 게임 플레이 코드에서 안전한 오류 처리와 선언적 파이프라인을 활용할 수 있도록, 실시간 애플리케이션에 최적화된 경량 대안을 목표로 UniFP를 개발했습니다.

 `Result<T>`와 `Option<T>`를 기반으로 한 파이프라인 확장을 제공하여 예외 대신 타입 안전한 흐름 제어를 구현하면서도 GC 부담은 최소화합니다.

> 모든 핵심 타입은 구조체로 제공되며, Editor 또는 `UNIFP_DEBUG` 환경에서는 각 연산이 발생한 파일/라인/메서드 정보를 자동으로 기록합니다. 별도의 설정 없이 Unity 프로젝트에 바로 붙여 사용할 수 있습니다.

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## 목차

- [핵심 하이라이트](#핵심-하이라이트)
- [시작하기](#시작하기)
  - [UPM 설치 (권장)](#upm-설치-권장)
  - [수동 설치](#수동-설치)
- [빠른 둘러보기](#빠른-둘러보기)
- [핵심 개념](#핵심-개념)
  - [`Result<T>` — 명시적인 성공/실패](#resultt--명시적인-성공실패)
  - [`Option<T>` — Null 안전](#optiont--null-안전)
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
- [감사의-말](#감사의-말)

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

## 빠른 둘러보기

### 전통적인 C# 방식

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

### UniFP 파이프라인으로 리팩터링

```csharp
using UniFP;
using UnityEngine;

public class LoginSample : MonoBehaviour
{
    void Start()
    {
        var login = Result.FromValue(PlayerPrefs.GetString("userId"))
            .Filter(DelegateCache.IsNotNullOrWhitespace, ErrorCode.InvalidInput)
            .Then(id => ValidateAccount(id)
                ? Result<string>.Success(id)
                : Result<string>.Failure(ErrorCode.NotFound))
            .Do(LogUser)
            .Recover(_ => "guest");

        login.Match(
            onSuccess: id => Debug.Log($"로그인 성공: {id}"),
            onFailure: code => Debug.LogError($"로그인 실패: {code}"));
    }

    void LogUser(string id) => Debug.Log($"인증 파이프라인이 {id}를 허용했습니다");
}
```

- 각 단계가 명시적으로 체이닝되어 성공/실패 흐름을 한눈에 파악할 수 있습니다.
- 실패 시 자동으로 `Recover` 분기로 이동하므로 예외와 기본 값 복구 로직이 분류됩니다.
- 추가 검증이나 비동기 호출도 `Then`, `Filter`, `ThenAsync` 등을 통해 쉽게 확장할 수 있습니다.

## 핵심 개념

### `Result<T>` — 명시적인 성공/실패

`Result<T>`는 `IsSuccess`, `Value`, `ErrorCode`, `ErrorMessage`를 갖는 불변 구조체입니다. 정적 팩토리를 사용하면 추가 할당 없이 결과를 표현할 수 있습니다.

```csharp
var ok = Result<int>.Success(120);
var error = Result.FromError<int>(ErrorCode.InvalidInput);

ok.Match(
    onSuccess: score => Debug.Log(score),
    onFailure: code => Debug.LogError(code));

var fallback = error.GetValueOrDefault(-1); // 실패 시 -1 반환
```

### `Option<T>` — Null 안전

Null 가능 상태를 `Some`/`None`으로 명확하게 모델링합니다.

```csharp
var maybeUser = Option<string>.From(PlayerPrefs.GetString("user"));

maybeUser.Match(
    onSome: id => Debug.Log($"사용자 {id} 로드"),
    onNone: () => Debug.LogWarning("세션 정보 없음"));

var result = maybeUser.ToResult("사용자를 찾을 수 없습니다");
```

### 오류 코드와 진단

`ErrorCode` 열거형은 문자열 대신 숫자 기반 오류를 제공해 GC를 줄입니다. Editor 또는 `UNIFP_DEBUG`에서는 `SafeExecutor`가 연산 타입(`OperationType`)과 호출 위치를 함께 기록하여 디버깅을 돕습니다.

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

- **DelegateCache**: 자주 사용하는 람다를 정적 캐시로 재사용합니다.
- **ResultPool & ListPool<T>**: 결과 컬렉션을 풀링하여 고빈도 루프에서 GC를 제거합니다.
- **SpanExtensions**: 스택 또는 풀 버퍼 기반 변환을 제공합니다.
- **Zero-allocation 오류 흐름**: `ErrorCode`, `OperationType`, 구조체 모나드로 힙 사용량을 억제합니다.

이 유틸리티들은 인벤토리 처리, 전투 판정, 상점 구매 같은 예제 씬에서 실제로 구동되며, GC 할당 없이 안정적으로 동작합니다.

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
