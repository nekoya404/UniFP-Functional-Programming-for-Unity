# UniFP (Unity Functional Programming)

![Unity](https://img.shields.io/badge/Unity-2020.3%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)

**UniFP**는 Unity를 위한 함수형 프로그래밍 라이브러리입니다. Result 모나드와 파이프라인 패턴을 통해 안전하고 우아한 에러 처리를 제공합니다.

## ✨ 특징

### Core Features
- **🔒 Result<T> Monad**: 예외 대신 명시적 에러 처리
- **🔗 Pipeline Pattern**: 함수형 스타일의 깔끔한 코드 체이닝
- **⚡ Zero Allocation**: 구조체 기반 설계로 GC 부담 최소화
- **📦 Pure C#**: 외부 의존성 없음 (UniTask 제외)
- **🎮 Unity Optimized**: Unity 워크플로우에 완벽히 통합

### Advanced Features (v1.1+)
- **🎯 Option<T> Monad**: null 안전한 값 표현
- **⏱️ AsyncResult**: UniTask 기반 비동기 지원
- **🔀 Combinators**: 여러 Result 합성 (Combine, Zip)
- **↔️ Either<L,R>**: 두 가지 타입 중 하나 표현
- **🔄 Retry Utilities**: 재시도 및 백오프 전략
- **📚 Collection Extensions**: Traverse, Sequence, Fold 등
- **🛤️ Railway-Oriented Programming**: 복잡한 분기 처리

## 🚀 시작하기

### 설치

#### UPM Package (Git URL)

Package Manager에서 다음 URL을 추가:

```
https://github.com/YOUR_USERNAME/UniFP.git?path=src/UniFP/Assets/Plugins/UniFP
```

또는 `Packages/manifest.json`에 직접 추가:

```json
{
  "dependencies": {
    "com.yourcompany.unifp": "https://github.com/YOUR_USERNAME/UniFP.git?path=src/UniFP/Assets/Plugins/UniFP"
  }
}
```

#### 수동 설치

`src/UniFP/Assets/Plugins/UniFP` 폴더를 프로젝트의 `Assets/Plugins/UniFP`로 복사

### 기본 사용법

```csharp
using UniFP;
using UnityEngine;

public class Example : MonoBehaviour
{
    void Start()
    {
        // 파이프라인으로 에러를 자동 전파
        var result = Pipe.Start(CheckFileSize("data.txt"))
            .Bind(LoadFile)        
            .Bind(ProcessData);

        // 결과 처리
        if (result.IsSuccess)
        {
            Debug.Log($"성공: {result.Value}");
        }
        else
        {
            Debug.LogError($"실패: {result.Error}");
        }
    }

    Result<string> CheckFileSize(string path)
    {
        return path.Length > 100 
            ? Result<string>.Failure("파일 경로가 너무 깁니다")
            : Result<string>.Success(path);
    }

    Result<byte[]> LoadFile(string path)
    {
        // 파일 로드 로직...
        return Result<byte[]>.Success(new byte[10]);
    }

    Result<string> ProcessData(byte[] data)
    {
        // 데이터 처리 로직...
        return Result<string>.Success("처리 완료");
    }
}
```

## 📚 핵심 개념

### Result 모나드

`Result<T>`는 성공 또는 실패를 나타내는 불변 타입입니다:

```csharp
// 성공 케이스
var success = Result<int>.Success(42);

// 실패 케이스
var failure = Result<int>.Failure("에러 메시지");

// 상태 확인
if (result.IsSuccess)
    Console.WriteLine(result.Value);
else
    Console.WriteLine(result.Error);
```

### Pipe 체이닝

파이프라인으로 여러 연산을 체이닝:

```csharp
var result = Pipe.Start(initialValue)
    .Bind(Step1)      // Result<T> 반환
    .Map(Step2)       // T 반환 (자동으로 Result로 래핑)
    .Filter(x => x > 0, "0보다 커야 합니다")
    .Do(x => Debug.Log(x))  // 부수 효과 (로깅 등)
    .Bind(Step3);
```

### 주요 메서드

#### Bind
Result를 반환하는 함수로 체이닝 (에러 자동 전파):
```csharp
result.Bind(x => DoSomething(x))
```

#### Map
값을 변환 (자동으로 Result로 래핑):
```csharp
result.Map(x => x * 2)
```

#### Match
성공/실패 케이스 처리:
```csharp
result.Match(
    onSuccess: value => Debug.Log(value),
    onFailure: error => Debug.LogError(error)
);
```

#### Do
부수 효과 실행 (디버깅/로깅):
```csharp
result.Do(x => Debug.Log($"중간 값: {x}"))
```

#### Filter
조건 검증:
```csharp
result.Filter(x => x > 0, "양수여야 합니다")
```

#### Recover
에러 복구:
```csharp
result.Recover(error => defaultValue)
```

## 🎯 실전 예제

### 에셋 로딩 파이프라인

```csharp
public void LoadAsset(string path)
{
    var result = Pipe.Start(ValidatePath(path))
        .Do(p => Debug.Log($"경로 검증 완료: {p}"))
        .Bind(LoadFromDisk)
        .Filter(data => data.Length > 0, "빈 파일입니다")
        .Bind(Decompress)
        .Map(data => ParseAsset(data))
        .Do(asset => Debug.Log($"에셋 로드 완료: {asset.name}"));

    result.Match(
        onSuccess: asset => UseAsset(asset),
        onFailure: error => ShowError(error)
    );
}
```

### 네트워크 요청 처리

```csharp
public async UniTask<Result<UserData>> FetchUserData(int userId)
{
    return await Pipe.Try(() => 
        {
            // API 호출
            return apiClient.GetUser(userId);
        })
        .Bind(ValidateResponse)
        .Map(response => response.ToUserData())
        .Filter(user => user.IsActive, "비활성 사용자입니다");
}
```

## 🔧 고급 기능

### Try 래퍼

예외를 Result로 변환:

```csharp
var result = Pipe.Try(() => 
{
    // 예외가 발생할 수 있는 코드
    return JsonUtility.FromJson<Data>(json);
});
```

### 복잡한 파이프라인

```csharp
var result = Pipe.Start(input)
    .Bind(Validate)
    .DoOnError(e => LogError(e))      // 에러 로깅
    .Recover(e => GetDefaultValue()) // 에러 복구
    .Bind(Transform)
    .Filter(IsValid, "검증 실패")
    .Map(Finalize);
```

## 🎨 디자인 철학

1. **명시적 에러 처리**: 예외 대신 Result로 에러를 명시적으로 표현
2. **불변성**: 모든 연산은 새로운 Result를 반환
3. **체이닝**: 함수형 파이프라인으로 가독성 향상
4. **Zero Allocation**: 구조체 기반으로 GC 부담 최소화
5. **타입 안정성**: 컴파일 타임에 에러 캐치

## 📖 문서

자세한 문서는 [docs](./docs) 폴더를 참조하세요:

- [시작 가이드](./docs/getting-started.md)
- [API 레퍼런스](./docs/api-reference.md)
- [예제 모음](./docs/examples.md)
- [베스트 프랙티스](./docs/best-practices.md)

## 🤝 기여

기여는 언제나 환영합니다! 이슈나 풀 리퀘스트를 자유롭게 열어주세요.

## 📄 라이선스

MIT License - 자세한 내용은 [LICENSE](./LICENSE) 파일을 참조하세요.

## 🙏 영감

이 프로젝트는 다음 라이브러리들에서 영감을 받았습니다:

- [UniRx](https://github.com/neuecc/UniRx) - Reactive Extensions for Unity
- [UniTask](https://github.com/Cysharp/UniTask) - Efficient async/await for Unity
- [R3](https://github.com/Cysharp/R3) - Modern Reactive Extensions

---

Made with ❤️ for Unity Developers
