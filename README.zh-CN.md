![C# Functional Programming for Unity Capsule Header](https://capsule-render.vercel.app/api?type=waving&height=220&color=0:5A2BFF,100:1FB5E9&text=C%23%20Functional%20Programming%20for%20Unity&fontAlign=50&fontAlignY=40&fontSize=46&fontColor=FFFFFF&desc=UniFP&descAlign=50&descAlignY=65&descSize=24)

[English](./README.md) · [한국어](./README.ko.md) · [简体中文](./README.zh-CN.md)

# UniFP — 面向 Unity 的 C# 函数式编程

[![Unity](https://img.shields.io/badge/Unity-2020.3%2B-000?logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-success.svg)](./LICENSE)
[![Release](https://img.shields.io/badge/version-v1.0.0-blue)](https://github.com/Nekoya-Jin/UniFP/releases)

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
- [开始使用](#开始使用)
  - [通过 UPM 安装（推荐）](#通过-upm-安装推荐)
  - [手动安装](#手动安装)
- [核心概念](#核心概念)
  - [`Result<T>` —— 逃离 if/else 与 try/catch 迷宫 🔥🔥🔥](#resultt--逃离-ifelse-与-trycatch-迷宫-)
  - [`Option<T>` —— 不再被 null 检查淹没 👻](#optiont--不再被-null-检查淹没-)
  - [错误码与诊断](#错误码与诊断)
  - [`NonEmpty<T>` —— 至少包含一个元素的集合](#nonemptyt--至少包含一个元素的集合)
- [流式流水线](#流式流水线)
  - [分支控制与恢复](#分支控制与恢复)
  - [组合多个结果](#组合多个结果)
  - [集合与遍历](#集合与遍历)
- [异步与 UniTask 集成](#异步与-unitask-集成)
- [韧性工具](#韧性工具)
- [调试与可观测性](#调试与可观测性)
- [性能工具箱](#性能工具箱)
- [示例场景与测试](#示例场景与测试)
- [文档](#文档)
- [贡献指南](#贡献指南)
- [许可证](#许可证)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 亮点

- **`Result<T>` 与 `Option<T>` 结构体** 在零堆分配的情况下提供显式的成功/失败与 Null 安全。
- **铁路式扩展方法**（`Then`、`Map`、`Filter`、`Recover`、`DoStrict`、`IfFailed` 等）让控制流程像叙事一样清晰。
- **面向 UniTask 的异步组合器**（`.ThenAsync`、`.MapAsync`、`.FilterAsync`、`AsyncResult.TryAsync()`）可无缝衔接异步步骤。
- **结果组合器与集合扩展** 帮你合并独立结果、遍历列表或 Span，并在批量处理中进行验证。
- **SafeExecutor** 在编辑器或调试环境中自动记录操作类型与调用位置。
- **DelegateCache、ResultPool、SpanExtensions** 专注性能，让热点代码也能保持零分配。
- **`Assets/Scenes` 示例与 `src/UniFP/Assets/Tests` 单元测试** 展示可直接复用的真实场景。

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

### 手动安装

将 `src/UniFP/Assets/Plugins/UniFP` 拷贝到项目的 `Assets/Plugins/UniFP`。请保留 `UniFP.asmdef`，以确保 Unity 编译速度。

## 核心概念

### `Result<T>` —— 逃离 if/else 与 try/catch 迷宫 🔥🔥🔥

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

### `Option<T>` —— 不再被 null 检查淹没 👻

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

## 异步与 UniTask 集成

UniTask 与 Result 流水线可以无缝拼接。

```csharp
async UniTask<Result<PlayerData>> FetchPlayer(int id)
{
    return await Result.TryFromResult(() => ValidateId(id))
        .ThenAsync(async _ => await Api.GetPlayer(id))
        .MapAsync(payload => payload.ToPlayerData())
        .FilterAsync(data => UniTask.FromResult(data.IsActive), "玩家未处于激活状态");
}

var cached = await FetchPlayer(42).DoAsync(data => Cache.Save(data));
```

将可能抛出的异步操作包裹在 `AsyncResult.TryAsync` 中，即可自动把异常转换为 `Result` 失败。

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
