using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
#if !UBUNTU
using System.Media;
using System.Windows.Forms;
#endif
#if UBUNTU
using System.Threading;
#endif
using OpenBrowserServer.Logger;
using static OpenBrowserServer.Component.Setting;

namespace OpenBrowserServer.Component
{
    public delegate void VRChatLogCallback(string key, string line);
    public class VRChatLogWatcher
    {
        readonly Config config;
        readonly URLOpener opener; // URLを開くやつ
        readonly FileSystemWatcher fswatcher; // ログファイルが作成されたことを監視するやーつ
        readonly History history; // 履歴ログ
        
        readonly System.Timers.Timer watchdogTimer;
        readonly object watchdogLockObject = new object();

        Process observerProcess; // ログを監視するプロセス
#if UBUNTU
        CancellationTokenSource tailCts; // ログ追尾スレッドの停止用
        Thread tailThread; // ログ追尾スレッド
#endif
        public string NowWorldId { get; private set; }
        public VRChatLogWatcher(Config config, History history)
        {
            this.config = config;
            opener = new URLOpener(config);
            this.history = history;

            watchdogTimer = new System.Timers.Timer(1000*60*config.Setting.WatchdogTime); // 分単位
            watchdogTimer.Elapsed += WatchdogTimer_Elapsed;
            watchdogTimer.AutoReset = false;

            NowWorldId = null;

#if UBUNTU
            // Ubuntu(ヘッドレス)ビルド用: 環境変数で指定された偽ログフォルダを使う
            string targetDirectoryName = Environment.GetEnvironmentVariable("OPENBROWSER_VRCHAT_LOG_DIR") ?? Path.Combine(config.WorkingPath, "FakeVRChat");
            Directory.CreateDirectory(targetDirectoryName);
#else
            string targetDirectoryName = Environment.ExpandEnvironmentVariables(@"%AppData%\..\LocalLow\VRChat\VRChat");
#endif
            string targetFileName = "output_log_*.txt";

            // VRChat起動時のログファイルの作成を監視するために、FileSystemWatcherの初期化
            this.fswatcher = new FileSystemWatcher
            {
                Path = targetDirectoryName, // VRChatのログがあるフォルダを監視
                NotifyFilter = NotifyFilters.FileName, // ファイル名を監視
                Filter = targetFileName,
            };
            this.fswatcher.Created += new FileSystemEventHandler(LogFileCreated); // ファイル作成を監視
            this.fswatcher.EnableRaisingEvents = true;

            // 最新のログファイルを監視する
            DirectoryInfo dir = new DirectoryInfo(targetDirectoryName);
            FileInfo[] fis = dir.GetFiles(targetFileName);
            if (fis.Length == 0)
            {
                Console.WriteLine("VRChatのログファイル無し");
            }
            else
            {
                FileInfo fi = fis.OrderByDescending(p => p.LastWriteTime).ToArray()[0];
#if UBUNTU
                ObserveVRChatLog(fi.FullName); // 最新のログファイルのログ監視をスタートする
#else
                ObserveVRChatLog(fi.DirectoryName + "\\" + fi.Name); // 最新のログファイルのログ監視をスタートする
#endif
            }

            this.history = history;
        }

        ~VRChatLogWatcher()
        {
            Console.WriteLine("VRChatLogWatcher destructor");
            StopObserver();
        }
        // VRChatのログ監視をスタートする
#if UBUNTU
        void ObserveVRChatLog(String fullpath)
        {
            // Ubuntu(ヘッドレス)ビルド用: PowerShell の代わりにプロセス内でファイルを追尾する
            StopObserver();
            tailCts = new CancellationTokenSource();
            CancellationToken token = tailCts.Token;
            tailThread = new Thread(() =>
            {
                try
                {
                    using (FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader reader = new StreamReader(fs, System.Text.Encoding.UTF8))
                    {
                        fs.Seek(0, SeekOrigin.End);
                        reader.DiscardBufferedData();
                        while (!token.IsCancellationRequested)
                        {
                            string line = reader.ReadLine();
                            if (line != null)
                            {
                                try
                                {
                                    ProcessLogLine(line);
                                }
                                catch (Exception le)
                                {
                                    history.WriteLine($"▲ログ行の処理で例外が発生しました: {le.Message}");
                                }
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    history.WriteLine(e.ToString());
                }
            });
            tailThread.IsBackground = true;
            tailThread.Start();
        }
#else
        void ObserveVRChatLog(String fullpath)
        {
            // ログファイルに書き込まれた行を監視するプロセスを起動する
            try
            {
                this.observerProcess = new Process();
                this.observerProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%systemroot%\system32\WindowsPowerShell\v1.0\PowerShell.exe");
                this.observerProcess.StartInfo.Arguments = "Get-Content -Wait -Tail 0 -Encoding UTF8 -Path " + "'" + fullpath + "'";
                this.observerProcess.StartInfo.CreateNoWindow = true;
                this.observerProcess.StartInfo.UseShellExecute = false;
                this.observerProcess.StartInfo.RedirectStandardOutput = true;
                this.observerProcess.OutputDataReceived += new DataReceivedEventHandler(LogOutputDataReceived); // イベント登録

                this.observerProcess.Start();
                this.observerProcess.BeginOutputReadLine();

                //Console.WriteLine("VRChatのログファイル監視スタートしました。 " + Path.GetFileName(fullpath));
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                history.WriteLine(e.ToString());
                DialogWrapper.ShowError("起動に失敗しました。\nPowerShellがみつかりません。", "VRChatOpenBrowser");
                return;
            }
            catch (Exception e)
            {
                history.WriteLine(e.ToString());
                DialogWrapper.ShowError("起動に失敗しました。", "VRChatOpenBrowser");
                return;
            }
        }
#endif
        // 監視プロセスを停止する
#if UBUNTU
        void StopObserver()
        {
            if (tailCts != null)
            {
                tailCts.Cancel();
                if (tailThread != null)
                {
                    tailThread.Join(1000);
                }
                tailCts.Dispose();
                tailCts = null;
                tailThread = null;
            }
        }
#else
        void StopObserver()
        {
            //Console.WriteLine("Observing stop.");
            if (this.observerProcess == null)
            {
                return;
            }
            try
            {
                if (!this.observerProcess.HasExited)
                {
                    this.observerProcess.Kill();
                }
            }
            catch (Exception)
            {
                // 未開始・終了済みなどで例外になる場合は、処分だけ続行する
            }
            this.observerProcess.Dispose();
            this.observerProcess = null;
        }
#endif
        // アプリ終了時に、監視プロセスやタイマーなどを確実に片付ける
        public void Shutdown()
        {
            StopObserver();
            fswatcher.Dispose();
            watchdogTimer.Dispose();
        }
        // ログファイルが新しく作成されたことを監視するイベントハンドラ
        void LogFileCreated(Object source, FileSystemEventArgs e)
        {
            //Console.WriteLine("new LogFile Created. " + e.FullPath);
            StopObserver(); // 監視プロセスを停止する
            ObserveVRChatLog(e.FullPath); // 作成されたファイルを監視対象にする
        }
#if !UBUNTU
        // ファイル書き込みイベントハンドラ
        // 1行分のログを読み、その行に対して処理する
        void LogOutputDataReceived(Object source, DataReceivedEventArgs e)
        {
            ProcessLogLine(e.Data);
        }
#endif
        // 1行分のログに対して処理する
        void ProcessLogLine(string line)
        {
            if(config.PauseSystem)
            {
                return;
            }
            if (String.IsNullOrEmpty(line))
            {
                // NOP
                return;
            }

            string LogPrefix;
            if (line.Contains(LogPrefix = "[YukiYukiVirtual/OpenURL]")) // 旧インターフェイス
            {
                if (!config.OpenBrowserToken.OldInterfaceUsedFlag)
                {
                    config.OpenBrowserToken.OldInterfaceUsedFlag = true;
                    DialogWrapper.ShowWarning("このワールドは旧インターフェイスを使用しています。クライアントアプリの設定により使用できますが、ワールド作者さんは新インターフェイス版に更新してください。", "VRChatOpenBrowser");
                }
                if (config.OpenBrowserToken.AllowOldInterface)
                {
                    // URL取得
                    int index = line.IndexOf(LogPrefix);
                    string rawurl = line.Substring(index + LogPrefix.Length);
                    string url = rawurl.Trim();
                    // URLを開く
                    URLOpenResult urlOpenResult = opener.Open(url);
                    history.WriteLine($" OpenURL: '{url}' {urlOpenResult} (old interface)");
                }
                else
                {
                    DialogWrapper.ShowWarning("旧インターフェイスを使用しない設定により、ブラウザを開きません。コントロールパネルから許可することで使用することが出来ます。", "VRChatOpenBrowser");
                }
            }
            else if (line.Contains(LogPrefix = $"[YukiYukiVirtual/OpenURL:{config.OpenBrowserToken.Token}]"))
            {
                // URL取得
                int index = line.IndexOf(LogPrefix);
                string rawurl = line.Substring(index + LogPrefix.Length);
                string url = rawurl.Trim();
                // URLを開く
                URLOpenResult urlOpenResult = opener.Open(url);
                history.WriteLine($" OpenURL: '{url}' {urlOpenResult}");
            }
            else if (line.Contains(LogPrefix = "[Behaviour] Joining wrld_"))
            {
                int index = line.IndexOf(LogPrefix);
                try
                {
                    // ワールドID取得(行が想定より短い場合は、例外にせず読み飛ばす)
                    int idStartIndex = index + LogPrefix.Length - "wrld_".Length;
                    int idLength = "wrld_xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx".Length;
                    if (line.Length < idStartIndex + idLength)
                    {
                        history.WriteLine($"▲Joining行が短く、ワールドIDを取得できませんでした: '{line}'");
                    }
                    else
                    {
                    NowWorldId = line.Substring(idStartIndex, idLength).Trim();
                    history.WriteLine($" Joining world. '{NowWorldId}'");
                    // 入ったワールドの作者がBANリストに入っているときはシステムを一時停止する
                    if(JsonDownloader.CacheWorldInformation(NowWorldId))
                    {
                        // BANリストから検索する
                        BannedUserInfo bannedUserInfo = config.Setting.BannedUser.Find(x => x.Id == JsonDownloader.CachedAuthorId);
                        if (bannedUserInfo != null)
                        {
                            config.PauseSystem = true;
                            history.WriteLine($"▲BannedUser: {JsonDownloader.CachedAuthorName}({JsonDownloader.CachedAuthorId}) Reason: {bannedUserInfo.Reason}");
                            history.WriteLine($" System paused.");
#if !UBUNTU
                            SystemSounds.Asterisk.Play();
#endif
                            DialogWrapper.ShowWarning($"今Joinしたワールドの作者は、以下の理由によりBANしています。\n理由:{bannedUserInfo.Reason}\n一時的に機能を停止しています。再開するには、コントロールパネルの「再開する」ボタンを押して一時停止を解除してください。", "VRChatOpenBrowser");
                        }
                    }
                    }
                }
                catch (Exception e)
                {
                    // 1行の処理で例外が出ても、ログ監視自体は継続する
                    history.WriteLine($"▲ワールド参加行の処理で例外が発生しました: {e.Message}");
                }
            }

            // watchdog更新
            lock(watchdogLockObject)
            {
                watchdogTimer.Stop();
                watchdogTimer.Start();
            }
        }
        private void WatchdogTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock(watchdogLockObject)
            {
                NowWorldId = null;
                history.WriteLine(" VRChatLogWatcher is bored...");
            }
        }
    }
}
