using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenBrowserServer.Logger;

namespace OpenBrowserServer.Component
{
    public delegate void VRChatLogCallback(string key, string line);
    public class VRChatLogWatcher
    {
        URLOpener opener; // URLを開くやつ
        FileSystemWatcher fswatcher; // ログファイルが作成されたことを監視するやーつ
        Process observerProcess; // ログを監視するプロセス
        History history; // 履歴ログ
        string worldName;
        string worldId;
        bool worldJoined;
        public VRChatLogWatcher(Settings setting, History history)
        {
            this.opener = new URLOpener(setting);
            this.history = history;

            worldName = null;
            worldId = null;
            worldJoined = false;

            string targetDirectoryName = Environment.ExpandEnvironmentVariables(@"%AppData%\..\LocalLow\VRChat\VRChat");
            string targetFileName = "output_log_*.txt";

            // VRChat起動時のログファイルの作成を監視するために、FileSystemWatcherの初期化
            this.fswatcher = new FileSystemWatcher();
            this.fswatcher.Path = targetDirectoryName; // VRChatのログがあるフォルダを監視
            this.fswatcher.NotifyFilter = NotifyFilters.FileName; // ファイル名を監視
            this.fswatcher.Filter = targetFileName;
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
                ObserveVRChatLog(fi.DirectoryName + "\\" + fi.Name); // 最新のログファイルのログ監視をスタートする
            }

            this.history = history;
        }
        ~VRChatLogWatcher()
        {
            Console.WriteLine("VRChatLogWatcher destructor");
            StopObserver();
        }
        // VRChatのログ監視をスタートする
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
                Console.WriteLine(e.ToString());
                MessageBox.Show("起動に失敗しました。\nPowerShellがみつかりません。", "例外");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                MessageBox.Show("起動に失敗しました。", "例外");
                return;
            }
        }
        // 監視プロセスを停止する
        void StopObserver()
        {
            //Console.WriteLine("Observing stop.");
            if (!this.observerProcess.HasExited)
            {
                this.observerProcess.Kill();
            }
            this.observerProcess.Close();
        }
        // ログファイルが新しく作成されたことを監視するイベントハンドラ
        void LogFileCreated(Object source, FileSystemEventArgs e)
        {
            //Console.WriteLine("new LogFile Created. " + e.FullPath);
            StopObserver(); // 監視プロセスを停止する
            ObserveVRChatLog(e.FullPath); // 作成されたファイルを監視対象にする
        }
        // ファイル書き込みイベントハンドラ
        void LogOutputDataReceived(Object source, DataReceivedEventArgs e)
        {
            string line = e.Data;
            if (String.IsNullOrEmpty(line))
            {
                // NOP
                return;
            }
            foreach(var callback in new Callback[] { OpenURL, JoinWorld })
            {
                if (callback(line)) break;
            }
        }
        delegate bool Callback(string line);
        bool OpenURL(string line)
        {
            string LogPrefix = "[YukiYukiVirtual/OpenURL]";
            if (line.Contains(LogPrefix))
            {
                int index = line.IndexOf(LogPrefix);
                string rawurl = line.Substring(index + LogPrefix.Length);
                string url = rawurl.Trim();
                URLOpenResult urlOpenResult = opener.Open(url);
                history.WriteLine($"OpenURL: {url} {urlOpenResult}");
                return true;
            }
            return false;
        }
        bool JoinWorld(string line)
        {
            string LogPrefix1 = "[Behaviour] Joining or Creating Room: ";
            string LogPrefix2 = "[Behaviour] Joining wrld_";
            if (line.Contains(LogPrefix1))
            {
                int index = line.IndexOf(LogPrefix1);
                worldName = line.Substring(index + LogPrefix1.Length).Trim();
                JoinWorldLogging();
                return true;
            }
            else if (line.Contains(LogPrefix2))
            {
                int index = line.IndexOf(LogPrefix2);
                try
                {
                    worldId = line.Substring(index + LogPrefix2.Length - "wrld_".Length, "wrld_xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx".Length).Trim();
                    JoinWorldLogging();
                }
                finally { }
                return true;
            }
            return false;
        }
        void JoinWorldLogging()
        {
            if (worldJoined)
            {
                worldJoined = false;
                history.WriteLine($"■Joining world. '{worldName}' ({worldId})");
            }
            else
            {
                worldJoined = true;
            }
        }
    }
}
