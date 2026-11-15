# Ubuntu(ヘッドレス)ビルドでのテスト方法

ブランチ `feature/ubuntu-test` で追加した、Ubuntu 上で中核ロジックをテストするための仕組みの説明です。
Windows 版のコードは無傷で、プラットフォーム依存箇所だけを `UBUNTU` コンパイルシンボルで差し替えています（BOOTH / Free の切り替えと同じ流儀）。

## 構成

- `OpenBrowserServer.Headless/` — SDK 形式(net10.0)のコンソール版プロジェクト
  - Component / WebServer / Logger のソースをリンクして共有し、`UBUNTU` シンボルを定義
  - WinForms の画面(トレイ/コントロールパネル)は含まない
  - `System.Web.MimeMapping` は WebServer プロジェクト側に最小シム(MimeMapping.cs)があるので、それを一緒にコンパイルする
- `tools/fake_vrchat_log.py` — VRChat の output_log を模倣するスクリプト

## UBUNTU ビルドでの差し替え内容

| 箇所 | Windows 版 | UBUNTU 版 |
|---|---|---|
| DialogWrapper | MessageBox | コンソール出力＋確認は自動応答 |
| URLOpener.StaticOpen | 音を鳴らしてブラウザ起動 | 開かず `[URLOpener] would open: <URL>` と記録 |
| VRChat ログフォルダ | `%AppData%\..\LocalLow\VRChat\VRChat` | 環境変数 `OPENBROWSER_VRCHAT_LOG_DIR`(既定は作業フォルダ配下の FakeVRChat) |
| ログ追尾 | PowerShell の Get-Content -Wait | プロセス内スレッドでファイルを追尾 |
| Config.Download | GitHub から setting.yaml を取得 | スキップ(配置済みファイルを使う) |
| JsonDownloader | VRChat API からワールド情報を取得 | API は叩かずダミーを返す(作者ID/名前は環境変数で差し替え可能) |
| JsonDownloader | ワールドのサムネイル画像も取得 | 画像取得を省略(System.Drawing が使えないため) |
| History のパス結合 | `\` 直書き | Path.Combine |

## ビルドと実行

```bash
dotnet build OpenBrowserServer.Headless/OpenBrowserServer.Headless.csproj -c Release

mkdir -p /tmp/obs-test/work/keys /tmp/obs-test/fakelogs
OPENBROWSER_WORKING_DIR=/tmp/obs-test/work \
OPENBROWSER_SETTING_FILE=$PWD/setting.yaml \
OPENBROWSER_VRCHAT_LOG_DIR=/tmp/obs-test/fakelogs \
dotnet OpenBrowserServer.Headless/bin/Release/net10.0/OpenBrowserServer.Headless.dll
```

環境変数:

- `OPENBROWSER_WORKING_DIR` — 設定/ログ/keys の置き場所(既定は `./work`)
- `OPENBROWSER_SETTING_FILE` — 起動前に作業フォルダへコピーする setting.yaml
- `OPENBROWSER_VRCHAT_LOG_DIR` — 模倣ログの置き場所
- `OPENBROWSER_DUMMY_AUTHOR_ID` / `OPENBROWSER_DUMMY_AUTHOR_NAME` — ダミーのワールド作者(既定は `usr_00000000-0000-0000-0000-000000000000` / `DummyAuthor`)。BAN リストにある ID を指定すると BAN 判定と一時停止もテストできる

## 模倣ログでテスト

アプリ起動中に別ターミナルで:

```bash
python3 tools/fake_vrchat_log.py --log-dir /tmp/obs-test/fakelogs
```

以下が流し込まれ、アプリ側の History ログとコンソールで結果を確認できる:

1. ワールド参加(Joining wrld_…)
2. 許可ドメインへの OpenURL → `OK`＋`would open`
3. 許可外ドメイン(example.com) → `DomainError`
4. 旧インターフェイス(トークンなし) → 警告ダイアログ(コンソール表示)＋`OK`
5. http(許可外プロトコル) → `ProtocolError`

HTTP 側は curl で確認できる(リクエスト間隔の制限が 5 秒あるので連打しないこと):

```bash
curl http://localhost:21983/                 # バージョンとトークンの JSON
curl http://localhost:21983/keys/hello.txt   # keys フォルダのファイル配信
```

## 検証済みの範囲(2026-11-15、Ubuntu VM)

- ビルド成功(警告は Windows 専用フィールド未使用の 1 件のみ)
- 模倣ログ 5 シナリオ全て期待どおりの判定(OK / DomainError / ProtocolError / 旧インターフェイス警告＋OK、間隔制限の TimeSpanError も再現)
- `/` の JSON 応答、`/keys/` のファイル配信(200)と無いファイル(404)
- BAN 判定:ダミー作者を BAN リストの ID にすると警告＋システム一時停止になり、以降の OpenURL は処理されず、HTTP は 503 を返す

## 注意点

- VRChat API はダミー化したため、ワールド情報の実取得(名前・説明・サムネイル)は Ubuntu では検証できない。
- 実際のブラウザ起動、ダイアログ表示、トレイ/コントロールパネルの見た目は Ubuntu では検証できない。
