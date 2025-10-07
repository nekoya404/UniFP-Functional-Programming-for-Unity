![C# Functional Programming for Unity Capsule Header](https://capsule-render.vercel.app/api?type=waving&height=220&color=0:5A2BFF,100:1FB5E9&text=C%23%20Functional%20Programming%20for%20Unity&fontAlign=50&fontAlignY=40&fontSize=46&fontColor=FFFFFF&desc=UniFP&descAlign=50&descAlignY=65&descSize=24)

[English](./README.md) · [한국어](./README.ko.md) · [简体中文](./README.zh-CN.md) · [日本語](./README.ja.md)

# UniFP — 面向 Unity 的 C# 函数式编程

[![Unity](https://img.shields.io/bad- 流水线展示了值必须通过的所有过滤条件。
- 需要更多校验时，再添一个 `Filter` 即可。
- 最终 `Match` 一次，主路径与兜底逻辑清晰分离。

#### 创建 Option

```csharp
using UniFP;

// 1. 直接用 Some / None 创建
var some = Option<int>.Some(42);
var none = Option<int>.None();

// 2. 用 From 将可空值转换为 Option（null 变为 None）
var fromValue = Option<string>.From(PlayerPrefs.GetString("username"));  // null 变为 None
var fromNullable = Option<int>.From(nullableInt);

// 3. 用 Where 条件转换（条件失败时为 None）
var adult = Option<int>.From(age)
    .Where(x => x >= 18);
```

#### 核心 Option 方法

```csharp
// Map：转换值（None 时跳过）
var doubled = Option<int>.Some(10)
    .Map(x => x * 2);  // Some(20)

var stillNone = Option<int>.None()
    .Map(x => x * 2);  // None

// Bind：链接返回 Option 的函数
Option<User> FindUser(string name) => /* ... */;
var user = Option<string>.From(username)
    .Bind(FindUser);

// Filter：条件验证（失败时为 None）
var valid = Option<int>.From(input)
    .Filter(x => x > 0)
    .Filter(x => x < 100);

// Or / OrElse：None 时提供替代值
var withDefault = Option<string>.None()
    .Or(Option<string>.Some("默认值"));

var fromFunc = Option<int>.None()
    .OrElse(() => Option<int>.Some(GetDefaultValue()));

// GetValueOrDefault：从 Option 提取值
var value = someOption.GetValueOrDefault(defaultValue);
var valueOrNull = someOption.GetValueOrDefault();
```

#### Option 与 Result 转换

```csharp
// Option -> Result：将 None 转换为错误
var result = Option<User>.From(FindUser(id))
    .ToResult(ErrorCode.NotFound, "找不到用户");

// Result -> Option：将失败转换为 None（忽略错误）
var option = LoadConfig()
    .ToOption();  // 成功 -> Some，失败 -> None
```

#### 使用 Match 进行分支

```csharp
// Match：根据 Some/None 进行不同处理
var message = Option<User>.From(user).Match(
    onSome: u => $"欢迎，{u.Name}",
    onNone: () => "访客模式");

// IfSome / IfNone：仅处理一种情况
Option<Config>.From(config)
    .IfSome(c => ApplyConfig(c))
    .IfNone(() => UseDefaults());
```

#### 集合辅助方法

```csharp
using System.Linq;

var items = new[] { 1, 2, 3, 4, 5 };

// TryFirst / TryLast：第一个/最后一个元素作为 Option
var first = items.TryFirst();  // Some(1)
var firstEven = items.TryFirst(x => x % 2 == 0);  // Some(2)
var empty = Array.Empty<int>().TryFirst();  // None

// TryFind：查找符合条件的元素
var found = items.TryFind(x => x > 3);  // Some(4)

// Choose：从 Option 集合中仅提取 Some
var options = new[] 
{ 
    Option<int>.Some(1), 
    Option<int>.None(), 
    Option<int>.Some(3) 
};
var values = options.Choose();  // [1, 3]
```

#### LINQ 集成

```csharp
using System.Linq;

// Select：与 Map 相同
var doubled = Option<int>.Some(10)
    .Select(x => x * 2);  // Some(20)

// Where：与 Filter 相同
var filtered = Option<int>.Some(42)
    .Where(x => x > 18);  // Some(42)

// SelectMany：与 Bind 相同（支持 LINQ 查询语法）
var result = 
    from name in Option<string>.From(username)
    from user in FindUser(name)
    from profile in LoadProfile(user.Id)
    select profile;
```

---

### `NonEmpty<T>` 使用方法

`NonEmpty<T>` 是**保证至少有一个元素**的集合。适用于不允许为空的领域，如队伍编成或必填槽位。

#### 创建 NonEmpty

```csharp
using UniFP;

// Create：用至少一个元素创建
var squad = NonEmpty.Create("Leader", "Support", "Tank");
var single = NonEmpty.Create(42);

// FromList：从列表转换（空列表则失败）
var list = new List<string> { "A", "B", "C" };
var nonEmpty = NonEmpty.FromList(list);  // Result<NonEmpty<string>>

var emptyList = new List<string>();
var failed = NonEmpty.FromList(emptyList);  // Failure（空）
```

#### NonEmpty 方法

```csharp
// Head / Tail：第一个元素和其余部分
var squad = NonEmpty.Create("Leader", "Tank", "Healer");
var leader = squad.Head;  // "Leader"（始终存在）
var others = squad.Tail;  // ["Tank", "Healer"]（IEnumerable）

// Map：转换所有元素
var upper = squad.Map(role => role.ToUpper());  // NonEmpty<string>

// Append / Prepend：添加元素
var expanded = squad.Append("Mage");  // NonEmpty（仍至少一个）
var withNewLeader = squad.Prepend("NewLeader");

// ToList / ToArray：转换为普通集合
var list = squad.ToList();
var array = squad.ToArray();
```

#### 使用示例

```csharp
// 队伍系统：至少需要一个队长
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
        // 编译时保证至少有一个成员
        _members.Map(p => p.ApplyBuff());
    }
}

// 配置：至少需要一个服务器地址
var servers = NonEmpty.Create(
    "https://primary.server.com",
    "https://backup1.server.com",
    "https://backup2.server.com"
);

var primary = servers.Head;
var fallbacks = servers.Tail;
```

---

### 错误码与诊断

UniFP 使用 `ErrorCode` 结构体提供**零 GC 错误分类**。

#### 内置 ErrorCode

```csharp
// 0-999：UniFP 保留范围
ErrorCode.None              // 0：无错误
ErrorCode.Unknown           // 1：未知错误
ErrorCode.InvalidInput      // 100：无效输入
ErrorCode.ValidationFailed  // 101：验证失败
ErrorCode.NotFound          // 102：未找到
ErrorCode.Unauthorized      // 103：未授权
ErrorCode.OperationFailed   // 104：操作失败
ErrorCode.Timeout           // 105：超时
ErrorCode.NetworkError      // 106：网络错误
ErrorCode.Forbidden         // 107：禁止
ErrorCode.InvalidOperation  // 108：无效操作
```

#### 自定义 ErrorCode

```csharp
// 1000+：用户定义错误码
public static class GameErrors
{
    public static readonly ErrorCode InsufficientGold = 
        ErrorCode.Custom(1001, "Economy");
    
    public static readonly ErrorCode InventoryFull = 
        ErrorCode.Custom(1002, "Inventory");
    
    public static readonly ErrorCode QuestNotAvailable = 
        ErrorCode.Custom(1003, "Quest");
}

// 使用示例
var result = PurchaseItem(itemId, price)
    .Filter(success => player.Gold >= price, GameErrors.InsufficientGold, 
            $"金币不足：还需要 {price - player.Gold}");
```

#### ErrorCode 属性

```csharp
var error = ErrorCode.NotFound;

error.Code;       // 102
error.Category;   // "Resource"
error.IsCustom;   // false（内置代码）

var custom = ErrorCode.Custom(2001, "Payment");
custom.Code;      // 2001
custom.Category;  // "Payment"
custom.IsCustom;  // true
```

#### 诊断信息（调试模式）

```csharp
// 在 Editor 或 UNIFP_DEBUG 环境中自动记录
var result = LoadAsset(path)
    .Filter(asset => asset != null, ErrorCode.NotFound);

if (result.IsFailure)
{
    // 失败时自动记录的信息
    Debug.LogError($"[{result.ErrorCode.Category}] {result.Error}");
    Debug.LogError($"位置：{result.FilePath}:{result.LineNumber}");
    Debug.LogError($"方法：{result.MemberName}");
    Debug.LogError($"操作类型：{result.OperationType}");
    
    // 示例输出：
    // [Resource] Asset not found: player_model.prefab
    // 位置：Assets/Scripts/AssetLoader.cs:42
    // 方法：LoadPlayerModel
    // 操作类型：Filter
}
```

---

## 流式流水线-000?logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-success.svg)](./LICENSE)
[![Version](https://img.shields.io/github/package-json/v/Nekoya-Jin/UniFP?filename=src%2FUniFP%2FAssets%2FPlugins%2FUniFP%2Fpackage.json&label=version&color=blue)](https://github.com/Nekoya-Jin/UniFP/releases)

UniFP 是一款受 Rust、Haskell 与 F# 启发的 Unity 专用 C# 函数式编程框架，追求 GC 零分配，让函数式思维与显式错误处理安全落地在游戏逻辑中。

传统的 C# 函数式库（例如 [language-ext](https://github.com/louthy/language-ext)）面向通用 .NET 应用，抽象层级繁复、学习曲线陡峭，而且多使用引用类型实现，往往会在 Unity 运行时引入额外 GC 负担。

UniFP 将 Rust 式的类型精度与函数式语言的“铁路编程”理念移植到 Unity C#：无需引入庞大依赖，即可拥有轻量、实时友好的工具箱，在不牺牲性能的前提下构建声明式流水线与安全的错误传播。

`Result<T>` 与 `Option<T>` 是整套扩展链的核心，帮助你用类型安全的流程控制取代异常，并将分配压到最低。

> 所有核心原语都以结构体提供。在 Editor 或定义了 `UNIFP_DEBUG` 的环境中，每个操作都会自动记录发生的文件、行号与方法，无需额外配置即可获得详尽诊断信息。

**UniFP 不是什么**

❌ 把所有 Unity 脚本都改写成纯函数式代码 🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️
✅ 把现有逻辑中复杂的分支与错误处理梳理成可读的函数式流水线 🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## 目录

- [亮点](#亮点)
- [与其他库的比较](#与其他库的比较)
- [开始使用](#开始使用)
  - [通过 UPM 安装（推荐）](#通过-upm-安装推荐)
  - [手动安装](#手动安装)
  - [可选依赖项](#可选依赖项)
- [核心概念](#核心概念)
  - [`Result<T>` 使用方法](#resultt-使用方法)
    - [创建 Result](#创建-result)
    - [核心方法：Then、Map、Filter](#核心方法thenmap filter)
    - [错误处理与恢复](#错误处理与恢复)
    - [副作用](#副作用)
    - [条件执行](#条件执行)
    - [异步 Result（UniTask / Awaitable）](#异步-resultunitask--awaitable)
  - [`Option<T>` 使用方法](#optiont-使用方法)
    - [创建 Option](#创建-option)
    - [核心 Option 方法](#核心-option-方法)
    - [Option 与 Result 转换](#option-与-result-转换)
    - [使用 Match 进行分支](#使用-match-进行分支)
    - [集合辅助方法](#集合辅助方法)
    - [LINQ 集成](#linq-集成)
  - [`NonEmpty<T>` 使用方法](#nonemptyt-使用方法)
    - [创建 NonEmpty](#创建-nonempty)
    - [NonEmpty 方法](#nonempty-方法)
    - [使用示例](#使用示例)
  - [错误码与诊断](#错误码与诊断)
    - [内置 ErrorCode](#内置-errorcode)
    - [自定义 ErrorCode](#自定义-errorcode)
    - [ErrorCode 属性](#errorcode-属性)
    - [诊断信息（调试模式）](#诊断信息调试模式)
- [流式流水线](#流式流水线)
  - [分支控制与恢复](#分支控制与恢复)
  - [组合多个结果](#组合多个结果)
  - [集合与遍历](#集合与遍历)
- [异步支持（UniTask / Awaitable）](#异步支持unitask--awaitable)
- [韧性工具](#韧性工具)
- [调试与可观测性](#调试与可观测性)
- [性能工具箱](#性能工具箱)
- [示例场景与测试](#示例场景与测试)
- [文档](#文档)
- [贡献指南](#贡献指南)
- [许可证](#许可证)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 亮点

- **`Result<T>` 与 `Option<T>` 结构体**实现了明确的成功/失败与空安全，且无堆分配。
- **铁路导向的扩展方法**（`Then`、`Map`、`Filter`、`Recover`、`DoStrict`、`IfFailed` 等）提供高可读性的流水线。
- **同时支持 UniTask 和 Unity Awaitable** 的异步流水线（`.ThenAsync`、`.MapAsync`、`.FilterAsync`、`AsyncResult.TryAsync()`）可无缝衔接异步步骤。
- **ResultCombinators 与集合扩展**用于合并多个结果或在列表/Span 上进行条件验证与遍历。
- **SafeExecutor** 在编辑器或调试环境中自动记录操作类型与调用位置。
- **DelegateCache、ResultPool、SpanExtensions** 专注性能，让热点代码也能保持零分配。
- **`Assets/Scenes` 示例与 `src/UniFP/Assets/Tests` 单元测试** 展示可直接复用的真实场景。

## 与其他库的比较

### UniFP vs Unity-NOPE

#### 性能优势
- **Zero-GC 结构体设计**：`readonly struct` 栈分配，避免装箱
- **委托缓存**：`DelegateCache` 重用 lambda，防止堆分配
- **对象池**：`ResultPool` & `ListPool` 支持高频场景

#### 功能比较

| 分类 | UniFP | Unity-NOPE |
|------|-------|------------|
| 错误处理 | `Result<T>` + `ErrorCode` | `Result<T,E>` |
| 可选值 | `Option<T>` | `Maybe<T>` |
| 异步 | UniTask + Awaitable | UniTask + Awaitable |
| 重试 | Retry、RetryWithBackoff | 不支持 |
| 性能优化 | 委托缓存 + 对象池 + Span 扩展 | 仅基础结构体 |

➡️ 详细比较：[库比较文档](./docs/library-comparison.zh-CN.md)

### UniFP vs language-ext

language-ext 是 .NET 生态系统中最好的函数式库，但不适合 Unity：

- **缺乏 Unity 优化**：为通用 .NET 设计，许多类型基于类，GC 压力大
- **功能复杂度过高**：100+ 单子和转换器，学习成本高
- **学习曲线陡峭**：Haskell 风格命名（Bind、Applicative、Monad...）

➡️ 详细比较：[库比较文档](./docs/library-comparison.zh-CN.md)

---

## 开始使用

### 通过 UPM 安装（推荐）

1. 在 Unity 中打开 **Window ▸ Package Manager**。
2. 选择 **Add package from git URL...**，粘贴以下地址。

   ```text
   https://github.com/Nekoya-Jin/UniFP.git?path=src/UniFP/Assets/Plugins/UniFP
   ```

3. Unity 会安装 `com.unifp.core`，同时导入 asmdef 及示例。

如果希望直接在 `Packages/manifest.json` 固定依赖，可添加：

```json
{
  "dependencies": {
    "com.unifp.core": "https://github.com/Nekoya-Jin/UniFP.git?path=src/UniFP/Assets/Plugins/UniFP"
  }
}
```

### 可选依赖项

UniFP 可独立工作，但您可以通过安装以下任一选项来增强异步功能：

**选项 1：UniTask**（推荐用于 Unity 2020.3+）
- 比 Unity Awaitable 功能更丰富，性能更优
- 通过 UPM 安装：
  ```text
  https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
  ```
- 启用 `AsyncResult.ThenAsync`、`MapAsync`、`FilterAsync`、`DoAsync`、`TryAsync`

**选项 2：Unity Awaitable**（Unity 6.0+）
- Unity 6.0+ 内置，无需安装
- 通过 UniFP.asmdef 中的 `versionDefines` 自动检测
- 提供与 UniTask 相同的异步 API

**无异步支持时：**
- 所有同步 `Result<T>` 功能完全可用
- 异步扩展方法不可用

## 核心概念

### `Result<T>` 使用方法

`Result<T>` 将**成功**（Success）或**失败**（Failure）表达为类型，让您摆脱 if/else 和 try/catch 地狱。

你一定见过这样的代码：`if` 里嵌着 `try`，`try` 又嵌着另一个 `if-else`。成功逻辑、失败逻辑、异常处理和兜底值搅成一团。再加一条校验规则，迷宫就更深一层，最后谁都不敢动这个文件。

#### 传统 C# 写法

```csharp
using UnityEngine;

public class LoginSample : MonoBehaviour
{
    void Start()
    {
        var userId = PlayerPrefs.GetString("userId");

        if (string.IsNullOrWhiteSpace(userId))
        {
            Debug.LogError("登录失败：输入为空");
            userId = "guest";
        }
        else
        {
            try
            {
                if (!ValidateAccount(userId))
                {
                    Debug.LogWarning("登录失败：未知用户");
                    userId = "guest";
                }
                else
                {
                    Debug.Log($"登录成功：{userId}");
                    LogUser(userId);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"登录时抛出异常：{ex.Message}");
                userId = "guest";
            }
        }
    }

    bool ValidateAccount(string id) => id == "player42";

    void LogUser(string id) => Debug.Log($"认证流水线允许 {id} 登录");
}
```

- 校验、异常、兜底逻辑散落在各层嵌套中。
- 每多一种失败场景，复杂度与维护成本都随之暴涨。

#### 使用 UniFP `Result<T>` 重构

UniFP 把所有分支与异常放到同一条传送带上。数据沿着成功高速路前进，一旦出错就进入专门的失败车道。代码自上而下阅读，每一步的意图都一目了然。

```csharp
using UniFP;
using UnityEngine;

public class LoginSample : MonoBehaviour
{
    void Start()
    {
        var loginResult = Result.FromValue(PlayerPrefs.GetString("userId"))
            // 1. 输入是否合法？否则直接返回 InvalidInput。
            .Filter(DelegateCache.IsNotNullOrWhitespace, ErrorCode.InvalidInput)
            // 2. 账号是否存在？否则直接返回 NotFound。
            .Then(id => ValidateAccount(id)
                ? Result<string>.Success(id)
                : Result<string>.Failure(ErrorCode.NotFound))
            // 3. 只有在成功通道上才执行副作用。
            .Do(LogUser)
            // 🚨 离开高速公路时，最终落在 "guest" 兜底。
            .Recover(_ => "guest");

        loginResult.Match(
            onSuccess: id => Debug.Log($"登录成功：{id}"),
            onFailure: code => Debug.LogError($"登录失败：{code}"));
    }

    bool ValidateAccount(string id) => id == "player42";
    void LogUser(string id) => Debug.Log($"认证流水线允许 {id} 登录");
}
```

- 每个步骤都显式表达，成功/失败流程一览无余。
- 失败自动进入 `Recover`，把兜底逻辑与主路径彻底分离。
- 继续添加校验或异步调用时，只需链式补上 `Then`、`Filter` 或 `ThenAsync`。

#### 创建 Result

```csharp
using UniFP;

// 1. 直接用 Success / Failure 创建
var success = Result<int>.Success(42);
var failure = Result<int>.Failure(ErrorCode.NotFound);
var failureWithMsg = Result<int>.Failure(ErrorCode.ValidationFailed, "年龄必须大于 0");

// 2. 用 FromValue 将值提升为 Result
var fromValue = Result.FromValue(userId);

// 3. 用 Try 将异常转换为 Result
var parseResult = Result.Try(() => int.Parse(input));
var parseWithCode = Result.Try(() => int.Parse(input), ErrorCode.InvalidInput);
```

#### 核心方法：Then、Map、Filter

```csharp
// Then：链接返回 Result 的函数
Result<User> LoadUser(int id) => /* ... */;
var result = Result.FromValue(42)
    .Then(LoadUser);  // int -> Result<User>

// Map：转换返回普通值的函数
var doubled = Result.FromValue(10)
    .Map(x => x * 2);  // int -> int（自动包装为 Result<int>）

// Filter：条件验证（条件失败时返回 Failure）
var validated = Result.FromValue(age)
    .Filter(x => x >= 18, ErrorCode.ValidationFailed, "仅限成人");
```

> **💡 提示：Then vs Map**
> - `Then` 用于返回 Result 的函数（可能失败的操作）
> - `Map` 用于返回普通值的函数（简单转换）

#### 错误处理与恢复

```csharp
// Recover：用默认值替换失败
var withDefault = LoadConfig()
    .Recover(code => DefaultConfig);

// IfFailed：失败时执行替代管道
var cached = LoadFromServer()
    .IfFailed(() => LoadFromCache());

// Catch：拦截并恢复特定错误
var result = LoadResource()
    .Catch(ErrorCode.NotFound, () => CreateDefault());

// Match：根据成功/失败进行不同处理
result.Match(
    onSuccess: user => Debug.Log($"欢迎，{user.Name}"),
    onFailure: code => Debug.LogError($"加载失败：{code}"));
```

#### 副作用

```csharp
// Do：仅在成功时执行副作用（失败时跳过）
var result = LoadUser(id)
    .Do(user => Analytics.Track("UserLoaded", user.Id))
    .Do(user => Debug.Log($"已加载：{user.Name}"));

// DoStrict：如果副作用失败则中止管道
var saved = CreateUser(data)
    .DoStrict(user => SaveToDatabase(user));  // 数据库保存失败时整体失败

// IfFailed：仅在失败时执行副作用
var result = Process()
    .IfFailed(code => Debug.LogError($"处理失败：{code}"));
```

#### 条件执行

```csharp
// ThenIf / MapIf：根据条件选择性转换
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

#### 异步 Result（UniTask / Awaitable）

```csharp
using Cysharp.Threading.Tasks;  // 或 using UnityEngine;（Awaitable）

// ThenAsync：异步 Result 链接
async UniTask<Result<User>> LoadUserAsync(int id)
{
    return await Result.FromValue(id)
        .Filter(x => x > 0, ErrorCode.InvalidInput)
        .ThenAsync(async id => await FetchFromAPI(id))
        .MapAsync(json => ParseUser(json))
        .FilterAsync(user => UniTask.FromResult(user.IsActive), "非活跃用户");
}

// TryAsync：将抛出异常的异步工作转换为 Result
var result = await AsyncResult.TryAsync(async () => 
{
    var response = await httpClient.GetAsync(url);
    return await response.Content.ReadAsStringAsync();
}, ErrorCode.NetworkError);

// DoAsync：异步副作用
var saved = await LoadUser(id)
    .DoAsync(async user => await SaveToCloud(user));
```

---

### `Option<T>` 使用方法

`Option<T>` 将**有值**（Some）或**无值**（None）表达为类型，让您摆脱 null 地狱。

Unity 项目里常见几十行的 `null` 防御：`if (foo == null)` → `else if (foo.Bar == null)` → `else if (foo.Bar.Length == 0)`…… 日志满天飞，你却要花大把时间查到底是哪一支触发了 `NullReferenceException`。

#### 传统 C# 写法

```csharp
public class UserProfileLoader
{
    public void Load()
    {
        var raw = PlayerPrefs.GetString("profile");

        if (string.IsNullOrEmpty(raw))
        {
            Debug.LogWarning("找不到存档：使用默认值");
            ApplyDefaults();
            return;
        }

        var profile = JsonUtility.FromJson<UserProfile>(raw);
        if (profile == null || profile.Items == null || profile.Items.Length == 0)
        {
            Debug.LogError("存档损坏：尝试恢复");
            ApplyDefaults();
            return;
        }

        Debug.Log($"存档加载成功：{profile.Name}");
        Apply(profile);
    }
}
```

- 防御性 `if` 堆叠，核心流程被淹没。
- 条件越多，缩进越深，也越容易漏掉某个检查。

#### 使用 UniFP `Option<T>` 重构

`Option<T>` 用 `Some`/`None` 明确表示值是否存在。一旦变成 `None`，后续流水线全部跳过，再也不用手写 `null` 检查。

```csharp
using UniFP;

public class UserProfileLoader
{
    public void Load()
    {
        var profileOption = Option<string>.From(PlayerPrefs.GetString("profile"))
            // 1. 空字符串立即变成 None。
            .Filter(DelegateCache.IsNotNullOrWhitespace)
            // 2. 将 JSON 解析结果提升为 Option。
            .Map(raw => JsonUtility.FromJson<UserProfile>(raw))
            .Filter(result => result is { Items: { Length: > 0 } });

        profileOption.Match(
            onSome: Apply,
            onNone: ApplyDefaults);
    }
}
```

- 流水线展示了值必须通过的所有过滤条件。
- 需要更多校验时，再添一个 `Filter` 即可。
- 最终 `Match` 一次，主路径与兜底逻辑清晰分离。

### 错误码与诊断

`ErrorCode` 使用枚举而非字符串，显著降低 GC 分配。处于 Editor 或启用 `UNIFP_DEBUG` 时，`SafeExecutor` 会记录每次失败的操作类型（`Map`、`Filter`、`Then` 等）与精确调用点，诊断信息即时可得。

### `NonEmpty<T>` —— 至少包含一个元素的集合

当“为空”不可接受时，请使用 `NonEmpty<T>`——例如队伍编成、必填槽位或关键队列。

```csharp
var squad = NonEmpty.Create("Leader", "Support", "Tank");
var upper = squad.Map(role => role.ToUpperInvariant());
```

## 流式流水线

导入 `UniFP` 命名空间即可使用全部扩展方法。铁路式模式让成功路径与失败路径无需嵌套即可分离。

```csharp
var pipeline = Result.FromValue(request)
    .Filter(req => req.IsValid, ErrorCode.ValidationFailed)
    .Then(Persist)
    .DoStrict(SendAnalyticsEvent)
    .IfFailed(() => LoadCachedResult())
    .Trace("Purchase");
```

### 分支控制与恢复

- `Recover(Func<ErrorCode, T>)` 将失败替换为兜底值。
- `IfFailed(Func<Result<T>>)` 在失败时执行备用流水线。
- `ThenIf`、`MapIf` 可按条件追加额外步骤。
- `DoStrict` 让副作用在失败时也能把错误冒泡出来。

### 组合多个结果

使用 `ResultCombinators` 将多个独立操作汇总为一个结果。

```csharp
var stats = ResultCombinators.Combine(
    LoadLevelProgress(),
    LoadInventory());

var snapshot = stats.Zip(
    CalculateRewards(),
    (progress, inventory, rewards) => new PlayerSnapshot(progress, inventory, rewards));
```

### 集合与遍历

- `SelectResults` 遍历集合，遇到第一个失败立即停止。
- `CombineAll` 把多个 `Result<T>` 聚合为单个 `Result<List<T>>`。
- `FilterResults`、`Partition`、`Fold`、`AggregateResults` 支持批量校验与汇总。
- `SpanExtensions` 在 Burst 关键代码中也能保持零分配。

## 异步支持（UniTask / Awaitable）

UniFP 支持使用 **UniTask**（推荐）和 **Unity Awaitable**（Unity 6.0+）进行异步操作。

**安装 UniTask 时：**
```csharp
using Cysharp.Threading.Tasks;

async UniTask<Result<PlayerData>> FetchPlayer(int id)
{
    return await Result.TryFromResult(() => ValidateId(id))
        .ThenAsync(async _ => await Api.GetPlayer(id))
        .MapAsync(payload => payload.ToPlayerData())
        .FilterAsync(data => UniTask.FromResult(data.IsActive), "玩家未处于激活状态");
}
```

**使用 Unity 6.0+（Awaitable）时：**
```csharp
using UnityEngine;

async Awaitable<Result<PlayerData>> FetchPlayer(int id)
{
    return await Result.TryFromResult(() => ValidateId(id))
        .ThenAsync(async _ => await Api.GetPlayer(id))
        .MapAsync(payload => payload.ToPlayerData())
        .FilterAsync(data => Awaitable.FromResult(data.IsActive), "玩家未处于激活状态");
}
```

两种方式提供相同的 API - 只需替换异步类型即可！

## 韧性工具

- `Retry`、`RetryAsync` 在放弃前尝试多次。
- `RetryWithBackoff` 针对不稳定服务应用指数退避。
- `Repeat`、`RepeatAsync` 适合必须连续成功 N 次的流程。
- `Catch` 截获特定失败并路由到自定义逻辑。

```csharp
var response = await RetryExtensions.RetryWithBackoff(
    () => Api.SendAsync(payload),
    maxAttempts: 5,
    initialDelayMilliseconds: 200,
    backoffMultiplier: 2.5f);
```

## 调试与可观测性

- `SafeExecutor` 记录每个操作的类型与调用位置。
- `PipelineDebug.Trace`、`TraceWith`、`TraceOnFailure`、`Breakpoint` 让你在迭代中观察流水线状态。
- `OperationType` 枚举可立即告知故障发生在 `Map`、`Filter`、`Then` 等哪一步。

```csharp
var result = LoadConfig()
    .Trace("Config")
    .Assert(cfg => cfg.Version >= 2, "Config version too old")
    .Breakpoint();
```

## 性能工具箱

虽然基本使用没有大问题，但如果是像 Update 这样每帧都要执行的逻辑，则需要以下优化：

- **DelegateCache** 静态缓存常用 Lambda。
- **ResultPool 与 ListPool<T>** 在高频循环中复用集合，避免 GC 震荡。
- **SpanExtensions** 通过栈或对象池缓冲区进行数据转换。
- **零分配错误流程** 依托结构体 Monad、`ErrorCode` 与 `OperationType` 让堆分配保持安静。

这些工具驱动了仓库中的示例：库存处理、战斗判定、商店购买等都能在无 GC 的情况下稳定运行。

## 示例场景与测试

- `Assets/Scenes`
  - `01_BasicResultExample` —— Result 入门示例
  - `02_PipelineExample` —— 链式模式
  - `04_AsyncExample` —— 基于 UniTask 的异步流程
  - `06_PerformanceExample` —— 零分配技巧
  - `10_RealWorld_UserLogin` —— 强健的登录流水线
  - `11_RealWorld_ItemPurchase` —— 服务间的铁路式编排
- 测试位于 `src/UniFP/Assets/Tests`，覆盖校验、异步失败、重试等关键边界场景。

在仓库根目录运行完整测试：

```bash
dotnet test src/UniFP/UniFP.Tests.csproj
```

## 文档

更多扩展指南位于 [`docs`](./docs) 目录。

- [快速入门](./docs/getting-started.md)
- [API 参考](./docs/api-reference.md)
- [示例集合](./docs/examples.md)
- [最佳实践](./docs/best-practices.md)

## 贡献指南

欢迎提交 Issue 与 Pull Request。请在修改前后补充相关测试与示例。

## 许可证

UniFP 基于 [MIT License](./LICENSE) 发布。
