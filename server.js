// グローバル変数
const SETTING = "setting.yaml";
let lastTime = 0;
let now = 0;

const exec = require("child_process").execSync;
// ブラウザを開く
function openBrowser(uri)
{
	const cmd = `start "" "${uri}"`;
	exec(cmd);
}

// HTTPサーバー
const express = require("express");
const app = express();

// ファイルシステム
const fs = require("fs");
const yaml = require("js-yaml");

// 設定を取得する関数
function loadSetting()
{
	try{
		const txt = fs.readFileSync(SETTING);
		const obj = yaml.safeLoad(txt);
		return obj;
	}
	catch(e)
	{
		console.error("設定ファイルのロードに失敗しました。ファイル名: " + SETTING);
		return false;
	}
}

console.group("初期化");
+function()
{
	const s = loadSetting();
	if(!s)
	{
		console.error("初期化に失敗しました。終了します。");
		return;
	}
	try{
		// 使用ポート設定
		const p = parseInt(s.Port);
		console.info("ポート番号: " + p);
		app.listen(p);
	}
	catch(e)
	{
		console.error("ポート設定に失敗しました。有効なポート番号を設定してください。設定されているポート番号: " + s.Port);
		return;
	}
	console.info("サーバー起動");
}();
console.groupEnd();

// 設定を読み込んで各種判定後、ブラウザを開く
function start(uri)
{
	// リクエスト間隔
	const interval = now - lastTime;
	lastTime = now;
	
	// 設定読み込み
	const s = loadSetting();
	if(!s)
	{
		console.error("設定の読み込みに失敗しました。設定を確認してください。");
		return "loadSetting Failure";
	}
	
	// 休止間隔
	const idlePeriod = parseInt(s.IdlePeriod);
	if(isNaN(idlePeriod))
	{
		console.error("IdlePeriodが不正です。");
		return "Invalid IdlePeriod";
	}
	if(idlePeriod <= 0)
	{
		console.info("IdlePeriodが0以下のため休止します。");
		console.log({idlePeriod});
		return "IdlePeriod <= 0";
	}	
	if(interval <= idlePeriod)
	{
		console.info("リクエスト間隔が設定値以下のため休止します。");
		console.log({interval,idlePeriod});
		return "IdlePeriod";
	}
	
	// ホワイトリスト
	const wl = s.WhiteList;
	if(wl == undefined)
	{
		console.error("ホワイトリストが正しく定義されていません。設定ファイルに[WhiteList:]の記述がありますか？");
		return "WhiteList Undefined";
	}
	// ホワイトリストマッチのフラグ
	let matched = false;
	for( const w of wl)
	{
		let u = w; // ホワイトリスト要素を変数uに退避
		// 末尾に/を補完する
		if(u.slice(-1) != "/")
		{
			u += "/";
		}
		// ホワイトリストに前方一致するか
		if(uri.indexOf(u) === 0)
		{
			matched = w; // ホワイトリストの要素にマッチしたことを表すため、退避前のwを設定する。
			break;
		}
	}
	if(!matched)
	{
		console.info("ホワイトリストにマッチしませんでした。");
		return "WhiteList Unmatch";
	}
	console.info("ホワイトリストにマッチしました。: " + matched);
	
	// ブラウザでページを開く
	openBrowser(uri);
	return "WhiteList Match";
}

// HTTP GETリクエストを処理する
app.get("/openURL/*", function(req, res)
{
	console.group("openURL");
	
	now = new Date();
	const uri = decodeURIComponent(req.url.substr("/openURL/".length));
	
	console.log({now, uri});
	
	let message = `Time: ${now}`;
	message += `\nURL: ${uri}`;
	
	// URIがhttpから始まっているか確認
	if(uri.indexOf("http") === 0)
	{
		// ブラウザを開く
		const rc = start(uri);
		message += `\nReturnCode: ${rc}`;
	}
	else
	{
		console.warn("httpから始まらない不正なパラメータが渡されました。");
		message += "\nInvalid Parameter.";
	}
	
	res.header('Content-Type', 'text/plain;charset=utf-8');
	res.writeHead(404);
	res.end(message);
	
	console.groupEnd();
});
