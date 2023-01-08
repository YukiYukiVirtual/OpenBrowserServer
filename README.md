# VRChatOpenBrowser

## このツールで出来ること
- このシステムに対応したワールドで、ブラウザを開くスイッチを押すと、デスクトップ上でWebページが開かれます。(https://yukiyukivirtual.booth.pm/items/2028769)
- ワールドで設定した鍵と一致する鍵を持っている時だけ動くギミックを使えます。(https://yukiyukivirtual.booth.pm/items/4160532)

## セットアップ
初めて使うときのみ、下記の手順を実施してから、exeを実行します。次回以降は、パソコンを起動したときに自動的に起動されるようになります。

1. VRChatOpenBrowser.exeのショートカットを作成する。
2. Windowsキー+Rで開く窓に「shell:startup」と打ち込む。
3. 開いたフォルダにショートカットを置く。
4. VRChatを起動して、SettingsからALLOW UNTRUSTED URLSを有効にする。

***※ALLOW UNTRUSTED URLSを有効にしないと動作しません。確実に実施してください。***

## 操作方法
タスクトレイの雪アイコンから各種操作できます。

- アイコンをダブルクリックすると、フォルダを開きます。アイコンを右クリック「フォルダを開く」でもフォルダを開くことができます。
- アイコンを右クリック「アップロードチェック」で、アプリのアップデートをチェックできます。設定ファイルの更新もこのメニューからします。
- アイコンを右クリック「終了」で、このアプリを終了します。

## アプリのアップデート方法
1. 最新版をダウンロードします。
2. 解凍したファイルを古いファイルに上書きしてください。
3. ログファイルを削除してください。

Boothで購入した場合は、Boothから最新版をダウンロードしてください。  
アップデートが表示されたのに、配布先にファイルがない場合は、ファイルアップ作業中かもしれませんので、少し待ってから再度確認してみてください。  
更新はDiscordで周知します。  

## 設定ファイルの更新について
「操作方法」に記述した方法以外に、アプリ起動時に設定ファイルが更新されます。許可されたドメインが追加されたときに設定ファイルを更新してください。

## 開くことができるURLについて
このアプリでは、httpsのページのみ開くことができます。  
また、*setting.yaml*に記載されているドメインのみ開くことができます。  
サブドメインはこの設定に含まれます。（例：yukiyukivirtual.booth.pmはbooth.pmに含まれます）  
新たに許可してほしいドメインがある場合は、お問い合わせください。

## ログファイルについて
ファイル名：*VRChatOpenBrowser.log*  
プログラムの実行状態や、エラーの原因を知るために出力されます。  
もしファイルサイズが大きくなりすぎた場合は、削除しても動作に問題ありません。

## keysフォルダについて
ワールドの鍵を入れておくフォルダです。ワールド作者から鍵をもらったらこのフォルダに入れてください。鍵は誰にも渡さないでください。

## PCのセキュリティについて
このアプリをダウンロード、実行時に、セキュリティソフトによってブロックされることがありますが、免責を確認の上、各自で対応してください。

## 免責
このアプリを使用して発生したいかなる障害について、製作者は責任を負いません。

## 問い合わせ
https://discord.gg/9MwqEGvdTm
不具合報告、デフォルトで許可してほしいドメイン名はここに報告お願いします。ツールが動作を停止した場合は、ツールに表示されているログもください。ツイッターでも一応受け付けます。  
イベント等で利用する際は、ぜひ教えてください！拡散に協力したいです。

