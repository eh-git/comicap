# ComiCap

Windowsのシステムトレイで動作するスクリーンキャプチャユーティリティ

## 概要

ComiCapは、Windows API (P/Invoke) を使用したスクリーンキャプチャ機能を提供するモダンなWPFアプリケーションです。システムトレイに常駐し、必要なときにスクリーンキャプチャを実行できます。

## 特徴

- システムトレイ常駐型アプリケーション
- プライマリスクリーン全体のキャプチャ
- **スライド自動キャプチャ** (v2.3.0)
  - ブラウザスライドの自動ページめくり
  - 各ページを連続キャプチャ
  - キャプチャ間隔・回数・保存形式を設定可能
  - ページめくりキーのカスタマイズ
  - 進捗表示と中断機能
- **画面領域選択キャプチャ** (v2.2.0)
  - マウスドラッグで任意の矩形領域を選択
  - 8箇所のリサイズハンドルで微調整
  - スピードボタンで素早く操作
  - キーボードショートカット対応
- アクティブウィンドウのキャプチャ
- **画像保存機能** (v2.1.0)
  - PNG/JPEG/BMP形式に対応
  - JPEG品質調整機能
  - デフォルト保存先の設定
- **設定画面** (v2.1.0)
  - 保存先フォルダの指定
  - 画像形式の選択
  - JPEG品質の調整（0-100）
- Windows API（BitBlt）を使用した高速キャプチャ
- WPFによるモダンなUI
- 最新の.NET 8.0フレームワークを使用
- エラーハンドリングとリソース管理の改善

## 動作環境

- **OS**: Windows 10/11 (Win32 API が必要)
- **ランタイム**: .NET 8.0 以上
- **開発環境**: Visual Studio 2022 または Visual Studio Code + .NET SDK 8.0

## ビルド方法

### 必要なツール

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 以上
- Visual Studio 2022（推奨）または Visual Studio Code

### .NET CLI を使用したビルド

```bash
# 依存パッケージの復元
dotnet restore ComiCap/ComiCap.csproj

# Debug ビルド
dotnet build ComiCap/ComiCap.csproj -c Debug

# Release ビルド
dotnet build ComiCap/ComiCap.csproj -c Release

# クリーンビルド
dotnet clean ComiCap/ComiCap.csproj
dotnet build ComiCap/ComiCap.csproj -c Release

# 発行（単一実行可能ファイル）
dotnet publish ComiCap/ComiCap.csproj -c Release -o publish
```

### Visual Studio Code を使用したビルド

1. Visual Studio Code でプロジェクトフォルダを開く
2. `Ctrl+Shift+B` でビルドタスクを実行
3. または、コマンドパレット（`Ctrl+Shift+P`）から「Tasks: Run Build Task」を選択

利用可能なタスク（`.vscode/tasks.json` に定義）:
- `build` - プロジェクトをビルド（デフォルト）
- `clean` - ビルド成果物をクリーン
- `rebuild` - クリーン後にビルド
- `restore` - NuGetパッケージを復元
- `publish` - Release構成で発行

### 出力先

- Debug: `ComiCap/bin/Debug/net8.0-windows/ComiCap.exe`
- Release: `ComiCap/bin/Release/net8.0-windows/ComiCap.exe`
- Publish: `publish/ComiCap.exe`

## 使い方

1. `ComiCap.exe` を実行します
2. アプリケーションはシステムトレイに常駐します（メインウィンドウは表示されません）
3. システムトレイアイコンを右クリックしてメニューを表示します：
   - **表示(O)**: キャプチャした画像を表示するウィンドウを開きます
   - **キャプチャ(C)**: スクリーン全体をキャプチャし、表示ウィンドウに画像を表示します
   - **範囲を選択してキャプチャ(R)** (v2.2.0): 透明オーバーレイで領域を選択してキャプチャします
     - マウスドラッグで矩形領域を選択
     - ハンドルをドラッグしてサイズ調整
     - スピードボタン: 全画面キャプチャ(F) / 選択範囲キャプチャ(Enter) / 保存(Ctrl+S) / キャンセル(Esc)
   - **スライド自動キャプチャ(L)** (v2.3.0): ブラウザスライドを自動でキャプチャします
     - URL、ページめくりキー、キャプチャ間隔・回数などを設定
     - ブラウザが自動起動し、指定されたキーでページめくり
     - 各ページを自動キャプチャして連番で保存
     - 進捗表示と停止ボタンで中断可能
   - **名前を付けて保存(S)** (v2.1.0): キャプチャした画像をファイルに保存します
   - **設定(T)** (v2.1.0): 保存先フォルダと画像形式を設定します
   - **終了(X)**: アプリケーションを終了します

## 技術スタック

- **UIフレームワーク**: WPF (Windows Presentation Foundation)
- **システムトレイ**: H.NotifyIcon.Wpf（純粋なWPF実装）
- **ネイティブAPI**: Windows API (user32.dll, gdi32.dll) via LibraryImport
- **ターゲットフレームワーク**: .NET 8.0-windows
- **言語機能**: C# 12.0（nullable参照型、file-scoped namespaces、LibraryImportなど）
- **グラフィックス**: GDI+ (`System.Drawing.Bitmap`) と WPF (`BitmapImage`)

### 主な依存パッケージ

- `H.NotifyIcon.Wpf` (2.1.4) - WPF用のモダンなシステムトレイアイコン実装
- `System.Drawing.Common` (8.0.10) - スクリーンキャプチャ用のBitmapサポート

## プロジェクト構成

```
ComiCap/
├── ComiCap.sln                      # ソリューションファイル
├── .vscode/                         # VS Code設定
│   ├── tasks.json                  # ビルドタスク定義
│   ├── launch.json                 # デバッグ設定
│   └── settings.json               # エディタ設定
├── documents/                       # ドキュメント
│   └── requirements.md             # 要件仕様書
├── CLAUDE.md                        # Claude Code用プロジェクトガイド
└── ComiCap/
    ├── ComiCap.csproj              # SDK-styleプロジェクトファイル
    ├── App.xaml                    # アプリケーション定義（TaskbarIcon含む）
    ├── App.xaml.cs                 # アプリケーションロジック
    ├── CaptureImpl.cs              # スクリーンキャプチャ実装（エラーハンドリング強化版）
    ├── MainWindow.xaml             # 画像表示ウィンドウ
    ├── MainWindow.xaml.cs          # ウィンドウロジック
    ├── RegionSelectorWindow.xaml   # 領域選択ウィンドウ (v2.2.0)
    ├── RegionSelectorWindow.xaml.cs# 領域選択ロジック (v2.2.0)
    ├── SlideCaptureSettingsWindow.xaml   # スライドキャプチャ設定ウィンドウ (v2.3.0)
    ├── SlideCaptureSettingsWindow.xaml.cs# スライドキャプチャ設定ロジック (v2.3.0)
    ├── SlideCaptureProgressWindow.xaml   # スライドキャプチャ進捗ウィンドウ (v2.3.0)
    ├── SlideCaptureProgressWindow.xaml.cs# 進捗表示ロジック (v2.3.0)
    ├── SlideCaptureSettings.cs     # スライドキャプチャ設定モデル (v2.3.0)
    ├── SlideCaptureTool.cs         # スライドキャプチャ実行ロジック (v2.3.0)
    ├── SettingsWindow.xaml         # 設定ウィンドウ (v2.1.0)
    ├── SettingsWindow.xaml.cs      # 設定ウィンドウロジック (v2.1.0)
    ├── ImageFormat.cs              # 画像形式列挙型 (v2.1.0)
    ├── AppSettings.cs              # アプリケーション設定モデル (v2.1.0)
    ├── SettingsManager.cs          # 設定の永続化管理 (v2.1.0)
    ├── ImageSaver.cs               # 画像保存ユーティリティ (v2.1.0)
    ├── icon.ico                    # アプリケーションアイコン
    └── Properties/
        └── AssemblyInfo.cs         # WPFテーマ情報のみ
```

## 最新化の変更点（v2.0.0）

### フレームワーク

- .NET Framework 4.5.2 → .NET 8.0-windows に移行
- 旧来のプロジェクトファイル形式 → SDK-styleプロジェクトファイル
- MSBuild 14.0 → .NET SDK 8.0

### コード品質

- C# 12.0の最新機能を使用:
  - Nullable参照型（`#nullable enable`）
  - File-scoped namespaces
  - `LibraryImport`（`DllImport`の後継）
  - Using宣言によるリソース管理の改善
- 包括的なエラーハンドリング
- より詳細なXMLドキュメントコメント

### アーキテクチャ

- Windows Forms の `NotifyIcon` → `H.NotifyIcon.Wpf` に置き換え
- 純粋なWPFアプリケーションに（Windows Forms依存を削減）
- リソース管理の改善とメモリリーク対策
- `MainWindow`のシングルトンパターン管理

### 開発体験

- Visual Studio Code完全サポート
- .NET CLI でのビルド対応
- ビルドタスクとデバッグ設定の追加
- 簡素化されたプロジェクト構造

## 画像保存機能と設定画面（v2.1.0）

### 主な機能

- **画像保存機能**
  - PNG/JPEG/BMP形式に対応
  - JPEG品質調整（0-100）
  - ファイル名の自動生成（タイムスタンプベース）
  - 重複ファイル名の自動回避（連番付与）
- **設定画面**
  - 保存先フォルダの選択（フォルダブラウザダイアログ）
  - デフォルト画像形式の選択
  - JPEG品質スライダー（JPEG選択時のみ有効）
  - デフォルト設定へのリセット機能
- **設定の永続化**
  - JSON形式で保存（`%APPDATA%\ComiCap\settings.json`）
  - アプリケーション起動時に自動読み込み

### 実装詳細

- `ImageSaver.cs`: 画像保存ロジック
- `AppSettings.cs`: 設定データモデル
- `SettingsManager.cs`: JSON永続化管理
- `ImageFormat.cs`: 画像形式列挙型
- `SettingsWindow.xaml/cs`: 設定UI

## 領域選択キャプチャ機能（v2.2.0）

### 主な機能

- **透明オーバーレイウィンドウ**
  - 画面全体を覆う透明ウィンドウ
  - 選択領域以外は半透明黒で表示
  - 選択領域は透明（背景が見える）
- **矩形選択機能**
  - マウスドラッグで任意の矩形領域を選択
  - 赤枠の点線で選択範囲を表示
  - 8箇所のリサイズハンドル（円形、赤色）
  - ハンドルをドラッグしてサイズ変更
- **スピードボタンツールバー**
  - **全画面キャプチャ(F)**: 画面全体をキャプチャ
  - **選択範囲キャプチャ(Enter)**: 選択した領域をキャプチャ
  - **保存(Ctrl+S)**: 選択領域を直接ファイルに保存
  - **キャンセル(Esc)**: 領域選択をキャンセル
- **キーボードショートカット対応**
  - `F`: 全画面キャプチャ
  - `Enter`: 選択範囲キャプチャ
  - `Ctrl+S`: 保存
  - `Esc`: キャンセル

### 実装詳細

- `RegionSelectorWindow.xaml/cs`: 領域選択UI
- `CaptureImpl.CaptureRegion()`: 指定領域のキャプチャメソッド
- マウスイベント処理（ドラッグ、リサイズ）
- オーバーレイマスク生成（選択領域に穴を開ける）

## スライド自動キャプチャ機能（v2.3.0）

### 主な機能

- **ブラウザスライドの自動キャプチャ**
  - プレゼンテーションスライド（Google Slides、PowerPointオンラインなど）を自動でキャプチャ
  - ページめくりキーを送信して自動ページ遷移
  - 各ページを連番ファイルで保存
- **設定ウィンドウ**
  - **URL**: キャプチャ対象のURL（省略時は既存ウィンドウを使用）
  - **ページめくりキー**: →/←/↓/↑/Space/PageDown/PageUp/Enter から選択
  - **キャプチャ間隔**: 100～10000ms（ページ遷移の待機時間）
  - **キャプチャ回数**: 1～1000回（0で無限ループ）
  - **保存先フォルダ**: キャプチャ画像の保存先
  - **画像形式**: PNG/JPEG/BMP
  - **JPEG品質**: 0～100
  - **ファイル名プレフィックス**: ファイル名の接頭辞（例: slide_001.png）
  - **ブラウザパス**: 使用するブラウザの実行ファイル（省略時はデフォルトブラウザ）
  - **ブラウザを最大化**: ブラウザウィンドウを最大化するか
- **進捗表示**
  - キャプチャ中の進捗をリアルタイム表示
  - プログレスバーで進捗を可視化
  - 停止ボタンでいつでも中断可能

### 実装詳細

- `SlideCaptureSettingsWindow.xaml/cs`: 設定UI
- `SlideCaptureProgressWindow.xaml/cs`: 進捗表示UI
- `SlideCaptureSettings.cs`: 設定データモデル
- `SlideCaptureTool.cs`: キャプチャ実行ロジック
  - `Process.Start`: ブラウザ起動
  - Windows API `SendInput`: キーボードイベント送信
  - `SetForegroundWindow`: ウィンドウのフォアグラウンド化
  - 非同期処理（async/await）とキャンセル対応

### 使用例

1. **Google Slidesをキャプチャ**:
   - URL: Google SlidesのプレゼンテーションURL
   - ページめくりキー: → (Right)
   - キャプチャ間隔: 1000ms
   - キャプチャ回数: 20回

2. **PowerPointオンラインをキャプチャ**:
   - URL: PowerPointオンラインのプレゼンテーションURL
   - ページめくりキー: Space
   - キャプチャ間隔: 1500ms
   - キャプチャ回数: 0（手動停止まで）

3. **既存のブラウザウィンドウをキャプチャ**:
   - URL: （空白）
   - ページめくりキー: PageDown
   - キャプチャ間隔: 2000ms
   - 先にブラウザでスライドを開いてから実行

## 参考資料

- [.NET 8.0 ドキュメント](https://learn.microsoft.com/ja-jp/dotnet/core/whats-new/dotnet-8)
- [WPF (.NET) ドキュメント](https://learn.microsoft.com/ja-jp/dotnet/desktop/wpf/)
- [WindowsAPIによるスクリーンキャプチャ](https://dobon.net/vb/dotnet/graphics/screencapture.html)
- [H.NotifyIcon GitHub](https://github.com/HavenDV/H.NotifyIcon)

## ライセンス

このプロジェクトのライセンスについては、プロジェクト管理者にお問い合わせください。
