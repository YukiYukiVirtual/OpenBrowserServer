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
	console.info("サーバー起動完了");
}();
console.groupEnd();

function intervalCheck(ip)
{
	// リクエスト間隔
	const interval = now - lastTime;
	lastTime = now;
	const idlePeriod = parseInt(ip);
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
	console.log({interval,idlePeriod});
	return true;
}
function protocolCheck(url, protocols)
{
	// 許可されているプロトコルかチェック
	for( const protocol of protocols)
	{
		if(url.protocol == (protocol+":"))
		{
			console.log({protocol});
			return true;
		}
	}
	return "Protocol Unmatch";
}
function domainCheck(url, domains)
{
	const hostname = url.hostname;
	// ホスト名にホワイトリストドメインがあるかチェック
	for( const domain of domains)
	{
		if(
		 hostname == domain             	// ドメイン
		 ||                             	// または
		 hostname.endsWith("." + domain)	// hoge.ドメインで終わっている
		)
		{
			console.log({domain});
			return true;
		}
	}
	return "Domain Unmatch";
}
function check(url)
{
	// 設定読み込み
	const s = loadSetting();
	if(!s)
	{
		console.error("設定の読み込みに失敗しました。設定を確認してください。");
		return "loadSetting Failure";
	}
	
	// 休止間隔
	const interval_ret = intervalCheck(s.IdlePeriod);
	if(interval_ret !== true)
	{
		return interval_ret;
	}
	
	// プロトコルチェック
	const protocol_ret = protocolCheck(url, s.Protocol);
	if(protocol_ret !== true)
	{
		return protocol_ret;
	}
	
	// ドメインチェック
	const domain_ret = domainCheck(url, s.Domain);
	if(domain_ret !== true)
	{
		return domain_ret;
	}
	
	return true;
}

// HTTP GETリクエストを処理する
app.get("/openURL/*", function(req, res)
{
	console.group("openURL");
	now = new Date();
	
	// URL
	const requrl = decodeURIComponent(req.url.substr("/openURL/".length));
	let url;
	
	try{
		url = new URL(requrl);
	}catch(e)
	{
		url = null;
	}
	
	console.log({now, requrl, url});
	
	let message = `Time: ${now}`;
	message += `\nURL: ${url}`;
	// ログ終わり
	
	if(url !== null)
	{
		// メイン処理
		console.group("Check");
		
		const check_ret = check(url);
		
		message += `\nReturnCode: ${check_ret}`;
		console.log({check_ret});
		
		console.groupEnd();
		if(check_ret === true)
		{
			// ブラウザでページを開く
			openBrowser(url);
		}
		else
		{
			console.info("要求されたURLはブロックされました。");
		}
	}
	else
	{
		message += `\nURL: ${requrl}`;
		console.info("URLではありません。");
	}
	res.header('Content-Type', 'text/plain;charset=utf-8');
	res.writeHead(404);
	res.end(message);
	
	console.groupEnd();
});
