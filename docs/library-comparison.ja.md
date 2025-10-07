# 他のライブラリとの比較

## 目次

- [UniFP vs Unity-NOPE](#unifp-vs-unity-nope)
  - [パフォーマンス比較](#パフォーマンス比較)
  - [機能比較](#機能比較)
  - [詳細なメソッド比較](#詳細なメソッド比較)
  - [エラーの型付け：99%のケースで不要](#エラーの型付け99のケースで不要)
- [UniFP vs language-ext](#unifp-vs-language-ext)
  - [なぜlanguage-extをUnityで直接使わないのか？](#なぜlanguage-extをunityで直接使わないのか)
  - [機能比較](#機能比較-1)

---

## UniFP vs Unity-NOPE

### パフォーマンス比較

UniFPはNOPEのパフォーマンス問題を改善しました。

**1. Zero-GC構造体設計**
- UniFP：すべてのコア型が`readonly struct`でスタック割り当て
- Unity-NOPE：`Result<T,E>`は`readonly struct`だが、ジェネリックエラー型`E`がボクシングを引き起こす可能性

**2. デリゲートキャッシング**
- UniFP：`DelegateCache`で頻繁に使用されるラムダを再利用 → ヒープ割り当て防止
- Unity-NOPE：デリゲートキャッシングなし → Updateループで繰り返し生成

**3. ResultPool & ListPool**
- UniFP：高頻度シナリオ用の組み込みオブジェクトプーリング
- Unity-NOPE：プーリング機構なし

### 機能比較

UniFPはNOPEのすべてのコア機能を実装していますが、C#ユーザーにより親しみやすい命名を使用しています。
UniFPの`Then` = NOPEの`Bind`、UniFPの`Filter` = NOPEの`Ensure`

**ハイレベル機能比較**

| 機能 | UniFP | Unity-NOPE |
|------|-------|------------|
| Resultモナド | `Result<T>`（単一型） | `Result<T,E>`（型付きエラー） |
| Optionモナド | `Option<T>` | `Maybe<T>` |
| 非同期サポート | UniTask + Awaitable | UniTask + Awaitable |
| エラー型 | `ErrorCode`（構造体、効率的） | `E`（ジェネリック、柔軟だがボクシング可能） |
| パイプライン操作 | Then, Map, Filter, Recover, Do... | Bind, Map, Ensure, Tap, Finally... |
| リトライロジック | Retry, RetryWithBackoff, Repeat | 未サポート |
| 結果の組み合わせ | ResultCombinators (Combine, Zip...) | Result.Combine, CombineValues |
| コレクショントラバーサル | SelectResults, CombineAll, Partition | 限定的 |
| パフォーマンス最適化 | DelegateCache, Pools, Span拡張 | 基本構造体のみ |
| デバッグツール | Trace, Breakpoint, SafeExecutor | 基本Matchのみ |

### 詳細なメソッド比較

[韓国語版と同じ表の内容を日本語に翻訳]

**凡例：**
- ✅ 完全サポート
- ⚠️ 部分サポートまたは異なる名前で提供
- ❌ 未サポート

### エラーの型付け：99%のケースで不要

Unity-NOPEは`Result<T,E>`でエラーを型付けできますが、Unity�ゲーム開発では**ほとんどがオーバーエンジニアリング**です：

**なぜ型付きエラーが不要か？**
- Unityゲームロジックは主に「成功したか？失敗したか？」だけが重要
- エラーの**種類**よりも**メッセージ**の方が有用（デバッグ/ロギング時）
- 型付きエラーはジェネリックパラメータ増加 → コード複雑度上昇
- ほとんどの失敗は「リソース読み込み失敗」「検証失敗」などの単純なカテゴリ

**UniFPのアプローチ：ErrorCode構造体**
```csharp
// UniFP：効率的で明確なエラー分類
var result = LoadAsset()
    .Filter(x => x != null, ErrorCode.NotFound)
    .Then(ValidateAsset);  // ErrorCode.ValidationFailedを返す可能性

if (result.IsFailure)
{
    Debug.LogError($"[{result.ErrorCode.Category}] {result.Error}");
    // [Resource] Asset not found: player_model.prefab
}
```

**1%のケース：型安全なエラーが必要な時**

複雑なドメインロジックで本当に型付きエラーが必要な場合：

```csharp
// 方法1：カスタムErrorCode
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

// 方法2：判別共用体パターン（C# 9.0+）
public record PaymentError
{
    public record InsufficientFunds(decimal Required, decimal Available) : PaymentError;
    public record InvalidCard(string CardNumber) : PaymentError;
    public record NetworkTimeout(int Attempts) : PaymentError;
}

// ResultのErrorメッセージにシリアライズして保存
var result = payment switch
{
    PaymentError.InsufficientFunds e => 
        Result<Payment>.Failure(ErrorCode.Custom(1001, "Payment"), 
                                $"不足：あと{e.Required - e.Available}必要"),
    // ...
};
```

---

## UniFP vs language-ext

### なぜlanguage-extをUnityで直接使わないのか？

language-extは.NETエコシステム最高の関数型ライブラリですが、Unityには適していません。

**1. Unityランタイム最適化の欠如**
- language-extは汎用.NET向けに設計
- 多くの型がクラスベース → GC圧迫増加
- UnityのIL2CPP AOTコンパイルとの互換性問題の可能性

**2. 圧倒的な機能複雑性**
- 100以上のモナドとトランスフォーマー
- 高階型シミュレーション（複雑なジェネリックパターン）
- ゲーム開発に不要：Parsec、Lenses、Free monadsなど

**3. 学習曲線**
- Haskellスタイルの命名規則（`camelCase`静的関数）
- Traitシステムの複雑な抽象化
- Unity開発者に馴染みのない過剰な関数型概念

**4. パフォーマンスオーバーヘッド**
- 高度な抽象化による間接呼び出し
- Unity Profilerでホットパスの特定が困難

### 機能比較

| カテゴリ | language-ext | UniFP | Unityゲーム開発の観点 |
|----------|--------------|-------|---------------------|
| **コアモナド** | Option, Either, Try, Validation, Fin | Result, Option, NonEmpty | UniFPがUnity特化の最小セットを提供 ✅ |
| **不変コレクション** | Arr, Lst, Seq, Map, HashMap, Set... | 標準C#コレクション+拡張 | language-ext優秀だがUnityには過剰 ⚠️ |
| **非同期** | IO monad, Eff, Pipes, StreamT | AsyncResult (UniTask/Awaitable) | UniFPがUnityエコシステム統合優秀 ✅ |
| **エラー処理** | Either<L,R>, Validation<E,S>, Fin<A> | Result<T> + ErrorCode | UniFPがシンプルで明確 ✅ |
| **パーサーコンビネーター** | Parsec（完全実装） | 未サポート | ゲームに不要（language-ext勝利） ❌ |
| **レンズと光学** | 完全サポート | 未サポート | ゲームには過剰（UnrealのFPropertyなどが適切） ❌ |
| **アトミック並行性** | Atom, Ref, AtomHashMap | 未サポート | Unityはシングルスレッド中心、必要時C#標準使用 ⚠️ |
| **パフォーマンス** | 高度な抽象化によるオーバーヘッド | Zero-GC構造体、プーリング最適化 | UniFPがUnityに最適化 ✅ |
| **学習曲線** | 急峻（Haskell背景必要） | 緩やか（C# LINQ経験で十分） | UniFPがアクセシビリティ優秀 ✅ |
