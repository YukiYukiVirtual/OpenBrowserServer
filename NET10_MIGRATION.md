# .NET 10 移行メモ(WinForms のまま)

ブランチ `feature/net10-winforms` で行った、.NET Framework 4.8 → .NET 10 への移行内容です。
UI フレームワークは WinForms のまま、プロジェクト形式と土台だけをモダン化しました。

## 変更内容

- 全プロジェクトを SDK 形式に変換
  - 本体 `OpenBrowserServer` → `net10.0-windows`(WinExe、AssemblyName は従来どおり `VRChatOpenBrowser`)
  - `OpenBrowserServer.Component` / `OpenBrowserServer.WebServer` → `net10.0-windows`(Component が MessageBox / SoundPlayer を使うため)
  - `OpenBrowserServer.Logger` → `net10.0`
  - ヘッドレス版 `OpenBrowserServer.Headless` → `net10.0`(変更なし、今回の変換に合わせて追随)
- バージョン情報は従来どおり `Properties\AssemblyInfo.cs` を正とした(自動生成はオフ)。本体は 6.2.0.0(今回マイナーバージョンを上げた)
- 構成(Booth / Free / Debug / Release)はソリューションに残した。Free 以外では `BOOTH` シンボルが定義される(従来の定義どおり)
- NuGet は packages.config から PackageReference へ。モダン.NET に同梱されるもの(System.Text.Json など)は参照自体を削除
- `System.Web.MimeMapping` はモダン.NET に無いため、WebServer プロジェクトに最小シム `MimeMapping.cs` を追加

## 廃止したもの

- **Costura.Fody**(DLL を exe に埋め込む仕組み)と FodyWeavers.xml — 単一ファイル配布は `dotnet publish` の PublishSingleFile で代替する(下記)
- packages.config / app.config(バインディングリダイレクトのみだった) — モダン.NET では不要

## ビルドと配布

Visual Studio 2026 でソリューションを開けば、そのままビルドできます。

単一ファイル exe の発行(従来の Costura 相当):

```powershell
dotnet publish OpenBrowserServer\OpenBrowserServer.csproj -c Free -r win-x64 --self-contained true -p:PublishSingleFile=true
```

- 自己完結(--self-contained)だと約 116MB、ランタイム同梱なので利用者側に .NET のインストールは不要
- .pdb が別ファイルで残るので、配布時は exe だけ渡せばよい

## 検証済みの範囲(2026-11-15、Ubuntu VM)

- ソリューション全体のビルド成功(Free / Booth の両構成)。Windows 向けビルドを Linux から行うため `EnableWindowsTargeting` を有効化済み
- ヘッドレス版の E2E(模倣ログの判定一式)が移行後も変わらず動作
- 上記コマンドでの単一ファイル発行まで確認

## 注意点・残件

- **Windows 実機での起動確認は未実施**。ビルドは通っているが、トレイ表示・コントロールパネル・実際のブラウザ起動は VS 側か実機で一度確認してください
- 警告 CA1416(プラットフォーム互換アナライザー)が多数出るが、Windows 専用アプリなので実害のないノイズ
- 旧来の `Prefer32Bit` はモダン.NET では無効なため外した(64bit として動作する)
- インストーラー(vdproj)は今回触っていない。配布を単一ファイル exe に寄せるなら、インストーラーの要否自体を別途検討
