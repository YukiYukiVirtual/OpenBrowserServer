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

## ランタイム同梱について

.NET 10 のランタイムは Windows に標準搭載されていないため、利用者にランタイムを別途インストールさせないよう、**自己完結(self-contained)で配布する**ことにした。本体プロジェクトに `RuntimeIdentifier=win-x64` と `SelfContained=true` を設定済みで、通常のビルド/発行でもランタイム一式(約119MB・239ファイル)が成果物に含まれる。インストーラー(vdproj)には、この成果物一式を取り込むこと。
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

## リリース手順 (インストーラー)

1. 本体プロジェクトを右クリック → 発行で、Free / Booth のどちらかのプロファイルを選んで発行する
   (単一ファイル exe が `publish\VRChatOpenBrowser.exe` に出力される。両プロファイルとも同じ場所に出す)
2. インストーラーを、発行した版と同じ構成 (Free / Booth) でビルドする
3. MSI の中身は exe 1本のみ (プロジェクト出力は Exclude 設定で同梱しない)
   ショートカットはこの exe を指す

注意: 発行し直さずにインストーラーをビルドすると、前回発行した版がそのまま入る。

### build-release.bat（自動化）

リポジトリ直下の `build-release.bat` を実行すると、上記手順を Booth / Free の両方について
発行 → インストーラービルド → 配布 zip 作成まで一括で行う。

- バージョンは `VersionMaster.txt` から読む
- 成果物: リポジトリ直下に `VRChatOpenBrowser_v<Version>B.zip`（Booth）と
  `VRChatOpenBrowser_v<Version>G.zip`（Free）。zip の中身は setup.exe / .msi / README.md / LICENSE.md
- 実行開始時に、直下の古い `VRChatOpenBrowser_*.zip` は削除される（今回分だけが残る）
- 発行は `-c <構成> /p:PublishProfile=<構成>` の両方を指定する。CLI の dotnet publish は
  .pubxml 内の Configuration を適用せず Release でビルドしてしまうため（Release は BOOTH を
  定義するので、Free 版が Booth 寄りの挙動で出来てしまう）。VS の発行は .pubxml を正しく適用する
