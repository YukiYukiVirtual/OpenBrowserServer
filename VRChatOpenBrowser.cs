using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

/*
Main
-> 多重起動抑止処理
->StartServer起動
->Formの何かを起動
 ->コンストラクタ
  ->NotifyIconの設定
   ->アイコンロード
 ->Application.Run()

StartServer
->設定ファイル初期化
->更新確認を促す
->サーバーメイン処理
 ->HTTPサーバーを建てる
 ->起動URLを解釈する
  ->チェックする
 ->ブラウザを開く
 ->HTTPレスポンス
*/

class VRChatOpenBrowser : Form
{
	static DateTime lastTime = DateTime.Now;
	static Settings settings;
	static Thread thread;
	
	// アイコンロード
	// アイコンをリソースからロードする
	private System.Drawing.Icon LoadIcon()
	{
		// Assembly Reference
		System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();

		System.IO.Stream stream;
		System.IO.StreamReader reader;

		stream = assembly.GetManifestResourceStream("ice.ico");
		reader = new System.IO.StreamReader(stream);
		return new System.Drawing.Icon(reader.BaseStream);
	}
	// タスクトレイにアイコンを表示する
	// 将来的には右クリックでログファイル表示や更新ページの確認や設定ファイル表示や終了ができるようにしたい
	// 分らんかった
	public VRChatOpenBrowser()
	{
		NotifyIcon ni = new NotifyIcon();
		ni.Icon = LoadIcon();
		ni.Text = "VRChatOpenBrowser";
		ni.Visible = true;
		var menu = new ContextMenuStrip();

		menu.Items.AddRange(new ToolStripMenuItem[]{
			new ToolStripMenuItem("&Open Folder", null, (s,e)=>{cmdstart(".");}, "Open"),
			new ToolStripMenuItem("E&xit", null, (s,e)=>{MessageBox.Show("未実装です。タスクマネージャーから終了させてください。", "未実装機能");}, "Exit")
		});

		ni.DoubleClick += (s,e)=>{OpenBrowser("https://github.com/YukiYukiVirtual/OpenBrowserServer/releases/");};
		ni.ContextMenuStrip = menu;
	}
	// エントリーポイント
	// やることはコード内コメントの通り
	[STAThread]
	public static void Main()
	{
		// 多重起動抑止処理
		System.Threading.Mutex mutex = new System.Threading.Mutex(false, "VRChatOpenBrowser");
		if(!mutex.WaitOne(0, false))
		{
			MessageBox.Show("すでに起動しています。2つ同時には起動できません。", "多重起動禁止");
			return;
		}
			
		// HTTPサーバースレッドを起動する
		thread = new Thread(StartServer);
		thread.Start();
		
		// Formをなんかする
		new VRChatOpenBrowser();
		Application.Run();
	}
	static void StartServer()
	{
		try{
			// 設定をインスタンス化
			settings = new Settings("setting.yaml");
			
			WriteLog("Start");
			
			// CheckUpdateが有効なら更新をチェックさせる
			if(settings.GetSettings() && settings.CheckUpdate)
			{
				OpenBrowser("https://github.com/YukiYukiVirtual/OpenBrowserServer/releases/");
			}
			// サーバーメイン処理
			ServerMain();
		}
		catch(Exception e)
		{
			WriteLog(e.ToString());
			MessageBox.Show("予期しない例外が発生しました。logを参照してください。", "例外");
			return;
		}
	}
	// サーバーメイン処理
	static void ServerMain()
	{
		// サーバーを建てる
		HttpListener listener = new HttpListener();
		
		// http://localhost/Temporary_Listen_Addresses/openURL/
		listener.Prefixes.Add("http://+:80/Temporary_Listen_Addresses/openURL/");
		listener.Start();
		
		while(true)
		{
			// リクエスト待ち
			HttpListenerContext context = listener.GetContext();
			// リクエストとレスポンス
			HttpListenerRequest req = context.Request;
			HttpListenerResponse res = context.Response;
			
			// QueryURLを取得する
			string queryURL = req.RawUrl.Substring("/Temporary_Listen_Addresses/openURL/".Length);
			
				
			// ブラウザを起動するかチェックする
			// 起動する場合：起動するURLをログ出力する
			// 起動しない場合：起動しない理由をログ出力する
			bool canOpen = CheckURL(queryURL);
			if(canOpen)
			{
				OpenBrowser(queryURL);
			}
			
			// HTTPレスポンス
			string outputString = "> " + queryURL + "\n" + canOpen;
			byte[] content = Encoding.UTF8.GetBytes(outputString);
			
			res.OutputStream.Write(content, 0, content.Length);
			
			res.StatusCode = 200;
			
			res.Close();
		}
	}
	// 指定されたURLを開けるかチェックする
	static bool CheckURL(string queryURL)
	{
		// 設定読み込み
		if(!settings.GetSettings())
		{
			WriteLog("設定を読み込めませんでした。");
			return false;
		}
		
		// 呼び出し間隔チェック
		TimeSpan ts = DateTime.Now - lastTime;
		if(ts.TotalMilliseconds < settings.IdlePeriod)
		{
			WriteLog("呼び出し間隔が設定値未満です。[" + ts.TotalMilliseconds + "]");
			return false;
		}
		// 呼び出し間隔の基準時刻を更新する
		lastTime = DateTime.Now;
		
		// Uriオブジェクトを生成する
		Uri uri;
		try{
			uri = new Uri(queryURL);
		}
		catch(UriFormatException e)
		{
			WriteLog("指定されたURLが有効ではありません。[" + queryURL + "]");
			if(e==null){}
			return false;
		}
		
		// プロトコルチェック
		{
			bool p = false;
			foreach(string s in settings.Protocol)
			{
				if(uri.Scheme == s)
				{
					p = true;
				}
			}
			if(!p)
			{
				WriteLog("許可されたプロトコルではありません。[" + queryURL　+ "]");
				return false;
			}
		}
		
		// ドメインチェック
		{
			bool p = false;
			foreach(string s in settings.Domain)
			{
				if(uri.Host == s
					||
				   uri.Host.EndsWith("." + s)
				)
				{
					p = true;
				}
			}
			if(!p)
			{
				WriteLog("許可されたドメインではありません。[" + queryURL + "]");
				return false;
			}
		}
		
		return true;
	}
	// 既定のブラウザでURLを開く
	static void OpenBrowser(string queryURL)
	{
		WriteLog("ブラウザを開きます。[" + queryURL + "]");
		// "" "query"
		cmdstart("\"\" \"" + queryURL + "\"");
	}
	// start arg
	static void cmdstart(string arg)
	{
		ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c start " + arg);
		psi.CreateNoWindow = true;
		psi.UseShellExecute = false;
		
		Process.Start(psi);
	}
	// ログを書く
	static void WriteLog(string str)
	{
		Console.WriteLine(str);
		using (var writer = new StreamWriter("VRChatOpenBrowser.log", true))
		{
			writer.WriteLine(str + " at " + DateTime.Now.ToString());
		}
	}
}


// 設定を読み込むクラス
// パブリックメンバーをReadして使う
class Settings{
	public bool CheckUpdate;
	public int IdlePeriod;
	public List<string> Protocol;
	public List<string> Domain;
	
	private string filename;
	// インスタンス生成、設定の初期値を作る
	public Settings(string filename)
	{
		this.filename = filename;
		CheckUpdate = true;
		IdlePeriod = 1000;
		Protocol = new List<string>();
		Domain = new List<string>();
	}
	// 設定ファイルを読み込む
	public bool GetSettings()
	{
		try{
			// テキストファイルをUTF-8で処理する
			using(StreamReader sr = new StreamReader(filename, Encoding.GetEncoding("UTF-8") ) )
			{
				string data = sr.ReadToEnd();	// 全部取得する
				// 設定名が始まる行を探す
				int index_up = data.IndexOf("CheckUpdate:");
				int index_ip = data.IndexOf("IdlePeriod:");
				int index_p  = data.IndexOf("Protocol:");
				int index_d  = data.IndexOf("Domain:");
				
				// 設定名を除去したもの
				string str_up = data.Substring(index_up + "CheckUpdate:".Length);
				string str_ip = data.Substring(index_ip + "IdlePeriod:".Length);
				string str_p  = data.Substring(index_p  + "Protocol:".Length);
				string str_d  = data.Substring(index_d  + "Domain:".Length);
				
				// CheckUpdateを取得する
				{
					StringReader srr = new StringReader(str_up);	// 1行目を取得する
					CheckUpdate = srr.ReadLine().IndexOf("true") != -1;	// "true"が含まれているかをCheckUpdateに設定する
				}
				// IdlePeriodを取得する
				{
					int index_dec = str_ip.IndexOfAny(new char[]{'0','1','2','3','4','5','6','7','8','9'});	// 数字で始まるインデックス
					str_ip = str_ip.Substring(index_dec);	// 数字で始まる文字列
					StringReader srr = new StringReader(str_ip);	// 1行目を取得する
					if(Int32.TryParse(srr.ReadLine(), out IdlePeriod))	// 1行目を取得して、数値に変換してIdlePeriodに入れようとする
					{
					}
					if(IdlePeriod <= 0)	// 数値に変換できなければ、代わりに1000を入れる
					{
						IdlePeriod = 1000;
					}
				}
				// ProtocolとDomainを取得する
				Protocol = GetDataFromRaw(str_p);
				Domain   = GetDataFromRaw(str_d);
			}
			return true;
		}
		catch(Exception e)
		{
			if(e == null){}
			return false;
		}
	}
	// 生のyamlから設定項目のリストを取得する
	// yamlパースできるとは言っていない
	// フォーマット↓
	// 対象の設定名
	//  - 設定項目
	//  - 設定項目
	// 対象以外の設定名
	static List<string> GetDataFromRaw(string str)
	{
		List<string> list = new List<string>();	// いったんリストに入れてから最後に配列に変換する
		string line;
		StringReader sr = new StringReader(str);
		sr.ReadLine();// 設定名なので読み飛ばす
		while((line = sr.ReadLine()) != null)
		{
			int index_minus = line.IndexOf("-");	// "-"が含まれる行は設定項目ですという仕様
			if(index_minus == -1)	// "-"が含まれなければ終わる
			{
				break;
			}
			line = line.Substring(index_minus+1).Trim();	// 設定項目を抜き出す
			list.Add(line);
		}
		return list;
	}
}