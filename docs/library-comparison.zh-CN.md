# 与其他库的比较

## 目录

- [UniFP vs Unity-NOPE](#unifp-vs-unity-nope)
  - [性能比较](#性能比较)
  - [功能比较](#功能比较)
  - [详细方法比较](#详细方法比较)
  - [错误类型化：99%的情况下不必要](#错误类型化99的情况下不必要)
- [UniFP vs language-ext](#unifp-vs-language-ext)
  - [为什么不直接在Unity中使用language-ext？](#为什么不直接在unity中使用language-ext)
  - [功能比较](#功能比较-1)

---

## UniFP vs Unity-NOPE

### 性能比较

UniFP改进了NOPE的性能问题。

**1. Zero-GC结构体设计**
- UniFP：所有核心类型都是`readonly struct`，在栈上分配
- Unity-NOPE：`Result<T,E>`是`readonly struct`，但泛型错误类型`E`可能导致装箱

**2. 委托缓存**
- UniFP：`DelegateCache`重用常用的lambda → 防止堆分配
- Unity-NOPE：无委托缓存 → Update循环中重复创建

**3. ResultPool & ListPool**
- UniFP：为高频场景内置对象池
- Unity-NOPE：无池化机制

### 功能比较

UniFP实现了NOPE的所有核心功能，但使用对C#用户更友好的命名。
UniFP的`Then` = NOPE的`Bind`，UniFP的`Filter` = NOPE的`Ensure`

**高级功能比较**

| 功能 | UniFP | Unity-NOPE |
|------|-------|------------|
| Result单子 | `Result<T>`（单一类型） | `Result<T,E>`（类型化错误） |
| Option单子 | `Option<T>` | `Maybe<T>` |
| 异步支持 | UniTask + Awaitable | UniTask + Awaitable |
| 错误类型 | `ErrorCode`（结构体，高效） | `E`（泛型，灵活但可能装箱） |
| 管道操作 | Then, Map, Filter, Recover, Do... | Bind, Map, Ensure, Tap, Finally... |
| 重试逻辑 | Retry, RetryWithBackoff, Repeat | 不支持 |
| 结果组合 | ResultCombinators (Combine, Zip...) | Result.Combine, CombineValues |
| 集合遍历 | SelectResults, CombineAll, Partition | 有限 |
| 性能优化 | DelegateCache, Pools, Span扩展 | 仅基本结构体 |
| 调试工具 | Trace, Breakpoint, SafeExecutor | 仅基本Match |

### 详细方法比较

[表格内容与韩语版本相同，翻译为中文]

**图例：**
- ✅ 完全支持
- ⚠️ 部分支持或以不同名称提供
- ❌ 不支持

### 错误类型化：99%的情况下不必要

Unity-NOPE允许使用`Result<T,E>`进行错误类型化，但对于大多数Unity游戏开发来说是**过度工程**：

**为什么类型化错误不必要？**
- Unity游戏逻辑主要关心"成功了吗？失败了吗？"
- 错误的**消息**比**类型**更有用（用于调试/日志）
- 类型化错误增加泛型参数 → 代码复杂度上升
- 大多数失败都是简单类别，如"资源加载失败"、"验证失败"

**UniFP的方法：ErrorCode结构体**
```csharp
// UniFP：高效清晰的错误分类
var result = LoadAsset()
    .Filter(x => x != null, ErrorCode.NotFound)
    .Then(ValidateAsset);  // 可以返回ErrorCode.ValidationFailed

if (result.IsFailure)
{
    Debug.LogError($"[{result.ErrorCode.Category}] {result.Error}");
    // [Resource] Asset not found: player_model.prefab
}
```

**1%的情况：需要类型安全错误时**

对于确实需要类型化错误的复杂域逻辑：

```csharp
// 方法1：自定义ErrorCode
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

// 方法2：判别联合模式（C# 9.0+）
public record PaymentError
{
    public record InsufficientFunds(decimal Required, decimal Available) : PaymentError;
    public record InvalidCard(string CardNumber) : PaymentError;
    public record NetworkTimeout(int Attempts) : PaymentError;
}

// 序列化到Result的Error消息中
var result = payment switch
{
    PaymentError.InsufficientFunds e => 
        Result<Payment>.Failure(ErrorCode.Custom(1001, "Payment"), 
                                $"不足：还需要{e.Required - e.Available}"),
    // ...
};
```

---

## UniFP vs language-ext

### 为什么不直接在Unity中使用language-ext？

language-ext是.NET生态系统中最好的函数式库，但不适合Unity。

**1. 缺乏Unity运行时优化**
- language-ext为通用.NET设计
- 许多类型基于类 → GC压力增加
- 与Unity的IL2CPP AOT编译可能存在兼容性问题

**2. 功能复杂度压倒性**
- 100+个单子和转换器
- 高阶类型模拟（复杂的泛型模式）
- 游戏开发不需要的：Parsec、Lenses、Free monads等

**3. 学习曲线**
- Haskell风格命名约定（`camelCase`静态函数）
- Trait系统的复杂抽象
- Unity开发者不熟悉的过多函数式概念

**4. 性能开销**
- 高度抽象导致的间接调用
- Unity Profiler中难以识别热路径

### 功能比较

| 类别 | language-ext | UniFP | Unity游戏开发视角 |
|------|--------------|-------|------------------|
| **核心单子** | Option, Either, Try, Validation, Fin | Result, Option, NonEmpty | UniFP提供Unity特定的最小集合 ✅ |
| **不可变集合** | Arr, Lst, Seq, Map, HashMap, Set... | 标准C#集合+扩展 | language-ext优秀但Unity过多 ⚠️ |
| **异步** | IO monad, Eff, Pipes, StreamT | AsyncResult (UniTask/Awaitable) | UniFP Unity生态系统集成更好 ✅ |
| **错误处理** | Either<L,R>, Validation<E,S>, Fin<A> | Result<T> + ErrorCode | UniFP更简单清晰 ✅ |
| **解析器组合子** | Parsec（完整实现） | 不支持 | 游戏不需要（language-ext胜） ❌ |
| **透镜和光学** | 完全支持 | 不支持 | 游戏过多（Unreal的FProperty更合适） ❌ |
| **原子并发** | Atom, Ref, AtomHashMap | 不支持 | Unity单线程为中心，需要时使用C#标准 ⚠️ |
| **性能** | 高度抽象的开销 | Zero-GC结构体，池化优化 | UniFP为Unity优化 ✅ |
| **学习曲线** | 陡峭（需要Haskell背景） | 平缓（C# LINQ经验即可） | UniFP可访问性更好 ✅ |
