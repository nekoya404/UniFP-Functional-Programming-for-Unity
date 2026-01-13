![C# Functional Programming for Unity Capsule Header](https://capsule-render.vercel.app/api?type=waving&height=220&color=0:5A2BFF,100:1FB5E9&text=C%23%20Functional%20Programming%20for%20Unity&fontAlign=50&fontAlignY=40&fontSize=46&fontColor=FFFFFF&desc=UniFP&descAlign=50&descAlignY=65&descSize=24)

[English](./README.md) · [한국어](./README.ko.md) · [简体中文](./README.zh-CN.md) · [日本語](./README.ja.md)

# UniFP — Unity向けC#関数型プログラミング

[![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-000?logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-success.svg)](./LICENSE)
[![Version](https://img.shields.io/github/package-json/v/Nekoya-Jin/UniFP?filename=src%2FUniFP%2FAssets%2FPlugins%2FUniFP%2Fpackage.json&label=version&color=blue)](https://github.com/Nekoya-Jin/UniFP/releases)

UniFPは、Rust、Haskell、F#からインスピレーションを得た、Unity向けのGCゼロアロケーションC#関数型プログラミングフレームワークです。ゲームロジックに関数型の考え方と明示的なエラー処理を導入し、ランタイムパフォーマンスを損なわずに実現します。

従来のC#関数型ライブラリ（例：[language-ext](https://github.com/louthy/language-ext)）は汎用的な.NET環境を対象としており、広範な機能と複雑な抽象化を提供しますが、学習曲線が急で、構造体を避ける場合が多く、UnityランタイムでGCアロケーションとパフォーマンス低下を招きます。

UniFPは、Rustの型システムベースの安定性とパフォーマンス重視の哲学を、関数型言語の鉄道指向プログラミングパラダイムと組み合わせ、リアルタイムアプリケーションに最適化された軽量な代替手段として開発されました。重い依存関係なしに、ゲームプレイコードで安全なエラー処理と宣言的パイプラインを可能にします。

`Result<T>`と`Option<T>`は、例外の代わりに型安全なフロー制御を実装するパイプライン拡張を提供し、GC負荷を最小限に抑えます。

> すべてのコア型は構造体として提供されます。EditorまたはUNIFP_DEBUG環境では、各操作が自動的にファイル/行/メソッド情報を記録します。追加の設定なしでUnityプロジェクトで直接使用できます。

**UniFPは何でないか**

❌ すべてのUnityスクリプトを関数型スタイルで書き直すこと🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️🙅‍♂️

✅ 既存のロジックの複雑な分岐とエラー処理を関数型パイプラインで簡素化すること🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️🙆‍♂️

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## 目次

- [コアハイライト](#コアハイライト)
- [他のライブラリとの比較](#他のライブラリとの比較)
- [はじめに](#はじめに)
  - [UPMインストール（推奨）](#upmインストール推奨)
  - [手動インストール](#手動インストール)
  - [オプショナル依存](#オプショナル依存)
- [コアコンセプト](#コアコンセプト)
  - [`Result<T>` 使用法](#resultt-使用法)
    - [Result作成](#result作成)
    - [コアメソッド: Then, Map, Filter](#コアメソッド-then-map-filter)
    - [エラー処理と復旧](#エラー処理と復旧)
    - [副作用](#副作用)
    - [条件付き実行](#条件付き実行)
    - [非同期Result (UniTask / Awaitable)](#非同期result-unitask--awaitable)
  - [`Option<T>` 使用法](#optiont-使用法)
    - [Option作成](#option作成)
    - [コアOptionメソッド](#コアoptionメソッド)
    - [OptionとResultの変換](#optionとresultの変換)
    - [Matchで分岐処理](#matchで分岐処理)
    - [コレクションヘルパー](#コレクションヘルパー)
    - [LINQ統合](#linq統合)
  - [`NonEmpty<T>` 使用法](#nonemptyt-使用法)
    - [NonEmpty作成](#nonempty作成)
    - [NonEmptyメソッド](#nonemptyメソッド)
    - [使用例](#使用例)
  - [エラーコードと診断](#エラーコードと診断)
    - [組み込みErrorCode](#組み込みerrorcode)
    - [カスタムErrorCode](#カスタムerrorcode)
    - [ErrorCodeプロパティ](#errorcodeプロパティ)
    - [診断情報（デバッグモード）](#診断情報デバッグモード)
- [フルエントパイプライン](#フルエントパイプライン)
  - [分岐制御と復旧](#分岐制御と復旧)
  - [複数の結果の結合](#複数の結果の結合)
  - [コレクションと走査](#コレクションと走査)
- [非同期サポート（UniTask / Awaitable）](#非同期サポートunitask--awaitable)
- [復元力ユーティリティ](#復元力ユーティリティ)
- [デバッグと可視性](#デバッグと可視性)
- [パフォーマンスツールキット](#パフォーマンスツールキット)
- [サンプルシーンとテスト](#サンプルシーンとテスト)
- [ドキュメント](#ドキュメント)
- [貢献](#貢献)
- [ライセンス](#ライセンス)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## コアハイライト

- **`Result<T>`と`Option<T>`構造体**は、ヒープアロケーションなしで明示的な成功/失敗とnull安全性を実装します。
- **鉄道スタイルの拡張メソッド**（`Then`、`Map`、`Filter`、`Recover`、`DoStrict`、`IfFailed`など）は、非常に読みやすいパイプラインを提供します。
- **UniTaskとUnity Awaitableの両方に対応**した非同期パイプライン（`.ThenAsync`、`.MapAsync`、`.FilterAsync`、`AsyncResult.TryAsync()`）を提供します。
- **ResultCombinatorsとコレクション拡張**により、複数のResultを結合したり、条件付き検証でリスト/Spanを走査できます。
- **SafeExecutor計測**により、Editor/デバッグ環境で操作タイプと呼び出し位置が自動的に記録されます。
- **DelegateCache、ResultPool、SpanExtensions**などのパフォーマンス重視のユーティリティにより、高頻度コードでもGCを抑制します。
- **`Assets/Scenes`デモと`src/UniFP/Assets/Tests`ユニットテスト**により、実際の使用パターンをすぐに確認できます。

## 他のライブラリとの比較

### UniFP vs Unity-NOPE

#### パフォーマンス優位性
- **Zero-GC構造体設計**: `readonly struct` スタック割り当て、ボクシング回避
- **デリゲートキャッシング**: `DelegateCache` でラムダ再利用、ヒープ割り当て防止
- **オブジェクトプール**: `ResultPool` & `ListPool` で高頻度シナリオ対応

#### 機能比較

| カテゴリ | UniFP | Unity-NOPE |
|------|-------|------------|
| エラー処理 | `Result<T>` + `ErrorCode` | `Result<T,E>` |
| オプション | `Option<T>` | `Maybe<T>` |
| 非同期 | UniTask + Awaitable | UniTask + Awaitable |
| リトライ | Retry、RetryWithBackoff | 非対応 |
| 性能最適化 | デリゲートキャッシュ + プール + Span拡張 | 基本的な構造体のみ |

➡️ 詳細比較: [ライブラリ比較ドキュメント](./docs/library-comparison.ja.md)

### UniFP vs language-ext

language-extは.NETエコシステム最高の関数型ライブラリですが、Unityには適していません：

- **Unity最適化の欠如**: 汎用.NET向け設計、多くの型がクラスベースでGC負荷大
- **圧倒的な機能複雑性**: 100以上のモナドとトランスフォーマー、学習コスト高
- **学習曲線**: Haskellスタイル命名（Bind、Applicative、Monad...）

➡️ 詳細比較: [ライブラリ比較ドキュメント](./docs/library-comparison.ja.md)

---

## はじめに

### UPMインストール（推奨）

1. Unityで**Window ▸ Package Manager**を開きます。
2. **Add package from git URL...**を選択し、以下のアドレスを入力します。

   ```text
   https://github.com/Nekoya-Jin/UniFP.git?path=src/UniFP/Assets/Plugins/UniFP
   ```

3. Unityが`com.unifp.core`パッケージをインストールし、サンプルとasmdefを含むフォルダを追加します。

`Packages/manifest.json`を直接編集する場合は、次の依存関係を追加してください。

```json
{
  "dependencies": {
    "com.unifp.core": "https://github.com/Nekoya-Jin/UniFP.git?path=src/UniFP/Assets/Plugins/UniFP"
  }
}
```

### オプショナル依存

UniFPは単独で動作しますが、以下のいずれかをインストールすることで非同期機能を強化できます：

**オプション1: UniTask**（Unity 2022.3+推奨）
- Unity Awaitableよりも多くの機能と優れたパフォーマンス
- UPMインストール:
  ```text
  https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
  ```
- `AsyncResult.ThenAsync`、`MapAsync`、`FilterAsync`、`DoAsync`、`TryAsync`が有効化

**オプション2: Unity Awaitable**（Unity 6.0+）
- Unity 6.0+に内蔵、別途インストール不要
- UniFP.asmdefの`versionDefines`により自動検出
- UniTaskと同じ非同期APIを提供

**非同期サポートなし:**
- すべての同期`Result<T>`機能は完璧に動作
- 非同期拡張メソッドは使用不可

## コアコンセプト

### `Result<T>` 使用法

`Result<T>`は、**成功**（Success）または**失敗**（Failure）を型で表現し、if/elseとtry/catchの地獄から解放します。

このようなコードを見たことがありますか？ ifの中にtryがあり、その中にまたif-elseがあるコード...
成功ロジック、失敗ロジック、例外処理、デフォルト値の割り当てがスパゲッティのように絡み合い、どこから読めばいいのか途方に暮れます。新しい検証ロジックを1つ追加するだけで、地獄はますます深くなり、最終的には誰も触りたくないコードが誕生します。

#### 従来のC#方式

```csharp
using UnityEngine;

public class LoginSample : MonoBehaviour
{
    void Start()
    {
        var userId = PlayerPrefs.GetString("userId");

        if (string.IsNullOrWhiteSpace(userId))
        {
            Debug.LogError("ログイン失敗: 入力が空です");
            userId = "guest";
        }
        else
        {
            try
            {
                if (!ValidateAccount(userId))
                {
                    Debug.LogWarning("ログイン失敗: ユーザーが存在しません");
                    userId = "guest";
                }
                else
                {
                    Debug.Log($"ログイン成功: {userId}");
                    LogUser(userId);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ログイン中に例外発生: {ex.Message}");
                userId = "guest";
            }
        }
    }

    bool ValidateAccount(string id) => id == "player42";

    void LogUser(string id) => Debug.Log($"認証パイプラインが{id}を許可しました");
}
```

- 状態チェック、例外処理、デフォルト値復旧ロジックがif/elseとtry/catchに散在し、分岐が複雑です。
- 失敗ケースが増えると分岐数が幾何級数的に増加し、保守が困難になります。

#### UniFP `Result<T>`でリファクタリング

UniFPでこの問題を解決しましょう。UniFPはすべての分岐と例外処理を1つのコンベアベルトに乗せ、明示的な成功/失敗を示します。
データは成功という高速道路に沿って直進し、どこかで問題が発生すれば、すぐに失敗という非常車線に抜けます。コードは上から下へ水が流れるように読め、各ステップが何をするのかが明確に見えます。

```csharp
// 良い例: すべてのステップが明確にチェーンされます。
using UniFP;
using UnityEngine;

public class LoginSample : MonoBehaviour
{
    void Start()
    {
        var loginResult = Result.FromValue(PlayerPrefs.GetString("userId"))
            // 1. 有効な入力か？（でなければInvalidInput非常車線へ）
            .Filter(DelegateCache.IsNotNullOrWhitespace, ErrorCode.InvalidInput)
            // 2. アカウントは存在するか？（でなければNotFound非常車線へ）
            .Then(id => ValidateAccount(id)
                ? Result<string>.Success(id)
                : Result<string>.Failure(ErrorCode.NotFound))
            // 3. （成功高速道路を走っている間だけ）ログを残す。
            .Do(LogUser)
            // 🚨 非常車線に抜けたら、最終目的地は"guest"だ。
            .Recover(_ => "guest");

        // 最終結果に応じて最後の処理
        loginResult.Match(
            onSuccess: id => Debug.Log($"ログイン成功: {id}"),
            onFailure: code => Debug.LogError($"ログイン失敗: {code}"));
    }

    bool ValidateAccount(string id) => id == "player42";
    void LogUser(string id) => Debug.Log($"認証パイプラインが{id}を許可しました");
}
```

- 各ステップが明示的にチェーンされ、成功/失敗フローが一目でわかります。
- 失敗時は自動的に`Recover`分岐に移動するため、例外とデフォルト値復旧ロジックが分離されます。
- 追加の検証や非同期呼び出しも`Then`、`Filter`、`ThenAsync`などで簡単に拡張できます。

#### Result作成

```csharp
using UniFP;

// 1. Success / Failureで直接作成
var success = Result<int>.Success(42);
var failure = Result<int>.Failure(ErrorCode.NotFound);
var failureWithMsg = Result<int>.Failure(ErrorCode.ValidationFailed, "年齢は0より大きい必要があります");

// 2. FromValueで値をResultに昇格
var fromValue = Result.FromValue(userId);

// 3. Tryで例外をResultに変換
var parseResult = Result.Try(() => int.Parse(input));
var parseWithCode = Result.Try(() => int.Parse(input), ErrorCode.InvalidInput);
```

#### コアメソッド: Then, Map, Filter

```csharp
// Then: Result<T>を返す関数のチェーン
Result<User> LoadUser(int id) => /* ... */;
var result = Result.FromValue(42)
    .Then(LoadUser);  // int -> Result<User>

// Map: 通常の値を返す関数の変換
var doubled = Result.FromValue(10)
    .Map(x => x * 2);  // int -> int（自動的にResult<int>にラップ）

// Filter: 条件検証（条件失敗時はFailure）
var validated = Result.FromValue(age)
    .Filter(x => x >= 18, ErrorCode.ValidationFailed, "成人のみ可能です");
```

> **💡 ヒント: Then vs Map**
> - `Then`はResultを返す関数に使用（失敗する可能性のある操作）
> - `Map`は通常の値を返す関数に使用（単純な変換）

#### エラー処理と復旧

```csharp
// Recover: 失敗をデフォルト値で復旧
var withDefault = LoadConfig()
    .Recover(code => DefaultConfig);

// IfFailed: 失敗時に代替パイプラインを実行
var cached = LoadFromServer()
    .IfFailed(() => LoadFromCache());

// Catch: 特定のエラーのみを捕捉して復旧
var result = LoadResource()
    .Catch(ErrorCode.NotFound, () => CreateDefault());

// Match: 成功/失敗に応じて異なる処理
result.Match(
    onSuccess: user => Debug.Log($"ようこそ、{user.Name}"),
    onFailure: code => Debug.LogError($"ロード失敗: {code}"));
```

#### 副作用

```csharp
// Do: 成功時のみ副作用を実行（失敗時はスキップ）
var result = LoadUser(id)
    .Do(user => Analytics.Track("UserLoaded", user.Id))
    .Do(user => Debug.Log($"ロード済み: {user.Name}"));

// DoStrict: 副作用が失敗したらパイプライン中断
var saved = CreateUser(data)
    .DoStrict(user => SaveToDatabase(user));  // DB保存失敗時は全体失敗

// IfFailed: 失敗時のみ副作用を実行
var result = Process()
    .IfFailed(code => Debug.LogError($"処理失敗: {code}"));
```

#### 条件付き実行

```csharp
// ThenIf / MapIf: 条件に応じて選択的に変換
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

#### 非同期Result (UniTask / Awaitable)

```csharp
using Cysharp.Threading.Tasks;  // またはusing UnityEngine;（Awaitable）

// ThenAsync: 非同期Resultチェーン
async UniTask<Result<User>> LoadUserAsync(int id)
{
    return await Result.FromValue(id)
        .Filter(x => x > 0, ErrorCode.InvalidInput)
        .ThenAsync(async id => await FetchFromAPI(id))
        .MapAsync(json => ParseUser(json))
        .FilterAsync(user => UniTask.FromResult(user.IsActive), "非アクティブユーザー");
}

// TryAsync: 例外を投げる非同期作業をResultに変換
var result = await AsyncResult.TryAsync(async () => 
{
    var response = await httpClient.GetAsync(url);
    return await response.Content.ReadAsStringAsync();
}, ErrorCode.NetworkError);

// DoAsync: 非同期副作用
var saved = await LoadUser(id)
    .DoAsync(async user => await SaveToCloud(user));
```

---

### `Option<T>` 使用法

`Option<T>`は、**値がある**（Some）または**値がない**（None）を型で表現し、nullの地獄から解放します。

Unityプロジェクトをやっていると、`null`チェックだけで数十行のコードに遭遇することがあります。
`if (foo == null)` → `else if (foo.Bar == null)` → `else if (foo.Bar.Length == 0)` …
エラーログがあちこちで飛び出し、どの分岐で`NullReferenceException`が発生したか探すのに時間を無駄にします。

#### 従来のC#方式

```csharp
public class UserProfileLoader
{
    public void Load()
    {
        var raw = PlayerPrefs.GetString("profile");

        if (string.IsNullOrEmpty(raw))
        {
            Debug.LogWarning("プロファイルなし: デフォルト値で代替");
            ApplyDefaults();
            return;
        }

        var profile = JsonUtility.FromJson<UserProfile>(raw);
        if (profile == null || profile.Items == null || profile.Items.Length == 0)
        {
            Debug.LogError("プロファイル破損: 復旧試行");
            ApplyDefaults();
            return;
        }

        Debug.Log($"プロファイルロード成功: {profile.Name}");
        Apply(profile);
    }
}
```

- `null`対策ロジックが散在しており、コアフローが見えにくい。
- 追加条件が付くほど`if`ブロックが増え、ステップを1つ忘れると即座に例外が発生。
- `if`のインデントにより、どこがコアロジックでどこがnull処理なのか一目で判断しにくい。

#### UniFPの`Option<T>`でリファクタリング

`Option<T>`は値があれば`Some`、なければ`None`で表現します。`None`の場合、以降のパイプラインは自動的にスキップされるため、nullチェックが自然に整理されます。

```csharp
using UniFP;

public class UserProfileLoader
{
    public void Load()
    {
        var profileOption = Option<string>.From(PlayerPrefs.GetString("profile"))
            // 1. 空の文字列ならNone処理
            .Filter(DelegateCache.IsNotNullOrWhitespace)
            // 2. Jsonパース結果をOptionに昇格
            .Map(raw => JsonUtility.FromJson<UserProfile>(raw))
            .Filter(result => result is { Items: { Length: > 0 } });

        profileOption.Match(
            onSome: Apply,
            onNone: ApplyDefaults);
    }
}
```

- パイプラインに沿って値を流し、どのステップで失敗したかすぐに目につく。
- `Filter`条件を追加するだけで新しい検証ロジックを安全に追加できる。
- 最後に`Match`1回で正常/デフォルトフローがはっきり分離される。

#### Option作成

```csharp
using UniFP;

// 1. Some / Noneで直接作成
var some = Option<int>.Some(42);
var none = Option<int>.None();

// 2. FromでnullableOptional値をOptionに変換（nullならNone）
var fromValue = Option<string>.From(PlayerPrefs.GetString("username"));  // nullならNone
var fromNullable = Option<int>.From(nullableInt);

// 3. Whereで条件付き変換（条件失敗時はNone）
var adult = Option<int>.From(age)
    .Where(x => x >= 18);
```

#### コアOptionメソッド

```csharp
// Map: 値変換（NoneならスキップΩ
var doubled = Option<int>.Some(10)
    .Map(x => x * 2);  // Some(20)

var stillNone = Option<int>.None()
    .Map(x => x * 2);  // None

// Bind: Optionを返す関数のチェーン
Option<User> FindUser(string name) => /* ... */;
var user = Option<string>.From(username)
    .Bind(FindUser);

// Filter: 条件検証（失敗時はNone）
var valid = Option<int>.From(input)
    .Filter(x => x > 0)
    .Filter(x => x < 100);

// Or / OrElse: Noneの時に代替値を提供
var withDefault = Option<string>.None()
    .Or(Option<string>.Some("デフォルト値"));

var fromFunc = Option<int>.None()
    .OrElse(() => Option<int>.Some(GetDefaultValue()));

// GetValueOrDefault: Optionから値を抽出
var value = someOption.GetValueOrDefault(defaultValue);
var valueOrNull = someOption.GetValueOrDefault();
```

#### OptionとResultの変換

```csharp
// Option -> Result: Noneをエラーに変換
var result = Option<User>.From(FindUser(id))
    .ToResult(ErrorCode.NotFound, "ユーザーが見つかりません");

// Result -> Option: 失敗をNoneに変換（エラーを無視）
var option = LoadConfig()
    .ToOption();  // 成功 -> Some、失敗 -> None
```

#### Matchで分岐処理

```csharp
// Match: Some/Noneに応じて異なる処理
var message = Option<User>.From(user).Match(
    onSome: u => $"ようこそ、{u.Name}",
    onNone: () => "ゲストモード");

// IfSome / IfNone: 片側のケースのみ処理
Option<Config>.From(config)
    .IfSome(c => ApplyConfig(c))
    .IfNone(() => UseDefaults());
```

#### コレクションヘルパー

```csharp
using System.Linq;

var items = new[] { 1, 2, 3, 4, 5 };

// TryFirst / TryLast: 最初/最後の要素をOptionとして
var first = items.TryFirst();  // Some(1)
var firstEven = items.TryFirst(x => x % 2 == 0);  // Some(2)
var empty = Array.Empty<int>().TryFirst();  // None

// TryFind: 条件に合う要素を探す
var found = items.TryFind(x => x > 3);  // Some(4)

// Choose: OptionコレクションからSomeのみ抽出
var options = new[] 
{ 
    Option<int>.Some(1), 
    Option<int>.None(), 
    Option<int>.Some(3) 
};
var values = options.Choose();  // [1, 3]
```

#### LINQ統合

```csharp
using System.Linq;

// Select: Mapと同じ
var doubled = Option<int>.Some(10)
    .Select(x => x * 2);  // Some(20)

// Where: Filterと同じ
var filtered = Option<int>.Some(42)
    .Where(x => x > 18);  // Some(42)

// SelectMany: Bindと同じ（LINQクエリ構文サポート）
var result = 
    from name in Option<string>.From(username)
    from user in FindUser(name)
    from profile in LoadProfile(user.Id)
    select profile;
```

---

### `NonEmpty<T>` 使用法

`NonEmpty<T>`は、**最低1つの要素**を保証するコレクションです。パーティ構成、必須スロットなど、空であってはならないドメインに適しています。

#### NonEmpty作成

```csharp
using UniFP;

// Create: 最低1つの要素で作成
var squad = NonEmpty.Create("Leader", "Support", "Tank");
var single = NonEmpty.Create(42);

// FromList: リストから変換（空の場合は失敗）
var list = new List<string> { "A", "B", "C" };
var nonEmpty = NonEmpty.FromList(list);  // Result<NonEmpty<string>>

var emptyList = new List<string>();
var failed = NonEmpty.FromList(emptyList);  // Failure（空）
```

#### NonEmptyメソッド

```csharp
// Head / Tail: 最初の要素と残り
var squad = NonEmpty.Create("Leader", "Tank", "Healer");
var leader = squad.Head;  // "Leader"（常に存在）
var others = squad.Tail;  // ["Tank", "Healer"]（IEnumerable）

// Map: すべての要素を変換
var upper = squad.Map(role => role.ToUpper());  // NonEmpty<string>

// Append / Prepend: 要素追加
var expanded = squad.Append("Mage");  // NonEmpty（依然として最低1つ）
var withNewLeader = squad.Prepend("NewLeader");

// ToList / ToArray: 通常のコレクションに変換
var list = squad.ToList();
var array = squad.ToArray();
```

#### 使用例

```csharp
// パーティシステム: 最低1人のリーダーが必須
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
        // コンパイル時に最低1人を保証
        _members.Map(p => p.ApplyBuff());
    }
}

// 設定: 最低1つのサーバーアドレスが必須
var servers = NonEmpty.Create(
    "https://primary.server.com",
    "https://backup1.server.com",
    "https://backup2.server.com"
);

var primary = servers.Head;
var fallbacks = servers.Tail;
```

---

### エラーコードと診断

UniFPは`ErrorCode`構造体で**Zero-GCエラー分類**を提供します。

#### 組み込みErrorCode

```csharp
// 0-999: UniFP予約範囲
ErrorCode.None              // 0: エラーなし
ErrorCode.Unknown           // 1: 不明なエラー
ErrorCode.InvalidInput      // 100: 無効な入力
ErrorCode.ValidationFailed  // 101: 検証失敗
ErrorCode.NotFound          // 102: 見つかりません
ErrorCode.Unauthorized      // 103: 認証が必要
ErrorCode.OperationFailed   // 104: 操作失敗
ErrorCode.Timeout           // 105: タイムアウト
ErrorCode.NetworkError      // 106: ネットワークエラー
ErrorCode.Forbidden         // 107: 権限なし
ErrorCode.InvalidOperation  // 108: 無効な操作
```

#### カスタムErrorCode

```csharp
// 1000+: ユーザー定義エラーコード
public static class GameErrors
{
    public static readonly ErrorCode InsufficientGold = 
        ErrorCode.Custom(1001, "Economy");
    
    public static readonly ErrorCode InventoryFull = 
        ErrorCode.Custom(1002, "Inventory");
    
    public static readonly ErrorCode QuestNotAvailable = 
        ErrorCode.Custom(1003, "Quest");
}

// 使用例
var result = PurchaseItem(itemId, price)
    .Filter(success => player.Gold >= price, GameErrors.InsufficientGold, 
            $"ゴールド不足: あと{price - player.Gold}必要");
```

#### ErrorCodeプロパティ

```csharp
var error = ErrorCode.NotFound;

error.Code;       // 102
error.Category;   // "Resource"
error.IsCustom;   // false（組み込みコード）

var custom = ErrorCode.Custom(2001, "Payment");
custom.Code;      // 2001
custom.Category;  // "Payment"
custom.IsCustom;  // true
```

#### 診断情報（デバッグモード）

```csharp
// EditorまたはUNIFP_DEBUG環境で自動記録
var result = LoadAsset(path)
    .Filter(asset => asset != null, ErrorCode.NotFound);

if (result.IsFailure)
{
    // 失敗時に自動的に記録された情報
    Debug.LogError($"[{result.ErrorCode.Category}] {result.Error}");
    Debug.LogError($"発生場所: {result.FilePath}:{result.LineNumber}");
    Debug.LogError($"メソッド: {result.MemberName}");
    Debug.LogError($"操作タイプ: {result.OperationType}");
    
    // 出力例:
    // [Resource] Asset not found: player_model.prefab
    // 発生場所: Assets/Scripts/AssetLoader.cs:42
    // メソッド: LoadPlayerModel
    // 操作タイプ: Filter
}
```

---

## フルエントパイプライン

`UniFP`名前空間をインポートすると、すべての拡張メソッドを利用できます。鉄道パターンで成功パスと失敗パスをきれいに分離します。

```csharp
var pipeline = Result.FromValue(request)
    .Filter(req => req.IsValid, ErrorCode.ValidationFailed)
    .Then(Persist)
    .DoStrict(SendAnalyticsEvent)
    .IfFailed(() => LoadCachedResult())
    .Trace("Purchase");
```

### 分岐制御と復旧

- `Recover(Func<ErrorCode, T>)`は失敗をデフォルト値で置き換えます。
- `IfFailed(Func<Result<T>>)`は代替パイプラインを実行します。
- `ThenIf`、`MapIf`は条件付きで追加作業を実行します。
- `DoStrict`は失敗を伝播させる必要がある副作用（分析イベント、データベース記録など）に適しています。

### 複数の結果の結合

`ResultCombinators`で独立した操作を1つの結果にまとめることができます。

```csharp
var stats = ResultCombinators.Combine(
    LoadLevelProgress(),
    LoadInventory());

var snapshot = stats.Zip(
    CalculateRewards(),
    (progress, inventory, rewards) => new PlayerSnapshot(progress, inventory, rewards));
```

### コレクションと走査

- `SelectResults`はコレクションを走査し、失敗時に即座に中断します。
- `CombineAll`は複数の`Result<T>`を`Result<List<T>>`に集めます。
- `FilterResults`、`Partition`、`Fold`、`AggregateResults`などでリスト検証と集計を実行します。
- `SpanExtensions`は`Span<T>`ベースの操作で、Burst対応コードでも追加アロケーションなしで動作します。

## 非同期サポート（UniTask / Awaitable）

UniFPは**UniTask**（推奨）および**Unity Awaitable**（Unity 6.0+）の両方で非同期作業をサポートします。

**UniTaskインストール時:**
```csharp
using Cysharp.Threading.Tasks;

async UniTask<Result<PlayerData>> FetchPlayer(int id)
{
    return await Result.TryFromResult(() => ValidateId(id))
        .ThenAsync(async _ => await Api.GetPlayer(id))
        .MapAsync(payload => payload.ToPlayerData())
        .FilterAsync(data => UniTask.FromResult(data.IsActive), "アクティブでないプレイヤーです");
}
```

**Unity 6.0+（Awaitable）使用時:**
```csharp
using UnityEngine;

async Awaitable<Result<PlayerData>> FetchPlayer(int id)
{
    return await Result.TryFromResult(() => ValidateId(id))
        .ThenAsync(async _ => await Api.GetPlayer(id))
        .MapAsync(payload => payload.ToPlayerData())
        .FilterAsync(data => Awaitable.FromResult(data.IsActive), "アクティブでないプレイヤーです");
}
```

両方式とも同じAPIを提供します - 非同期型を変えるだけです！

例外を投げる非同期作業は`AsyncResult.TryAsync`でラップすれば自動的に`Result`失敗に変換されます。

## 復元力ユーティリティ

- `Retry`、`RetryAsync`は指定回数だけ再試行を実行します。
- `RetryWithBackoff`は指数バックオフ遅延を適用して不安定なサービスを扱います。
- `Repeat`、`RepeatAsync`はN回連続成功が必要なシナリオを処理します。
- `Catch`は特定の失敗メッセージを捕捉して代替ロジックを実行します。

```csharp
var response = await RetryExtensions.RetryWithBackoff(
    () => Api.SendAsync(payload),
    maxAttempts: 5,
    initialDelayMilliseconds: 200,
    backoffMultiplier: 2.5f);
```

## デバッグと可視性

- `SafeExecutor`は各操作の位置と種類を記録します。
- `PipelineDebug.Trace`、`TraceWith`、`TraceOnFailure`、`Breakpoint`でパイプライン状態をコンソールで追跡します。
- `OperationType`列挙型でどのステップ（`Map`、`Filter`、`Then`など）で失敗したかすぐに確認できます。

```csharp
var result = LoadConfig()
    .Trace("Config")
    .Assert(cfg => cfg.Version >= 2, "Config version too old")
    .Breakpoint();
```

## パフォーマンスツールキット

基本的な使用では大きな問題はありませんが、Updateステートメントのようにフレームごとに実行される必要があるロジックの場合、以下の最適化が必要です。

- **DelegateCache**: 頻繁に使用されるラムダを静的キャッシュで再利用します。
- **ResultPool & ListPool<T>**: 結果コレクションをプーリングして高頻度ループでGCを排除します。
- **SpanExtensions**: スタックまたはプールバッファベースの変換を提供します。
- **Zero-allocationエラーフロー**: `ErrorCode`、`OperationType`、構造体モナドでヒープ使用量を抑制します。

## サンプルシーンとテスト

- `Assets/Scenes`
  - `01_BasicResultExample` — Result基礎
  - `02_PipelineExample` — チェーンパターン
  - `04_AsyncExample` — UniTask連携非同期フロー
  - `06_PerformanceExample` — ゼロアロケーション技法
  - `10_RealWorld_UserLogin` — 堅牢なログインパイプライン
  - `11_RealWorld_ItemPurchase` — サービス間鉄道処理
- テストは`src/UniFP/Assets/Tests`にあり、検証、非同期失敗、再試行シナリオなどの主要エッジケースを扱います。

リポジトリルートからテストを実行するには、次のコマンドを使用してください。

```bash
dotnet test src/UniFP/UniFP.Tests.csproj
```

## ドキュメント

拡張ガイドは[`docs`](./docs)フォルダで確認できます。

- [クイックスタート](./docs/getting-started.md)
- [APIリファレンス](./docs/api-reference.md)
- [サンプル集](./docs/examples.md)
- [ベストプラクティス](./docs/best-practices.md)

## 貢献

イシュー登録とプルリクエストはいつでも歓迎します。変更を提出する前にテストとサンプルも作成してください。

## ライセンス

UniFPは[MITライセンス](./LICENSE)に従います。
