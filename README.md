# VRChatからURLを開くシステム
## これは何？
VRChat内でのトリガーで、既定のブラウザからWEBページを開くことを想定したソフトウェアです。

## 使い方
開きたいURLを引数にした起動用URLを、VRChat内からHTTP GETします。

```
http://localhost/Temporary_Listen_Addresses/openURL/
```

上記を起動用URLと呼びます。

起動用URLの後ろに開きたいURLを指定します。引数と呼びます。  
例:

```
http://localhost/Temporary_Listen_Addresses/openURL/https://www.yukiyukivirtual.net/?query=hoge  
```

- 引数の#以降は無視されます。#を使いたい場合は、#を%23に置き換えてください。

## 起動
exeを実行します。  
パソコン起動時に、このアプリを常に起動しておくことをおすすめします。ショートカットをスタートアップに登録しておきましょう。

1. VRChatOpenBrowser.exeのショートカットを作成する。
2. Win+Rに「shell:startup」と打ち込む。
3. 開いたフォルダにショートカットを置く。

## 終了
タスクマネージャーから終了させてください。通常は終了させる必要はありません。

## 設定
*setting.yaml* を編集します。テキストエディタで編集できます。  
デフォルト設定を追加してほしい場合は、「問い合わせ」へお願いします。

### 構文
```yaml
CheckUpdate: true
IdlePeriod: 1000
Protocol:
  - https
Domain:
  - yukiyukivirtual.net
  - booth.pm
```

### 更新確認
アプリ起動時にReleaseページを開きます。更新の確認をしてください。
ページを開きたくない場合は、trueをfalseに書き換えてください。

```yaml
CheckUpdate: false
```

### 休止期間
先のリクエストから次回のリクエストまでの時間を設定します。短い間隔で大量のリクエストが来た時に、この間隔以内のリクエストは無視されます。  
正のミリ秒単位で設定してください。不正な値を設定するとデフォルト値が適用されます（500ミリ秒）。  
この設定は保存することで即座に有効になります。

```yaml
IdlePeriod: 1000
```

### プロトコル
許可するプロトコルを指定します。デフォルトではhttpsのみ許可されています。  
httpや、その他のプロトコルを許可する場合は下記のような構文で追記してください。  
この設定は保存することで即座に有効になります。

```yaml
Protocol:
  - https
  - http
```

### ドメイン名
開くことが出来るドメイン名を指定します。  
サブドメインはこの設定に含まれます。（例：yukiyukivirtual.booth.pmはbooth.pmに含まれます）

ユーザー側で追加したい場合は下記のような構文で追記してください。
この設定は保存することで即座に有効になります。

```yaml
Domain:
  - yukiyukivirtual.net
  - booth.pm
```

## ログ
ファイル名：*VRChatOpenBrowser.log*  
プログラムの実行状態や、エラーの原因を知るために出力されます。

## 免責
このアプリを使用して発生したいかなる障害について、製作者は責任を負いません。

## 問い合わせ
https://github.com/YukiYukiVirtual/OpenBrowserServer/issues

不具合報告、デフォルトで許可してほしいドメイン名はここに報告お願いします。ツールが動作を停止した場合は、ツールに表示されているログもください。ツイッターでも一応受け付けます。

## 開発向け
コンパイルオプション `csc -t:winexe VRChatOpenBrowser.cs /win32icon:ice.ico /res:ice.ico`
