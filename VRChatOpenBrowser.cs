using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using System.Windows.Forms;

class VRChatOpenBrowser : Form
{
	const string version_local = "v5.0.0";
	
	static DateTime lastTime = DateTime.Now; // 最後にリクエストを受けた時間
	static Settings settings; // 設定ファイルを読み込むクラス
	static HttpListener listener; // HTTPサーバー
	
	static FileSystemWatcher fswatcher; // ログファイルが作成されたことを監視するやーつ
	static Process observerProcess; // ログを監視するプロセス
	
	private NotifyIcon ni; // タスクトレイのアイコン
	
	private static readonly HttpClient client = new HttpClient(); // 設定ファイルを更新する用のHTTPクライアント
	
	
	// 設定ファイルを更新する処理
	// アップデートがある場合は0を返します
	// アップデートがない場合は1を返します
	private static int UpdateSettingFile(VRChatOpenBrowser c)
	{
		try	
		{
			string uri = "https://raw.githubusercontent.com/YukiYukiVirtual/OpenBrowserServer/master/setting.yaml#" + DateTime.Now.ToString();
			System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			string responseBody = client.GetStringAsync(uri).Result;

			File.WriteAllText("setting.yaml", responseBody);
			
			int firstLineEnd = responseBody.IndexOf("\r\n");
			string version_latest = responseBody.Substring(2, firstLineEnd-2); // "# v1.2.3".IndexOf("v")
			
			if(version_latest.Equals(version_local))
			{
				return 1;
			}
			else
			{
				string msg = "更新があります。\n" +
				             "現在のバージョン: " + version_local + "\n" +
							 "最新バージョン: " + version_latest + "\n\n" +
							 "プログラムを終了しますか？\n" +
							 "(ダウンロードページを開くので手動で更新してください)" ;
				DialogResult result = MessageBox.Show(msg, "Check Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if(result == DialogResult.Yes)
				{
					cmdstart("."); // フォルダを開く
					// ダウンロードページを開く
					OpenBrowser("https://yukiyukivirtual.booth.pm/items/2539784");
					
					// アイコン、サーバーも止めて、プログラムを終了する
					c.ExitAll();
				}
			}
		}
		catch(HttpRequestException e)
		{
			Logger.WriteLog(e);
		}
		return 0;
	}
	
	// アイコンをリソースからロードする処理
	private System.Drawing.Icon LoadIcon()
	{
		System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
		Stream stream = assembly.GetManifestResourceStream("ice.ico");
		StreamReader reader = new System.IO.StreamReader(stream);
		System.Drawing.Icon icon = new System.Drawing.Icon(reader.BaseStream);
		reader.Dispose();
		return icon;
	}
	private void ExitAll()
	{
		ni.Dispose();
		StopServer();
		Application.Exit();
	}
	// コンストラクタ
	public VRChatOpenBrowser()
	{
		// タスクトレイを作成する
		ni = new NotifyIcon();
		ni.Icon = LoadIcon();
		ni.Text = "VRChatOpenBrowser";
		ni.Visible = true;
		ContextMenuStrip menu = new ContextMenuStrip();

		menu.Items.AddRange(new ToolStripMenuItem[]{
			new ToolStripMenuItem("設定ファイル更新・アップデートチェック", null, (s,e)=>{
				if(UpdateSettingFile(this) == 1)
				{
					MessageBox.Show("設定ファイルの更新完了！\nアップデートはありませんでした。", "Check Update");
				}
			}, "Check Update"),
			new ToolStripMenuItem("フォルダを開く", null, (s,e)=>{cmdstart(".");}, "Open Folder"),
			new ToolStripMenuItem("終了", null, (s,e)=>{
				ExitAll();
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
		
		// ログ開始
		Logger.StartBlock(Logger.LogType.Session);
		Logger.TimeLog();
		Logger.WriteLog(Logger.LogType.Version, "Version: " + version_local);
		Logger.EndBlock();
		
		
		// Formアプリとしてインスタンス化
		VRChatOpenBrowser inst = new VRChatOpenBrowser();
		
		// 設定ファイル更新
		UpdateSettingFile(inst);
		
		// 設定クラスをインスタンス化
		settings = new Settings("setting.yaml");
		
		// Boothで買ってほしい気持ち
		#if BOOTH
		{
		}
		#else
		{
			OpenBrowser("https://yukiyukivirtual.booth.pm/items/2539784");
		}
		#endif
		
		// ログウォッチャーを起動する
		WatcherInitialize();
		ObserveLatestLogFile();
		
		// サーバーを起動する
		StartServer();
		
		Application.Run();
	}
	//#########################################################################################################################
	// FileSystemWatcherの初期化
	static void WatcherInitialize()
	{
		fswatcher = new FileSystemWatcher();
		fswatcher.Path = Environment.ExpandEnvironmentVariables(@"%AppData%\..\LocalLow\VRChat\VRChat"); // VRChatのログがあるフォルダを監視
		fswatcher.NotifyFilter = NotifyFilters.FileName;
		fswatcher.Filter = "output_log_*.txt";
		fswatcher.Created += new FileSystemEventHandler(LogFileCreated);
		fswatcher.EnableRaisingEvents = true;
	}
	// 最新のログファイルを開く
	static void ObserveLatestLogFile()
	{
		DirectoryInfo dir = new DirectoryInfo(fswatcher.Path);
		FileInfo[] fis = dir.GetFiles(fswatcher.Filter);
		if(fis.Length == 0)
		{
			Logger.WriteLog(Logger.LogType.Log, "VRChatのログファイル無し");
			return;
		}
		FileInfo fi = fis.OrderByDescending(p => p.LastWriteTime).ToArray()[0];
		ObserveVRChatLog(fi.DirectoryName+"\\"+fi.Name);
	}
	// VRChatのログ監視をスタートする
	static void ObserveVRChatLog(String fullpath)
	{
		try{
			observerProcess = new Process();
			observerProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%systemroot%\system32\WindowsPowerShell\v1.0\PowerShell.exe");
			observerProcess.StartInfo.Arguments = "Get-Content -Wait -Tail 0 -Encoding UTF8 -Path " + "'" + fullpath + "'";
			observerProcess.StartInfo.CreateNoWindow = true;
			observerProcess.StartInfo.UseShellExecute = false;
			observerProcess.StartInfo.RedirectStandardOutput = true;
			observerProcess.OutputDataReceived += new DataReceivedEventHandler(LogOutputDataReceived);

			observerProcess.Start();
			observerProcess.BeginOutputReadLine();

			Logger.WriteLog(Logger.LogType.Log, "VRChatのログファイル監視スタートしました。", Path.GetFileName(fullpath));
		}
		catch(System.ComponentModel.Win32Exception e)
		{
			Logger.WriteLog(e);
			MessageBox.Show("起動に失敗しました。\n" + observerProcess.StartInfo.FileName + "がみつかりません。", "例外");
			return;
		}
		catch(Exception e)
		{
			Logger.WriteLog(e);
			MessageBox.Show("起動に失敗しました。", "例外");
			return;
		}
	}
	static void StopObserver()
	{
		if(!observerProcess.HasExited)
		{
			observerProcess.Kill();
		}
		observerProcess.Close();
	}
	static void LogOutputDataReceived(Object source, DataReceivedEventArgs e)
	{
		string line = e.Data;
		if(String.IsNullOrEmpty(line))
		{
			// NOP
			return;
		}
		// "2222.22.22 22:22:22 Log       -  [YukiYukiVirtual/OpenURL]https://"
		if(Regex.IsMatch(line, @"^(\d+)\.(\d+)\.(\d+) (\d+):(\d+):(\d+) Log +-  \[YukiYukiVirtual/OpenURL\]*"))
		{
			
			Logger.StartBlock(Logger.LogType.Observer);
			Logger.TimeLog();
			string LogName = "[YukiYukiVirtual/OpenURL]";
			int index = line.IndexOf(LogName);
			string rawurl = line.Substring(index + LogName.Length);
			string url = rawurl.Trim();
			
			TryOpenURL(url);
			Logger.EndBlock();
		}
	}
	
	static void LogFileCreated(Object source, FileSystemEventArgs e)
	{
		StopObserver();
		ObserveVRChatLog(e.FullPath);
	}
	// 指定したURLを開こうとする
	// 開けなくても何もしない
	static void TryOpenURL(string str_url)
	{
		// ブラウザを起動するかチェックする
		// 起動する場合：起動するURLをログ出力する
		// 起動しない場合：起動しない理由をログ出力する
		bool canOpen = CheckURL(str_url);
		if(canOpen)
		{
			OpenBrowser(str_url);
		}
		// 呼び出し間隔の基準時刻を更新する
		lastTime = DateTime.Now;
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
			Logger.WriteLog(Logger.LogType.Error, "Could not load the settings");
			return false;
		}
		IdlePeriod = settings.IdlePeriod;
		Protocol.AddRange(settings.Protocol);
		Domain.AddRange(settings.Domain);
		
		
		// 呼び出し間隔チェック
		TimeSpan ts = DateTime.Now - lastTime;
		
			
		if(ts.TotalMilliseconds < IdlePeriod)
		{
			Logger.WriteLog(Logger.LogType.Error,
				"呼び出し間隔が短すぎます", 
				"呼び出し間隔(ms):   " + ts.TotalMilliseconds.ToString(),
				"必要待機時間(ms): " + IdlePeriod );
			return false;
		}
		
		// Uriオブジェクトを生成する
		Uri uri;
		try{
			uri = new Uri(str_url);
		}
		catch(UriFormatException e)
		{
			Logger.WriteLog(Logger.LogType.Error,
				"Invalid URL",
				"URL: " + str_url );
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
				Logger.WriteLog(Logger.LogType.Error, "Not an authorized Protocol");
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
				Logger.WriteLog(Logger.LogType.Error, "Not an authorized Domain");
				return false;
			}
		}
		
		return true;
	}
	// 既定のブラウザでURLを開く
	private static SoundPlayer MySoundPlayer = new SoundPlayer("C:\\Windows\\Media\\Windows Navigation Start.wav");
	static void OpenBrowser(string str_url)
	{
		Logger.WriteLog(Logger.LogType.OpenBrowser, str_url);
		cmdstart(str_url);
		try{
			MySoundPlayer.Play();
		}
		finally{/*ここの例外処理は重要ではない*/}
	}
	// argを開くのに適切なプログラムで開く
	static void cmdstart(string arg)
	{
		ProcessStartInfo psi = new ProcessStartInfo(arg);
		psi.CreateNoWindow = true;
		psi.UseShellExecute = true;
		
		Process.Start(psi);
	}
	//#########################################################################################################################
	
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
			
			// 受け付けるURL
			listener.Prefixes.Add("http://localhost:21983/keys/"); // keys/鍵名でアクセス
			
			listener.Start();
			listener.BeginGetContext(OnRequested, null);
		}
		catch(Exception e)
		{
			Logger.WriteLog(e);
			MessageBox.Show("起動に失敗しました。\nポート番号21983が既に使用されているかもしれません。", "例外");
			return;
		}
	}
	// HTTPリクエスト処理
	static void OnRequested(IAsyncResult ar)
	{
		if(!listener.IsListening)
		{
			return;
		}
		Logger.StartBlock(Logger.LogType.HttpLog);
		Logger.TimeLog();
		
		try
		{
			listener.BeginGetContext(OnRequested, null);
			HttpListenerContext context = listener.EndGetContext(ar);
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;
			
			string apipath = GetAPIPath(request.RawUrl);
			
			Logger.WriteLog(Logger.LogType.Request,
				"HTTP Method: " + request.HttpMethod,
				"RawURL: " + request.RawUrl,
				"ApiPath: " + apipath );
			
			Logger.StartBlock(Logger.LogType.Log);
			
			switch(apipath)
			{
			case "keys" :
				ProcessKeys(context);
				break;
			default :
				Logger.WriteLog(Logger.LogType.Error, "Apiが不正(バグってる)", apipath);
				break;
			}
			
			Logger.EndBlock();
			
			Logger.WriteLog(Logger.LogType.Response,
				"StatusCode: " + response.StatusCode);
				
			response.Close();
		}
		catch(Exception e)
		{
			Logger.WriteLog(e);
		}
		
		// メモリ使用量をログ
		Logger.WriteLog(Logger.LogType.Log, "現在のメモリ使用量: " + Environment.WorkingSet);
		
		Logger.EndBlock();
	}
	static string GetAPIPath(string rawurl)
	{
		string[] str = rawurl.Split('/');
		Console.WriteLine(str);
		foreach(string s in str)Console.WriteLine(s);
		Console.WriteLine(str.Length-1);
		if(rawurl.IndexOf("Temporary_Listen_Addresses") != -1)
		{
			return str[2]; // /Temporary_Listen_Addresses/API (非推奨)
		}
		else
		{
			return str[1]; // /API/xxx.xyz
		}
	}
	
	private static int _ProcessKeys_count = 0; // 短時間連続リクエスト回数(この関数でしか使ってはいけないstatic変数)
	static void ProcessKeys(HttpListenerContext context)
	{
		// リクエストとレスポンス
		HttpListenerRequest request = context.Request;
		HttpListenerResponse response = context.Response;
		
		// 呼び出し間隔チェック
		int IdlePeriod = 3 * 1000;
		TimeSpan ts = DateTime.Now - lastTime;
		
		if(ts.TotalMilliseconds > 1000) // 間隔が1000ms超え
		{
			// 次のリクエストは少なくとも3秒間隔をあけてほしい
			if(ts.TotalMilliseconds < IdlePeriod)
			{
				response.KeepAlive = false;
				response.StatusCode = 403;
				response.ContentLength64 = 0;
				
				Logger.WriteLog(Logger.LogType.Error,
					"呼び出し間隔が短すぎます",
					"呼び出し間隔(ms):   " + ts.TotalMilliseconds.ToString(),
					"必要待機時間(ms): " + IdlePeriod );
				return;
			}
			else // このリクエストは3秒以上間隔をあけている=新しいリクエストだから連続回数をリセットする
			{
				_ProcessKeys_count = 0;
			}
		}
		else // 間隔が1000ms以下
		{
			// 1000ms以下でリクエストが多すぎる？
			_ProcessKeys_count++;
			if(_ProcessKeys_count >= 5)
			{
				response.KeepAlive = false;
				response.StatusCode = 403;
				response.ContentLength64 = 0;
				
				Logger.WriteLog(Logger.LogType.Error,
					"短時間でリクエストが多すぎます",
					"回数: " + _ProcessKeys_count );
				return;
			}
		}
		
		string filename = Path.GetFileName(request.RawUrl);
		string filepath = "keys/" + filename;
		string mimetype = System.Web.MimeMapping.GetMimeMapping(filename);
		
		try
		{
			using (FileStream filestream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
			{
				int len = (int)filestream.Length;
				byte[] buf = new Byte[len];
				
				filestream.Read(buf, 0, len);
				
				response.KeepAlive = false;
				response.StatusCode = 200;
				response.ContentType  = mimetype;
				response.ContentLength64 = len;
				
				if(request.HttpMethod.Equals("GET"))
				{
					response.OutputStream.Write(buf, 0, len);
				}
				else if(request.HttpMethod.Equals("HEAD"))
				{
					// NOP
				}
			}
		}
		catch(FileNotFoundException e)
		{
			response.KeepAlive = false;
			response.StatusCode = 404;
			response.ContentLength64 = 0;
			
			Logger.WriteLog(e);
		}
		catch(Exception e)
		{
			response.KeepAlive = false;
			response.StatusCode = 500;
			response.ContentLength64 = 0;
			
			Logger.WriteLog(e);
		}
		if(_ProcessKeys_count == 0)
		{
			// 呼び出し間隔の基準時刻を更新する
			lastTime = DateTime.Now;
		}
	}
}

class Logger
{
	private static int level = 0;
	public enum LogType
	{
		Session,
		Version,
		DateTime,
		Exception,
		HttpLog,
		Request,
		Log,
		OpenBrowser,
		Error,
		Response,
		Observer
	};
	private static string MakeIndent()
	{
		return new String(' ', level);
	}
	public static void StartBlock(LogType t)
	{
		string space = MakeIndent();
		WriteLog(space + "<div class='"+ t +"'>");
		level++;
	}
	public static void EndBlock()
	{
		level--;
		string space = MakeIndent();
		WriteLog(space + "</div>");
	}
	public static void TimeLog()
	{
		string space = MakeIndent();
		WriteLog(space + "<time>" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "</time>");
	}
	public static void WriteLog(Exception e)
	{
		WriteLog(LogType.Exception, HttpUtility.HtmlEncode(e));
	}
	public static void WriteLog(LogType t, params string[] strs)
	{
		string space = MakeIndent();
		for(int i=0; i<strs.Length; i++)
		{
			strs[i] = space + " " + strs[i];
		}
		
		string joined =
			  space + "<div class='" + t + "'>\n"
			+ HttpUtility.HtmlEncode(String.Join("\n", strs)) + "\n"
			+ space + "</div>";
			
		WriteLog(joined);
	}
	private static void WriteLog(string str)
	{
		Console.WriteLine(str);
		using (var writer = new StreamWriter("VRChatOpenBrowser.log", true))
		{
			writer.WriteLine(str);
		}
	}
}
// 設定を読み込むクラス
// パブリックメンバーをReadして使う
class Settings
{
	public int IdlePeriod;
	public List<string> Protocol;
	public List<string> Domain;
	
	private string filename;
	// インスタンス生成、設定の初期値を作る
	public Settings(string filename)
	{
		this.filename = filename;
		IdlePeriod = 500;
		Protocol = new List<string>();
		Domain = new List<string>();
	}
	// 設定ファイルを読み込む
	public bool GetSettings()
	{
		try
		{
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
					catch(Exception e)	// 例外発生時は、代わりに500を入れる
					{
						if(e == null){}
						IdlePeriod = 500;
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

/*
コンパイルオプション 
csc -t:winexe VRChatOpenBrowser.cs /win32icon:ice.ico /res:ice.ico /res:OpenBrowser.mp4 -r:System.Net.Http.dll -define:
*/
