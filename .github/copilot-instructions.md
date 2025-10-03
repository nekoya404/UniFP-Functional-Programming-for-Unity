# UniFP AI 코딩 가이드

이 문서는 AI 코딩 에이전트가 UniFP 프로젝트를 효과적으로 작업할 수 있도록 가이드를 제공합니다.

## 프로젝트 개요

**UniFP**는 Unity를 위한 함수형 프로그래밍 라이브러리입니다. Result 모나드와 파이프라인 패턴을 통해 안전하고 명시적인 에러 처리를 제공합니다.

### 핵심 컨셉트
- **Result 모나드**: 성공(Success) 또는 실패(Failure)를 타입으로 표현
- **파이프라인 체이닝**: Bind, Map을 통한 함수형 조합
- **명시적 에러 처리**: 예외 대신 Result로 에러를 전파
- **Zero Allocation**: 구조체 기반 설계로 GC 부담 최소화

## 프로젝트 구조

```
.
├── src/
│   └── UniFP/
│       └── Assets/
│           └── Plugins/
│               └── UniFP/          # 핵심 라이브러리 코드
│                   ├── Result.cs    # Result<T> 모나드 구현
│                   ├── Pipe.cs      # 파이프라인 유틸리티
│                   ├── package.json # UPM 패키지 정의
│                   └── UniFP.asmdef # Assembly Definition
├── Assets/                          # Unity 테스트 프로젝트
│   ├── Test.cs                      # 예제 및 테스트 코드
│   └── ...
├── docs/                            # 문서
│   ├── getting-started.md
│   ├── api-reference.md
│   ├── best-practices.md
│   └── examples.md
├── README.md
├── CHANGELOG.md
└── LICENSE
```

## 아키텍처 원칙

### 1. 구조체 기반 설계
Result<T>는 구조체로 설계되어 스택에 할당됩니다. 이는 GC 부담을 최소화하기 위함입니다.

```csharp
public readonly struct Result<T>  // struct, not class!
```

### 2. 불변성 (Immutability)
모든 연산은 새로운 Result를 반환하며, 기존 Result를 변경하지 않습니다.

```csharp
// 각 연산은 새로운 Result를 반환
var result1 = Result<int>.Success(10);
var result2 = result1.Map(x => x * 2);  // result1은 변경되지 않음
```

### 3. 널 체크 최소화
가독성과 성능을 위해 불필요한 널 체크는 하지 않습니다. 대신 Filter를 사용합니다.

```csharp
// ❌ 피할 것
if (input == null) return Failure("null");
if (input.Length == 0) return Failure("empty");

// ✅ 권장
Pipe.Start(input)
    .Filter(x => x?.Length > 0, "입력이 유효하지 않습니다")
```

## 코드 작성 규칙

### 1. 함수 시그니처
- 에러가 발생할 수 있는 함수는 `Result<T>` 반환
- 순수 변환 함수는 `T` 반환 (Map에서 사용)

```csharp
// Result를 반환 (Bind용)
Result<int> ParseInt(string s)
{
    return int.TryParse(s, out var value)
        ? Result<int>.Success(value)
        : Result<int>.Failure("파싱 실패");
}

// 값을 반환 (Map용)
string FormatAge(int age) => $"{age}세";
```

### 2. 에러 메시지
명확하고 구체적인 에러 메시지를 작성합니다.

```csharp
// ❌ 피할 것
Result<T>.Failure("Error")
Result<T>.Failure("Invalid")

// ✅ 권장
Result<T>.Failure("파일을 찾을 수 없습니다: {path}")
Result<T>.Failure("나이는 0보다 커야 합니다")
```

### 3. 주석 작성
모든 public 메서드에는 XML 문서 주석을 작성합니다.

```csharp
/// <summary>
/// 문자열을 정수로 파싱합니다.
/// </summary>
/// <param name="s">파싱할 문자열</param>
/// <returns>
/// Success: 파싱된 정수
/// Failure: 파싱 실패 메시지
/// </returns>
public Result<int> ParseInt(string s)
```

### 4. Region 사용
생성자는 `#region Construction`으로 그룹화합니다.

```csharp
#region Construction
private Result(bool isSuccess, T value, string error)
{
    _isSuccess = isSuccess;
    _value = value;
    _error = error;
}
#endregion
```

## 일반적인 패턴

### 파이프라인 구성

```csharp
// 기본 패턴
var result = Pipe.Start(initialValue)
    .Bind(Step1)      // Result<T> 반환
    .Map(Step2)       // T 반환
    .Bind(Step3);

// 디버깅 패턴
var result = Pipe.Start(initialValue)
    .Do(x => Debug.Log($"단계 1: {x}"))
    .Bind(Process)
    .Do(x => Debug.Log($"단계 2: {x}"));

// 검증 패턴
var result = Pipe.Start(input)
    .Filter(x => x > 0, "양수여야 합니다")
    .Filter(x => x < 100, "100보다 작아야 합니다");

// 복구 패턴
var result = Pipe.Start(LoadConfig())
    .Recover(error => defaultConfig);
```

### 예외 처리

```csharp
// Try로 예외를 Result로 변환
var result = Pipe.Try(() => 
{
    return JsonUtility.FromJson<Data>(json);
});

// 파이프라인에서 사용
var result = Pipe.Try(() => LoadFile(path))
    .Bind(ValidateData)
    .Bind(ProcessData);
```

## DI 통합 (VContainer)

### 일반 클래스
생성자 주입을 사용하며, 생성자는 Region으로 감쌉니다.

```csharp
public class UserService
{
    private readonly IUserRepository _repository;

    #region Construction
    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }
    #endregion

    public Result<User> GetUser(int id)
    {
        return Pipe.Start(id)
            .Filter(x => x > 0, "ID는 양수여야 합니다")
            .Bind(_repository.FindById);
    }
}
```

### MonoBehaviour
메서드 주입을 사용합니다.

```csharp
public class UserController : MonoBehaviour
{
    private UserService _userService;

    [Inject]
    public void Construct(UserService userService)
    {
        _userService = userService;
    }
}
```

## 테스트 작성

### 단위 테스트 패턴

```csharp
[Test]
public void MethodName_Condition_ExpectedResult()
{
    // Arrange
    var input = "test";

    // Act
    var result = MethodUnderTest(input);

    // Assert
    Assert.IsTrue(result.IsSuccess);
    Assert.AreEqual(expectedValue, result.Value);
}
```

## 피해야 할 안티패턴

### 1. Result 내부에서 예외 던지기
```csharp
// ❌ 잘못됨
public Result<int> Parse(string s)
{
    if (string.IsNullOrEmpty(s))
        throw new ArgumentException();  // Result를 사용하는 의미가 없음!
    // ...
}

// ✅ 올바름
public Result<int> Parse(string s)
{
    if (string.IsNullOrEmpty(s))
        return Result<int>.Failure("입력이 비어있습니다");
    // ...
}
```

### 2. Result 무시하고 Value 직접 접근
```csharp
// ❌ 잘못됨
var result = Process(input);
var value = result.Value;  // 실패 시 예외 발생!

// ✅ 올바름
var result = Process(input);
if (result.IsSuccess)
    var value = result.Value;
```

### 3. 불필요한 중첩
```csharp
// ❌ 잘못됨
var result = Pipe.Start(input)
    .Bind(x => 
    {
        var temp = Process1(x);
        if (temp.IsSuccess)
            return Process2(temp.Value);
        return Result<int>.Failure(temp.Error);
    });

// ✅ 올바름
var result = Pipe.Start(input)
    .Bind(Process1)
    .Bind(Process2);
```

## 빌드 및 배포

### Unity Package Manager (UPM)
이 프로젝트는 UPM 패키지로 배포됩니다. 핵심 코드는 `src/UniFP/Assets/Plugins/UniFP`에 있습니다.

### 버전 관리
- `package.json`의 version 필드를 업데이트
- `CHANGELOG.md`에 변경 사항 기록
- Git 태그 생성: `v1.0.0`

## 참고 자료

- **UniTask**: https://github.com/Cysharp/UniTask - 비동기 처리 패턴 참고
- **UniRx**: https://github.com/neuecc/UniRx - Reactive 패턴 참고
- **R3**: https://github.com/Cysharp/R3 - 최신 Reactive Extensions

## 자주 묻는 질문

### Q: 언제 Bind를 사용하고 언제 Map을 사용하나요?
A: Result를 반환하는 함수는 Bind, 일반 값을 반환하는 함수는 Map을 사용합니다.

### Q: 예외 처리는 어떻게 하나요?
A: `Pipe.Try()`를 사용하여 예외를 Result로 변환합니다.

### Q: 여러 Result를 합치려면?
A: 현재는 중첩된 Bind를 사용하거나, 각 Result를 개별적으로 확인한 후 조합합니다.

---

이 가이드를 따라 일관되고 유지보수하기 쉬운 UniFP 코드를 작성하세요!
