# ComiCap

Windowsのシステムトレイで動作するスクリーンキャプチャユーティリティ

## 概要

ComiCapは、Windows API (P/Invoke) を使用したスクリーンキャプチャ機能を提供するWPFアプリケーションです。システムトレイに常駐し、必要なときにスクリーンキャプチャを実行できます。

## 特徴

- システムトレイ常駐型アプリケーション
- プライマリスクリーン全体のキャプチャ
- アクティブウィンドウのキャプチャ
- Windows API（BitBlt）を使用した高速キャプチャ
- WPFによるシンプルなUI

## 動作環境

- Windows (Win32 API が必要)
- .NET Framework 4.5.2 以上

## ビルド方法

### 必要なツール

- MSBuild (.NET Framework 開発環境)
- Visual Studio 2015 以降（推奨）

### ビルドコマンド

```bash
# Debug ビルド
msbuild ComiCap.sln /p:Configuration=Debug

# Release ビルド
msbuild ComiCap.sln /p:Configuration=Release

# クリーンビルド
msbuild ComiCap.sln /t:Clean,Build /p:Configuration=Debug
```

### 出力先

- Debug: `ComiCap/bin/Debug/ComiCap.exe`
- Release: `ComiCap/bin/Release/ComiCap.exe`

## 使い方

1. `ComiCap.exe` を実行します
2. アプリケーションはシステムトレイに常駐します（メインウィンドウは表示されません）
3. システムトレイアイコンを右クリックしてメニューを表示します：
   - **表示**: キャプチャした画像を表示するウィンドウを開きます
   - **Capture**: スクリーンキャプチャを実行します
   - **終了**: アプリケーションを終了します

## 技術スタック

- **UIフレームワーク**: WPF (Windows Presentation Foundation)
- **Windows Forms統合**: NotifyIcon とシステムトレイ機能に使用
- **ネイティブAPI**: Windows API (user32.dll, gdi32.dll) via P/Invoke
- **ターゲットフレームワーク**: .NET Framework 4.5.2
- **グラフィックス**: GDI+ (`System.Drawing.Bitmap`) と WPF (`BitmapImage`) の混在

## プロジェクト構成

```
ComiCap/
├── ComiCap.sln              # ソリューションファイル
└── ComiCap/
    ├── App.xaml.cs          # アプリケーションエントリーポイント
    ├── NotifyIconWrapper.cs # システムトレイアイコン管理
    ├── CaptureImpl.cs       # スクリーンキャプチャ実装
    ├── MainWindow.xaml      # 画像表示ウィンドウ
    └── Properties/          # アセンブリ情報とリソース
```

## 参考資料

- [WindowsAPIによるスクリーンキャプチャ](https://dobon.net/vb/dotnet/graphics/screencapture.html)
- [タスクトレイで動作する常駐アプリケーション](https://garafu.blogspot.jp/2015/06/dev-tasktray-residentapplication.html)

## ライセンス

このプロジェクトのライセンスについては、プロジェクト管理者にお問い合わせください。
