using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.CompilerServices;

static class Version
{
	public const string revision = "v5.0.0";
	public const string edition = 
	#if BOOTH
		"Booth";
	#else
		"GitHub";
	#endif
}
	
class VRChatOpenBrowser : Form
{
	static HttpListener listener; // HTTPサーバー
	static HttpsServer httpsServer;
	
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

			File.WriteAllText(Settings.filename, responseBody);
			
			int firstLineEnd = responseBody.IndexOf("\r\n");
			string version_latest = responseBody.Substring(2, firstLineEnd-2); // "# v1.2.3".IndexOf("v")
			
			if(version_latest.Equals(Version.revision))
			{
				return 1;
			}
			else
			{
				string msg = "更新があります。\n" +
				             "現在のバージョン: " + Version.revision + "\n" +
							 "最新バージョン: " + version_latest + "\n\n" +
							 "プログラムを終了しますか？\n" +
							 "(ダウンロードページを開くので手動で更新してください)" ;
				DialogResult result = MessageBox.Show(msg, "Check Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if(result == DialogResult.Yes)
				{
					OpenUrlUtil.cmdstart("."); // フォルダを開く
					// ダウンロードページを開く
					OpenUrlUtil.OpenBrowser("https://yukiyukivirtual.booth.pm/items/2539784");
					
					// アイコン、サーバーも止めて、プログラムを終了する
					c.ExitAll();
				}
			}
		}
		catch(HttpRequestException e)
		{
			Logger.LogError(e.ToString());
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
		if(ni != null) ni.Dispose();
		if(httpsServer != null) httpsServer.Stop();
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
			new ToolStripMenuItem("フォルダを開く", null, (s,e)=>{OpenUrlUtil.cmdstart(".");}, "Open Folder"),
			new ToolStripMenuItem("終了", null, (s,e)=>{
				ExitAll();
				return;
			}, "Exit"),
		});

		ni.DoubleClick += (s,e)=>{OpenUrlUtil.cmdstart(".");};
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
		Logger.LogInfo("Version: " + Version.revision);
		
		// Formアプリとしてインスタンス化
		VRChatOpenBrowser inst = new VRChatOpenBrowser();
		
		// 設定ファイル更新
		UpdateSettingFile(inst);
		
		// Boothで買ってほしい気持ち
		#if BOOTH
		{
		}
		#else
		{
			OpenUrlUtil.OpenBrowser("https://yukiyukivirtual.booth.pm/items/2539784");
		}
		#endif
		
		// ログウォッチャーを起動する
		new VRChatLogWatcher();
		
		// サーバーを起動する
		try
		{
			httpsServer = new HttpsServer(20443);
			StartServer();
		}
		catch(System.Security.Cryptography.CryptographicException e)
		{
			// pfxファイルが存在しない
			Logger.LogError(e.ToString());
			MessageBox.Show("証明書系の異常のため、Webサーバーの起動に失敗しました。\n機能が一部制限されます。", "例外");
		}
		catch(System.Net.Sockets.SocketException e)
		{
			Logger.LogError(e.ToString());
			MessageBox.Show("Webサーバーの起動に失敗しました。ポート443がすでに使用されているかもしれません。\n機能が一部制限されます。", "例外");
		}
		catch(Exception e)
		{
			Logger.LogError(e.ToString());
			MessageBox.Show("Webサーバーの起動に失敗しました。\n機能が一部制限されます。", "例外");
		}
		
		Application.Run();
	}
	// ####################################################
	// 今後消す
	// サーバーを終了させる
	static void StopServer()
	{
		if(listener != null)
		{
			listener.Stop();
			listener.Close();
		}
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
			Logger.LogError(e.ToString());
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
		OpenUrlUtil.OpenBrowser("https://www.yukiyukivirtual.net/openbrowser_kawaruyo.html");
		
		try
		{
			listener.BeginGetContext(OnRequested, null);
			HttpListenerContext context = listener.EndGetContext(ar);
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;
			
			string apipath = GetAPIPath(request.RawUrl);
			
			Logger.LogTrace("HTTP Method: " + request.HttpMethod + "RawURL: " + request.RawUrl + "ApiPath: " + apipath );
			
			switch(apipath)
			{
			case "keys" :
				ProcessKeys(context);
				break;
			default :
				Logger.LogWarn("API不正: " + apipath);
				break;
			}
			
			Logger.LogTrace("StatusCode: " + response.StatusCode);
				
			response.Close();
		}
		catch(Exception e)
		{
			Logger.LogError(e.ToString());
		}
		
		// メモリ使用量をログ
		Logger.LogInfo("現在のメモリ使用量: " + Environment.WorkingSet);
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
		TimeSpan ts = DateTime.Now - OpenUrlUtil.lastTime;
		
		if(ts.TotalMilliseconds > 1000) // 間隔が1000ms超え
		{
			// 次のリクエストは少なくとも3秒間隔をあけてほしい
			if(ts.TotalMilliseconds < IdlePeriod)
			{
				response.KeepAlive = false;
				response.StatusCode = 403;
				response.ContentLength64 = 0;
				
				Logger.LogWarn("呼び出し間隔が短すぎます 呼び出し間隔(ms): " + ts.TotalMilliseconds.ToString() + "必要待機時間(ms): " + IdlePeriod );
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
				
				Logger.LogWarn("短時間でリクエストが多すぎます 回数: " + _ProcessKeys_count );
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
			
			Logger.LogWarn(e.ToString());
		}
		catch(Exception e)
		{
			response.KeepAlive = false;
			response.StatusCode = 500;
			response.ContentLength64 = 0;
			
			Logger.LogWarn(e.ToString());
		}
		if(_ProcessKeys_count == 0)
		{
			// 呼び出し間隔の基準時刻を更新する
			OpenUrlUtil.lastTime = DateTime.Now;
		}
	}
	// ####################################################
}
class HttpsServer
{
	// スレッドを起動してリクエストを処理する
	// 止めるときはスレッドを停止する
	// 受け付けるパス
	// ・/ サーバーが生きていることを確認する
	// ・/keys キーファイルを返す
	// ・その他 404を返す
	private Thread serverThread;
	private X509Certificate2 cert;
	private TcpListener listener;
	public HttpsServer(int port)
	{
		cert = new X509Certificate2(GetCertificatePfxBytes(), "password");
		listener = new TcpListener(IPAddress.Loopback, port);
		listener.Start();
		serverThread = new Thread(ListenerThread);
		serverThread.Start();
		Logger.LogInfo("サーバー起動");
	}
	~HttpsServer()
	{
		Stop();
	}
	// アイコンをリソースからロードする処理
	private byte[] GetCertificatePfxBytes()
	{
		System.Reflection.Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
		Stream stream = assembly.GetManifestResourceStream("certificate.pfx");
		StreamReader reader = new System.IO.StreamReader(stream);
		// StreamをByte[]に変換してリターン
        using (MemoryStream ms = new MemoryStream())
        {
            reader.BaseStream.CopyTo(ms);
			reader.Dispose();
            return ms.ToArray();
        }
	}
	public void Stop()
	{
		if(serverThread != null) serverThread.Abort();
		if(listener != null) listener.Stop();
		Logger.LogInfo("サーバー停止");
	}
	void ListenerThread()
	{
		int requestCount = 0;
		DateTime lastTime = DateTime.Now;
		while(true)
		{
			Logger.LogTrace("Listen");
			TcpClient client = listener.AcceptTcpClient();
			Logger.LogTrace("Accept");
			SslStream stream = new SslStream(client.GetStream(), false);
			try
			{
				stream.AuthenticateAsServer(cert, false, SslProtocols.Tls12 | SslProtocols.Tls13, false);
				StreamReader sr = new StreamReader(stream);
				string line;
				int lineCount = 0;
				// httpプロトコル
				string method = "";
				string path = "";
				// httpプロトコルのパーサー
				while ((line = sr.ReadLine()) != null && !line.Equals(""))
				{
					Logger.LogTrace(line);
					if(lineCount == 0)
					{
						string[] subs = line.Split(' ');
						method = subs[0];
						path = subs[1];
					}
					lineCount++;
				}
				// 呼び出し間隔チェック
				TimeSpan ts = DateTime.Now - lastTime;
				if(ts.TotalMilliseconds > 1000) // 間隔が1000ms超え
				{
					requestCount = 0;
				}
				else // 間隔が1000ms以下
				{
					// 1000ms以下でリクエストが多すぎる？
					requestCount++;
					if(requestCount >= 5)
					{
						SetHttpResponce((HttpStatusCode)429, stream);
						Logger.LogWarn("短時間でリクエストが多すぎます 回数: " + requestCount );
						continue;
					}
				}
				// レスポンス作成
				if(path.Equals("/")) // ルート
				{
					string[] contents = {
						"VRChatOpenBrowser",
						"Version: " + Version.revision,
						"Edition: " + Version.edition,
					};
					
					SetHttpResponce(HttpStatusCode.OK, String.Join("\n", contents), stream);
				}
				else if(path.StartsWith("/keys")) // keys API
				{
					string[] subs = path.Split('/');
					if(subs.Length == 3) // ファイル名指定されている場合
					{
						try
						{
							string filename = subs[2];
							string filepath = "keys/" + filename;
							string mimetype = System.Web.MimeMapping.GetMimeMapping(filename);
							using (FileStream filestream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
							{
								int len = (int)filestream.Length;
								byte[] buf = new Byte[len];
								
								filestream.Read(buf, 0, len);
								
								if(method.Equals("GET"))
								{
									SetHttpResponce(HttpStatusCode.OK, mimetype, buf, stream);
								}
								else if(method.Equals("HEAD"))
								{
									SetHttpResponceHead(HttpStatusCode.OK, mimetype, buf, stream);
								}
							}
						}
						catch(FileNotFoundException e)
						{
							SetHttpResponce(HttpStatusCode.NotFound, stream);
							Logger.LogWarn(e.ToString());
						}
						catch(ArgumentException e)
						{
							SetHttpResponce(HttpStatusCode.NotFound, stream);
							Logger.LogWarn(e.ToString());
						}
						catch(DirectoryNotFoundException e)
						{
							SetHttpResponce(HttpStatusCode.NotFound, stream);
							Logger.LogWarn(e.ToString());
						}
						catch(Exception e)
						{
							SetHttpResponce(HttpStatusCode.InternalServerError, stream);
							Logger.LogError(e.ToString());
						}
					}
					else // ファイル名指定されてないかパスが不正
					{
						SetHttpResponce(HttpStatusCode.Forbidden, stream);
						Logger.LogWarn(path);
					}
				}
				else if(path.Equals("/favicon.ico")) // ブラウザでアクセスしたときにfavicon.icoを勝手に見るので無いよって教えてあげる
				{
					SetHttpResponce(HttpStatusCode.NotFound, stream);
				}
				else // 無いよ
				{
					SetHttpResponce(HttpStatusCode.NotImplemented, stream);
					Logger.LogWarn(path);
				}
			}
			catch (Exception e)
			{
				SetHttpResponce(HttpStatusCode.InternalServerError, stream);
				Logger.LogError(e.ToString());
			}
			finally
			{
				stream.Close();
			}
			lastTime = DateTime.Now;
		}
	}
	private string GetHTTPStatusString(HttpStatusCode statusCode)
	{
		string status;
		switch(statusCode)
		{
			case HttpStatusCode.OK:
				status = "200 OK";
				break;
			case HttpStatusCode.Forbidden:
				status = "403 Forbidden";
				break;
			case HttpStatusCode.NotFound:
				status = "404 NotFound";
				break;
			case (HttpStatusCode)429:
				status = "429 Too Many Requests";
				break;
			default:
				status = "500 Internal Server Error";
				break;
		}
		return status;
	}
	// ステータスコードだけを設定するI/F
	private void SetHttpResponce(HttpStatusCode statusCode, SslStream stream)
	{
		SetHttpResponce(statusCode, GetHTTPStatusString(statusCode), stream); // ステータスコードをコンテンツに設定する
	}
	// ステータスコードと文字列コンテンツを設定するI/F
	private void SetHttpResponce(HttpStatusCode statusCode, string content, SslStream stream)
	{
		SetHttpResponce(statusCode, "text/plain", Encoding.GetEncoding("UTF-8").GetBytes(content), stream);
	}
	// ステータスコード、Content-Type、Byteコンテンツを設定するI/F
	private void SetHttpResponce(HttpStatusCode statusCode, string contentType, byte[] content, SslStream stream)
	{
		SetHttpResponceHead(statusCode, contentType, content, stream);
		stream.Write(content);
	}
	private void SetHttpResponceHead(HttpStatusCode statusCode, string contentType, byte[] content, SslStream stream)
	{
		string[] lines = {
			"HTTP/1.0 " + GetHTTPStatusString(statusCode),
			"Conenction: close",
			"Content-Type: " + contentType,
			"Content-Length: " + content.Length,
			"\r\n",
		};
		foreach(var s in lines)
		{
			if(s.Equals("\r\n")) break;
			Logger.LogTrace(s);
		}
		string httpResponceHeader = String.Join("\r\n", lines);
		stream.Write(Encoding.GetEncoding("UTF-8").GetBytes(httpResponceHeader));
	}
}
class VRChatLogWatcher
{
	FileSystemWatcher fswatcher; // ログファイルが作成されたことを監視するやーつ
	Process observerProcess; // ログを監視するプロセス
	public VRChatLogWatcher()
	{
		// VRChat起動時のログファイルの作成を監視するために、FileSystemWatcherの初期化
		fswatcher = new FileSystemWatcher();
		fswatcher.Path = Environment.ExpandEnvironmentVariables(@"%AppData%\..\LocalLow\VRChat\VRChat"); // VRChatのログがあるフォルダを監視
		fswatcher.NotifyFilter = NotifyFilters.FileName; // ファイル名を監視
		fswatcher.Filter = "output_log_*.txt";
		fswatcher.Created += new FileSystemEventHandler(LogFileCreated); // ファイル作成を監視
		fswatcher.EnableRaisingEvents = true;

		// 最新のログファイルを監視する
		DirectoryInfo dir = new DirectoryInfo(fswatcher.Path);
		FileInfo[] fis = dir.GetFiles(fswatcher.Filter);
		if(fis.Length == 0)
		{
			Logger.LogInfo("VRChatのログファイル無し");
		}
		else
		{
			FileInfo fi = fis.OrderByDescending(p => p.LastWriteTime).ToArray()[0];
			ObserveVRChatLog(fi.DirectoryName+"\\"+fi.Name); // 最新のログファイルのログ監視をスタートする
		}
	}
	~VRChatLogWatcher()
	{
		StopObserver();
	}
	// VRChatのログ監視をスタートする
	void ObserveVRChatLog(String fullpath)
	{
		// ログファイルに書き込まれた行を監視するプロセスを起動する
		try{
			observerProcess = new Process();
			observerProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%systemroot%\system32\WindowsPowerShell\v1.0\PowerShell.exe");
			observerProcess.StartInfo.Arguments = "Get-Content -Wait -Tail 0 -Encoding UTF8 -Path " + "'" + fullpath + "'";
			observerProcess.StartInfo.CreateNoWindow = true;
			observerProcess.StartInfo.UseShellExecute = false;
			observerProcess.StartInfo.RedirectStandardOutput = true;
			observerProcess.OutputDataReceived += new DataReceivedEventHandler(LogOutputDataReceived); // イベント登録

			observerProcess.Start();
			observerProcess.BeginOutputReadLine();

			Logger.LogInfo("VRChatのログファイル監視スタートしました。 " + Path.GetFileName(fullpath));
		}
		catch(System.ComponentModel.Win32Exception e)
		{
			Logger.LogError(e.ToString());
			MessageBox.Show("起動に失敗しました。\nPowerShellがみつかりません。", "例外");
			return;
		}
		catch(Exception e)
		{
			Logger.LogError(e.ToString());
			MessageBox.Show("起動に失敗しました。", "例外");
			return;
		}
	}
	// 監視プロセスを停止する
	void StopObserver()
	{
		if(!observerProcess.HasExited)
		{
			observerProcess.Kill();
		}
		observerProcess.Close();
	}
	// ログファイルが新しく作成されたことを監視するイベントハンドラ
	void LogFileCreated(Object source, FileSystemEventArgs e)
	{
		Logger.LogInfo("new LogFile Created. " + e.FullPath);
		StopObserver(); // 監視プロセスを停止する
		ObserveVRChatLog(e.FullPath); // 作成されたファイルを監視対象にする
	}
	// ファイル書き込みイベントハンドラ
	void LogOutputDataReceived(Object source, DataReceivedEventArgs e)
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
			string LogName = "[YukiYukiVirtual/OpenURL]";
			int index = line.IndexOf(LogName);
			string rawurl = line.Substring(index + LogName.Length);
			string url = rawurl.Trim();
			Logger.LogTrace("OpenURL: " + url);
			OpenUrlUtil.TryOpenURL(url);
		}
	}
}
class OpenUrlUtil
{
	public static DateTime lastTime = DateTime.Now; // 最後にリクエストを受けた時間
	// 指定したURLを開こうとする
	// 開けなくても何もしない
	public static void TryOpenURL(string str_url)
	{
		// ブラウザを起動するかチェックする
		// 起動する場合：起動するURLをログ出力する
		// 起動しない場合：起動しない理由をログ出力する
		Logger.LogTrace(str_url);
		bool canOpen = CheckURL(str_url);
		if(canOpen)
		{
			OpenBrowser(str_url);
		}
		// 呼び出し間隔の基準時刻を更新する
		lastTime = DateTime.Now;
	}
	// 指定されたURLを開けるかチェックする
	public static bool CheckURL(string str_url)
	{
		int IdlePeriod;
		List<string> Protocol = new List<string>();
		List<string> Domain = new List<string>();

		// 設定読み込み
		if(!Settings.GetSettings())
		{
			Logger.LogError("Could not load the settings");
			return false;
		}
		IdlePeriod = Settings.IdlePeriod;
		Protocol.AddRange(Settings.Protocol);
		Domain.AddRange(Settings.Domain);
		
		
		// 呼び出し間隔チェック
		TimeSpan ts = DateTime.Now - lastTime;
		
			
		if(ts.TotalMilliseconds < IdlePeriod)
		{
			Logger.LogWarn("呼び出し間隔が短すぎます 呼び出し間隔(ms): " + ts.TotalMilliseconds.ToString() + "必要待機時間(ms): " + IdlePeriod );
			return false;
		}
		
		// Uriオブジェクトを生成する
		Uri uri;
		try{
			uri = new Uri(str_url);
		}
		catch(UriFormatException e)
		{
			Logger.LogWarn("Invalid URL: " + str_url );
			Logger.LogWarn(e.ToString());
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
				Logger.LogWarn("Not an authorized Protocol");
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
				Logger.LogWarn("Not an authorized Domain");
				return false;
			}
		}
		
		return true;
	}
	// 既定のブラウザでURLを開く
	private static SoundPlayer MySoundPlayer = new SoundPlayer("C:\\Windows\\Media\\Windows Navigation Start.wav");
	public static void OpenBrowser(string str_url)
	{
		Logger.LogTrace(str_url);
		cmdstart(str_url);
		try{
			MySoundPlayer.Play();
		}
		finally{/*ここの例外処理は重要ではない*/}
	}
	// argを開くのに適切なプログラムで開く
	public static void cmdstart(string arg)
	{
		ProcessStartInfo psi = new ProcessStartInfo(arg);
		psi.CreateNoWindow = true;
		psi.UseShellExecute = true;
		
		Process.Start(psi);
	}
}
class Logger
{
	public static void LogTrace(string body, [CallerLineNumber]int lineNumber = 0, [CallerMemberName]string memberName = "")
	{
		Log("<T>", memberName, lineNumber, body);
	}
	public static void LogInfo(string body, [CallerLineNumber]int lineNumber = 0, [CallerMemberName]string memberName = "")
	{
		Log("<I>", memberName, lineNumber,body);
	}
	public static void LogWarn(string body, [CallerLineNumber]int lineNumber = 0, [CallerMemberName]string memberName = "")
	{
		Log("<W>", memberName, lineNumber,body);
	}
	public static void LogError(string body, [CallerLineNumber]int lineNumber = 0, [CallerMemberName]string memberName = "")
	{
		Log("<E>", memberName, lineNumber,body);
	}
	public static void Log(string level, string memberName, int lineNumber, string body)
	{
		string log = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.ffff] ") + level + " " + memberName + "(" + lineNumber + "): " + body;
		Console.WriteLine(log);
		using (var writer = new StreamWriter("VRChatOpenBrowser.log", true))
		{
			writer.WriteLine(log);
		}
	}
}
// 設定を読み込むクラス
// パブリックメンバーをReadして使う
class Settings
{
	public static int IdlePeriod = 500;
	public static List<string> Protocol = new List<string>();
	public static List<string> Domain = new List<string>();
	
	public static string filename = "setting.yaml";
	// 設定ファイルを読み込む
	public static bool GetSettings()
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
			Logger.LogError(e.ToString());
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
	public static List<string> GetDataFromRaw(string str)
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
csc -t:winexe VRChatOpenBrowser.cs /win32icon:resource\\ice.ico /res:resource\\ice.ico /res:resource\\certificate.pfx -r:System.Net.Http.dll -define:
*/
