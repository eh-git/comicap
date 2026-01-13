# ComiCap

Windowsのシステムトレイで動作するスクリーンキャプチャユーティリティ

## 概要

ComiCapは、Windows API (P/Invoke) を使用したスクリーンキャプチャ機能を提供するモダンなWPFアプリケーションです。システムトレイに常駐し、必要なときにスクリーンキャプチャを実行できます。

## 特徴

- システムトレイ常駐型アプリケーション
- プライマリスクリーン全体のキャプチャ
- アクティブウィンドウのキャプチャ
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
   - **キャプチャ(C)**: スクリーンキャプチャを実行し、表示ウィンドウに画像を表示します
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
├── ComiCap.sln                 # ソリューションファイル
├── .vscode/                    # VS Code設定
│   ├── tasks.json             # ビルドタスク定義
│   ├── launch.json            # デバッグ設定
│   └── settings.json          # エディタ設定
├── CLAUDE.md                   # Claude Code用プロジェクトガイド
└── ComiCap/
    ├── ComiCap.csproj         # SDK-styleプロジェクトファイル
    ├── App.xaml               # アプリケーション定義（TaskbarIcon含む）
    ├── App.xaml.cs            # アプリケーションロジック
    ├── CaptureImpl.cs         # スクリーンキャプチャ実装（エラーハンドリング強化版）
    ├── MainWindow.xaml        # 画像表示ウィンドウ
    ├── MainWindow.xaml.cs     # ウィンドウロジック
    ├── icon.ico               # アプリケーションアイコン
    └── Properties/
        └── AssemblyInfo.cs    # WPFテーマ情報のみ
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

## 参考資料

- [.NET 8.0 ドキュメント](https://learn.microsoft.com/ja-jp/dotnet/core/whats-new/dotnet-8)
- [WPF (.NET) ドキュメント](https://learn.microsoft.com/ja-jp/dotnet/desktop/wpf/)
- [WindowsAPIによるスクリーンキャプチャ](https://dobon.net/vb/dotnet/graphics/screencapture.html)
- [H.NotifyIcon GitHub](https://github.com/HavenDV/H.NotifyIcon)

## ライセンス

このプロジェクトのライセンスについては、プロジェクト管理者にお問い合わせください。
