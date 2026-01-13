# ソース構成詳細ドキュメント

このドキュメントは、ComiCapプロジェクトのソースコード構成を詳細に記述しています。コード変更時の参考資料として使用してください。

## ファイル一覧と役割

### アプリケーションエントリポイント

#### App.xaml
```xml
<Application x:Class="ComiCap.App" ...>
    <!-- StartupUri="MainWindow.xaml"> ← コメントアウト -->
```

- WPFアプリケーション定義ファイル
- **重要**: `StartupUri`がコメントアウトされており、起動時にMainWindowは自動表示されない
- リソースディクショナリは未使用

#### App.xaml.cs
```csharp
namespace ComiCap
public partial class App : Application
```

**主要メンバ:**
- `notifyIcon`: NotifyIconWrapperのインスタンス（システムトレイアイコン）

**主要メソッド:**
- `OnStartup(StartupEventArgs e)`:
  - `ShutdownMode.OnExplicitShutdown`を設定（ウィンドウを閉じてもアプリ終了しない）
  - NotifyIconWrapperを生成
- `OnExit(ExitEventArgs e)`: NotifyIconWrapperのリソースを解放

**設計ポイント:**
- 通常のWPFアプリと異なり、メインウィンドウを持たないトレイ常駐型アプリケーション
- ShutdownModeの変更により、全ウィンドウが閉じられてもアプリケーションは終了しない

### システムトレイ管理

#### NotifyIconWrapper.cs
```csharp
namespace ComiCap
public partial class NotifyIconWrapper : Component
```

**主要メンバ:**
- `main_window`: MainWindowのインスタンス（遅延生成、null許容）

**コンストラクタ:**
- `NotifyIconWrapper()`: InitializeComponent呼び出し、イベントハンドラ登録
- `NotifyIconWrapper(IContainer container)`: コンテナ対応版

**イベントハンドラ:**
- `toolStripMenuItem_Open_Click`:
  - MainWindowを新規生成してShow()
  - **注意**: 毎回新しいインスタンスを生成（既存インスタンスは破棄されない可能性）

- `toolStripMenuItem_Exit_Click`:
  - `Application.Current.Shutdown()`でアプリケーション終了

- `toolStripMenuItem_Capture_Click`:
  - スクリーン全体をキャプチャ（Graphics.CopyFromScreenを使用）
  - Bitmap → MemoryStream → BitmapImageに変換
  - main_window.image.Sourceに設定
  - **注意**: main_windowがnullの場合はNullReferenceExceptionが発生する可能性

**実装詳細:**
- Windows Forms の NotifyIcon を使用（WPFには標準のNotifyIconがないため）
- GDI+のBitmapをWPFのBitmapImageに変換する処理を実装
- Screen.PrimaryScreen.Boundsでプライマリモニタのサイズを取得

#### NotifyIconWrapper.Designer.cs
```csharp
partial class NotifyIconWrapper
```

**デザイナ生成コード（自動生成）:**

**主要コンポーネント:**
- `notifyIcon1`: System.Windows.Forms.NotifyIcon
  - Text: "ComiCap"
  - Visible: true
  - Icon: リソースから読み込み

- `contextMenuStrip1`: System.Windows.Forms.ContextMenuStrip
  - 3つのメニュー項目を含む

- `toolStripMenuItem_Open`: "Open"メニュー項目
- `toolStripMenuItem_Exit`: "Exit"メニュー項目
- `toolStripMenuItem_Capture`: "Capture"メニュー項目

**注意点:**
- このファイルはデザイナによって自動生成されるため、直接編集しない
- メニュー項目の表示テキストはここで定義されている

### スクリーンキャプチャ実装

#### CaptureImpl.cs
```csharp
namespace ComiCap
static class CaptureImpl
```

**P/Invoke定義:**

```csharp
// user32.dll
[DllImport("user32.dll")]
private static extern IntPtr GetDC(IntPtr hwnd);

[DllImport("user32.dll")]
private static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

[DllImport("user32.dll")]
private static extern IntPtr GetWindowDC(IntPtr hwnd);

[DllImport("user32.dll")]
private static extern IntPtr GetForegroundWindow();

[DllImport("user32.dll")]
private static extern int GetWindowRect(IntPtr hwnd, ref RECT lpRect);

// gdi32.dll
[DllImport("gdi32.dll")]
private static extern int BitBlt(IntPtr hDestDC, int x, int y,
    int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
```

**定数:**
- `SRCCOPY = 13369376`: BitBlt のラスタオペレーションコード（コピー）
- `CAPTUREBLT = 1073741824`: レイヤードウィンドウを含める（現在未使用）

**構造体:**
```csharp
[StructLayout(LayoutKind.Sequential)]
private struct RECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}
```

**公開メソッド:**

1. `public static Bitmap CaptureScreen()`
   - プライマリスクリーン全体をキャプチャ
   - 処理フロー:
     1. GetDC(IntPtr.Zero)でプライマリモニタのDCを取得
     2. Screen.PrimaryScreen.Boundsでサイズ取得
     3. Bitmap作成
     4. Graphics.GetHdc()でBitmapのDCを取得
     5. BitBltで画面内容をコピー
     6. リソース解放（Graphics、DC）
   - 戻り値: System.Drawing.Bitmap

2. `public static Bitmap CaptureActiveWindow()`
   - アクティブウィンドウをキャプチャ
   - 処理フロー:
     1. GetForegroundWindow()でアクティブウィンドウのハンドル取得
     2. GetWindowDC()でウィンドウのDCを取得
     3. GetWindowRect()でウィンドウサイズ取得
     4. Bitmap作成
     5. BitBltでウィンドウ内容をコピー
     6. リソース解放
   - 戻り値: System.Drawing.Bitmap

**注意点:**
- どちらのメソッドもアンマネージリソース（DC）を適切に解放している
- 現在、NotifyIconWrapperではこのクラスを使用せず、独自実装を使っている
- CAPTUREBLTフラグは定義されているが使用されていない

### UI実装

#### MainWindow.xaml
```xml
<Window x:Class="ComiCap.MainWindow" ...
    Title="MainWindow" Height="350" Width="525">
    <Grid>
        <Image x:Name="image" HorizontalAlignment="Left"
               Height="100" Margin="47,60,0,0"
               VerticalAlignment="Top" Width="100"/>
    </Grid>
</Window>
```

**レイアウト:**
- ウィンドウサイズ: 525x350
- Imageコントロール:
  - 名前: `image` (NotifyIconWrapperから参照される)
  - サイズ: 100x100
  - 位置: 左上から (47, 60)
  - **注意**: キャプチャ画像のサイズに関わらず100x100で表示される

#### MainWindow.xaml.cs
```csharp
namespace ComiCap
public partial class MainWindow : Window
```

**主要メンバ:**
- `m_bitmap`: BitmapImageフィールド
  - publicアクセス可能
  - **注意**: 現在のコードでは未使用の可能性がある

**メソッド:**
- `MainWindow()`: InitializeComponent()のみ

**設計ポイント:**
- 非常にシンプルな実装
- ロジックはほとんどNotifyIconWrapperに委譲
- imageコントロールはNotifyIconWrapperから直接アクセスされる

### プロジェクト設定

#### ComiCap.csproj

**重要な設定:**
- ToolsVersion: 14.0 (Visual Studio 2015)
- TargetFramework: .NET Framework 4.5.2
- OutputType: WinExe (Windowsアプリケーション)

**参照アセンブリ:**
- System.Drawing (GDI+用)
- System.Windows.Forms (NotifyIcon用)
- PresentationCore, PresentationFramework (WPF用)
- System.Xaml

#### Properties/AssemblyInfo.cs

**アセンブリ情報:**
- AssemblyTitle: "ComiCap"
- AssemblyCopyright: "Copyright © 2018"
- AssemblyVersion: "1.0.0.0"
- ComVisible: false

## データフロー

### キャプチャ処理のフロー

```
ユーザー操作（トレイアイコン右クリック）
    ↓
NotifyIconWrapper.toolStripMenuItem_Capture_Click
    ↓
1. new Bitmap(画面サイズ)で空のBitmap作成
    ↓
2. Graphics.FromImage()でGraphicsオブジェクト作成
    ↓
3. Graphics.CopyFromScreen()で画面をBitmapにコピー
    ↓
4. Bitmap → MemoryStream (PNG形式)
    ↓
5. MemoryStream → BitmapImage (WPF用)
    ↓
6. main_window.image.Source = BitmapImage
    ↓
画面に表示
```

### アプリケーションライフサイクル

```
アプリ起動
    ↓
App.OnStartup()
    ↓
ShutdownMode.OnExplicitShutdown設定
    ↓
NotifyIconWrapper生成（トレイアイコン表示）
    ↓
[ユーザーが"Open"選択]
    ↓
MainWindow生成・表示
    ↓
[ユーザーが"Exit"選択]
    ↓
Application.Current.Shutdown()
    ↓
App.OnExit()
    ↓
NotifyIconWrapper.Dispose()
    ↓
終了
```

## 技術的な特記事項

### WPFとWindows Formsの混在

- **NotifyIcon**: Windows Formsコンポーネント（WPFに標準NotifyIconがないため）
- **MainWindow**: WPFウィンドウ
- **相互運用**: Application.Current（WPF）からShutdown()を呼び出し

### 画像処理の二重構造

1. **GDI+ (System.Drawing)**:
   - `Bitmap`: スクリーンキャプチャの取得に使用
   - `Graphics`: 描画処理に使用

2. **WPF (System.Windows.Media.Imaging)**:
   - `BitmapImage`: WPFコントロールでの表示に使用

3. **変換処理**:
   ```csharp
   Bitmap → MemoryStream (PNG) → BitmapImage
   ```
   - 中間フォーマットとしてPNGを使用
   - `BitmapCacheOption.OnLoad`でストリームから即座に読み込み

### リソース管理

**適切に管理されているリソース:**
- CaptureImpl内のデバイスコンテキスト（GetDC/ReleaseDC）
- Graphics オブジェクト（Dispose呼び出し）
- NotifyIconWrapper（App.OnExitでDispose）

**潜在的な問題:**
- MainWindowの複数生成時のメモリリーク可能性
  - toolStripMenuItem_Open_Clickで毎回newしているが、古いインスタンスを破棄していない
- キャプチャ実行時にmain_windowがnullの場合の例外

## 既知の制限事項と改善候補

### 現在の制限

1. **画像表示サイズ固定**: MainWindowのImageコントロールが100x100固定
2. **プライマリモニタのみ**: マルチモニタ環境で他のモニタをキャプチャできない
3. **MainWindow管理**: 複数回Open選択時に複数ウィンドウが開く
4. **エラーハンドリング不足**: main_windowがnullの場合の対策がない

### コードの重複

- NotifyIconWrapper.csにスクリーンキャプチャ実装があるが、CaptureImpl.csにも実装がある
- 現状、CaptureImpl.csのメソッドは使用されていない

### 未使用コード

- `MainWindow.m_bitmap`: 定義されているが使用されていない
- `CaptureImpl.CaptureActiveWindow()`: 実装されているが呼び出されていない
- `CaptureImpl.CAPTUREBLT`: 定義されているが使用されていない

## 拡張時の推奨事項

### MainWindow管理の改善
```csharp
// 推奨パターン
private MainWindow main_window = null;

private void toolStripMenuItem_Open_Click(object sender, EventArgs e) {
    if (main_window == null || !main_window.IsLoaded) {
        main_window = new MainWindow();
    }
    main_window.Show();
    main_window.Activate();
}
```

### CaptureImplの活用
```csharp
// NotifyIconWrapper.cs内で既存のCaptureImplを使用
private void toolStripMenuItem_Capture_Click(object sender, EventArgs e) {
    if (main_window == null) return;

    Bitmap bmp = CaptureImpl.CaptureScreen();
    // 以下、BitmapImage変換処理
}
```

### エラーハンドリングの追加
```csharp
private void toolStripMenuItem_Capture_Click(object sender, EventArgs e) {
    if (main_window == null) {
        MessageBox.Show("先に「Open」でウィンドウを表示してください。");
        return;
    }
    // キャプチャ処理
}
```
