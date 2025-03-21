using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenBrowserServer.Component
{
    public class VRChatLogWatcher
    {
        URLOpener opener; // URLを開くやつ
        FileSystemWatcher fswatcher; // ログファイルが作成されたことを監視するやーつ
        Process observerProcess; // ログを監視するプロセス
        public VRChatLogWatcher(Settings setting)
        {
            this.opener = new URLOpener(setting);

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
                //Logger.LogInfo("VRChatのログファイル無し");
            }
            else
            {
                FileInfo fi = fis.OrderByDescending(p => p.LastWriteTime).ToArray()[0];
                ObserveVRChatLog(fi.DirectoryName + "\\" + fi.Name); // 最新のログファイルのログ監視をスタートする
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

                //Logger.LogInfo("VRChatのログファイル監視スタートしました。 " + Path.GetFileName(fullpath));
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                //Logger.LogError(e.ToString());
                //MessageBox.Show("起動に失敗しました。\nPowerShellがみつかりません。", "例外");
                return;
            }
            catch (Exception e)
            {
                //Logger.LogError(e.ToString());
                //MessageBox.Show("起動に失敗しました。", "例外");
                return;
            }
        }
        // 監視プロセスを停止する
        void StopObserver()
        {
            //Logger.LogInfo("Observing stop.");
            if (!this.observerProcess.HasExited)
            {
                this.observerProcess.Kill();
            }
            this.observerProcess.Close();
        }
        // ログファイルが新しく作成されたことを監視するイベントハンドラ
        void LogFileCreated(Object source, FileSystemEventArgs e)
        {
            //Logger.LogInfo("new LogFile Created. " + e.FullPath);
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
            string LogName = "[YukiYukiVirtual/OpenURL]";
            if (line.Contains(LogName))
            {
                int index = line.IndexOf(LogName);
                string rawurl = line.Substring(index + LogName.Length);
                string url = rawurl.Trim();
                //Logger.LogTrace("OpenURL: " + url);
                this.opener.Open(url);
            }
        }
    }
}
