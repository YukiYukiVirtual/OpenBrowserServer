using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;

class VRChatOpenBrowser : Form
{
	static string l_version = "v2.2.0";
	static DateTime lastTime = DateTime.Now;
	static Settings settings;
	static Settings userSettings;
	static HttpListener listener;
	
	// 設定ファイルを更新する
	private static readonly HttpClient client = new HttpClient();
	private static async void UpdateSettingFile()
	{
		// Call asynchronous network methods in a try/catch block to handle exceptions.
		try	
		{
			string uri = "https://raw.githubusercontent.com/YukiYukiVirtual/OpenBrowserServer/master/setting.yaml#" + DateTime.Now.ToString();
			System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			string responseBody = await client.GetStringAsync(uri);

			File.WriteAllText("setting.yaml", responseBody);
			
			int firstLineEnd = responseBody.IndexOf("\r\n");
			string r_version = responseBody.Substring(2, firstLineEnd-2); // "# v1.2.3".IndexOf("v")
			
			if(!r_version.Equals(l_version))
			{
				string msg = "更新があります。\n"+
				             "現在のバージョン: "+l_version+"\n"+
							 "最新バージョン: " +r_version+"\n";
				MessageBox.Show(msg, "VRChatOpenBrowser");
			}
		}
		catch(HttpRequestException e)
		{
			WriteLog(e.ToString());
		}
	}
	
	// アイコンロード
	// アイコンをリソースからロードする
	private System.Drawing.Icon LoadIcon()
	{
		System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
		Stream stream = assembly.GetManifestResourceStream("ice.ico");
		StreamReader reader = new System.IO.StreamReader(stream);
		return new System.Drawing.Icon(reader.BaseStream);
	}
	// タスクトレイにアイコンを表示する
	public VRChatOpenBrowser()
	{
		NotifyIcon ni = new NotifyIcon();
		ni.Icon = LoadIcon();
		ni.Text = "VRChatOpenBrowser";
		ni.Visible = true;
		var menu = new ContextMenuStrip();

		menu.Items.AddRange(new ToolStripMenuItem[]{
			new ToolStripMenuItem("更新をチェックしに行く", null, (s,e)=>{
				#if BOOTH
				{
					OpenBrowser("https://yukiyukivirtual.booth.pm/items/2539784");
				}
				#else
				{
					OpenBrowser("https://github.com/YukiYukiVirtual/OpenBrowserServer/releases/");
				}
				#endif
			}, "Check Update"),
			new ToolStripMenuItem("フォルダを開く", null, (s,e)=>{cmdstart(".");}, "Open Folder"),
			new ToolStripMenuItem("終了", null, (s,e)=>{
				ni.Dispose();
				StopServer();
				Application.Exit();
				return;
			}, "Exit"),
		});

		ni.DoubleClick += (s,e)=>{cmdstart(".");};
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
		
		WriteLog("Start");
		
		// 設定ファイル更新
		UpdateSettingFile();
		
		// 設定をインスタンス化
		settings = new Settings("setting.yaml");
		userSettings = new Settings("user_setting.yaml");
		if(userSettings.GetSettings() == false)
		{
			WriteLog("\"user_setting.yaml\" is not exist");
		}
		
		// Boothで買ってほしい気持ち
		#if BOOTH
		{
		}
		#else
		{
			OpenBrowser("https://yukiyukivirtual.booth.pm/items/2539784");
		}
		#endif
		
		// サーバーを起動する
		StartServer();
		
		// Formをなんかする
		new VRChatOpenBrowser();
		Application.Run();
	}
	// サーバーを終了させる
	static void StopServer()
	{
		listener.Stop();
		listener.Close();
	}
	// サーバーを起動する
	static void StartServer()
	{
		try{
			
			// サーバーを建てる
			listener = new HttpListener();
			
			// http://localhost/Temporary_Listen_Addresses/openURL/
			listener.Prefixes.Add("http://+:80/Temporary_Listen_Addresses/openURL/");
			listener.Start();
			listener.BeginGetContext(OnRequested, null);
		}
		catch(Exception e)
		{
			WriteLog(e.ToString());
			MessageBox.Show("予期しない例外が発生しました。logを参照してください。", "例外");
			return;
		}
	}
	// HTTPリクエスト処理
	static void OnRequested(IAsyncResult ar)
	{
		if(!listener.IsListening)
		{
			WriteLog("OnRequested return");
			return;
		}
		try
		{
			HttpListenerContext context = listener.EndGetContext(ar);
			listener.BeginGetContext(OnRequested, null);
			
			// リクエストとレスポンス
			HttpListenerRequest req = context.Request;
			HttpListenerResponse res = context.Response;
			
			// openURL/より後ろのURLを取得する /無しでも起動できるため、処理しておく
			string str_url = req.RawUrl.Split(new string[]{"openURL/", "openURL"}, StringSplitOptions.None)[1];
		
			// ブラウザを起動するかチェックする
			// 起動する場合：起動するURLをログ出力する
			// 起動しない場合：起動しない理由をログ出力する
			bool canOpen = CheckURL(str_url);
			if(canOpen)
			{
				OpenBrowser(str_url);
			}
			
			// HTTPレスポンス
			string outputString = "> " + str_url + "\n" + canOpen;
			byte[] content = Encoding.UTF8.GetBytes(outputString);
			
			res.OutputStream.Write(content, 0, content.Length);
			res.StatusCode = 200;
			res.Close();
		}
		catch(Exception e)
		{
			WriteLog(e.ToString());
			MessageBox.Show("予期しない例外が発生しました。logを参照してください。", "例外");
			return;
		}
	}
	// 指定されたURLを開けるかチェックする
	static bool CheckURL(string str_url)
	{
		int IdlePeriod;
		List<string> Protocol = new List<string>();
		List<string> Domain = new List<string>();

		// 設定読み込み
		if(!settings.GetSettings())
		{
			WriteLog("Could not load the settings");
			return false;
		}
		IdlePeriod = settings.IdlePeriod;
		Protocol.AddRange(settings.Protocol);
		Domain.AddRange(settings.Domain);
		
		
		// ユーザー設定読み込み
		if(userSettings.GetSettings())
		{
			// ユーザー設定を適用する
			Protocol.AddRange(userSettings.Protocol);
			Domain.AddRange(userSettings.Domain);
		}
		
		// 呼び出し間隔チェック
		TimeSpan ts = DateTime.Now - lastTime;
		if(ts.TotalMilliseconds < IdlePeriod)
		{
			WriteLog("The call interval is less than the set value", ts.TotalMilliseconds.ToString(), "set:" + IdlePeriod);
			return false;
		}
		// 呼び出し間隔の基準時刻を更新する
		lastTime = DateTime.Now;
		
		// Uriオブジェクトを生成する
		Uri uri;
		try{
			uri = new Uri(str_url);
		}
		catch(UriFormatException e)
		{
			WriteLog("Invalid URL", str_url);
			if(e==null){}
			return false;
		}
		
		// プロトコルチェック
		{
			bool p = false;
			foreach(string s in Protocol)
			{
				if(uri.Scheme == s)
				{
					p = true;
				}
			}
			if(!p)
			{
				WriteLog("Not an authorized Protocol", str_url);
				return false;
			}
		}
		
		// ドメインチェック
		{
			bool p = false;
			foreach(string s in Domain)
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
				WriteLog("Not an authorized Domain", str_url);
				return false;
			}
		}
		
		return true;
	}
	// 既定のブラウザでURLを開く
	private static SoundPlayer MySoundPlayer = new SoundPlayer("C:\\Windows\\Media\\Windows Navigation Start.wav");
	static void OpenBrowser(string str_url)
	{
		WriteLog("Open URL", str_url);
		cmdstart(str_url);
		MySoundPlayer.Play();
	}
	// argを開くのに適切なプログラムで開く
	static void cmdstart(string arg)
	{
		ProcessStartInfo psi = new ProcessStartInfo(arg);
		psi.CreateNoWindow = true;
		psi.UseShellExecute = true;
		
		Process.Start(psi);
	}
	// ログを書く
	static void WriteLog(params string[] str)
	{
		string joined = String.Join(", ", str);
		
		Console.WriteLine(joined);
		using (var writer = new StreamWriter("VRChatOpenBrowser.log", true))
		{
			writer.WriteLine(joined + " at " + DateTime.Now.ToString());
		}
	}
}


// 設定を読み込むクラス
// パブリックメンバーをReadして使う
class Settings{
	public int IdlePeriod;
	public List<string> Protocol;
	public List<string> Domain;
	
	private string filename;
	// インスタンス生成、設定の初期値を作る
	public Settings(string filename)
	{
		this.filename = filename;
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
				int index_ip = data.IndexOf("IdlePeriod:");
				int index_p  = data.IndexOf("Protocol:");
				int index_d  = data.IndexOf("Domain:");
				
				// 設定名を除去したもの
				string str_ip = data.Substring(index_ip + "IdlePeriod:".Length);
				string str_p  = data.Substring(index_p  + "Protocol:".Length);
				string str_d  = data.Substring(index_d  + "Domain:".Length);
				
				// IdlePeriodを取得する
				{
					try
					{
						int index_dec = str_ip.IndexOfAny(new char[]{'0','1','2','3','4','5','6','7','8','9'});	// 数字で始まるインデックス
						str_ip = str_ip.Substring(index_dec);	// 数字で始まる文字列
						StringReader srr = new StringReader(str_ip);	// 1行目を取得する
						IdlePeriod = Int32.Parse(srr.ReadLine());	// 1行目を取得して、数値に変換してIdlePeriodに入れようとする
					}
					catch(Exception e)	// 例外発生時は、代わりに1000を入れる
					{
						if(e == null){}
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
			Console.WriteLine(e);
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
