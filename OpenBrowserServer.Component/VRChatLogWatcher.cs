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
        readonly URLOpener opener; // URLを開くやつ
        readonly FileSystemWatcher fswatcher; // ログファイルが作成されたことを監視するやーつ
        readonly History history; // 履歴ログ

        Process observerProcess; // ログを監視するプロセス
        public string NowWorldId { get; private set; }
        bool worldJoined;
        public VRChatLogWatcher(Settings setting, History history)
        {
            this.opener = new URLOpener(setting);
            this.history = history;

            NowWorldId = null;
            worldJoined = false;

            string targetDirectoryName = Environment.ExpandEnvironmentVariables(@"%AppData%\..\LocalLow\VRChat\VRChat");
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

            string LogPrefix;
            if (line.Contains(LogPrefix = "[YukiYukiVirtual/OpenURL]"))
            {
                int index = line.IndexOf(LogPrefix);
                string rawurl = line.Substring(index + LogPrefix.Length);
                string url = rawurl.Trim();
                URLOpenResult urlOpenResult = opener.Open(url);
                history.WriteLine($"OpenURL: '{url}' {urlOpenResult}");
            }
            else if (line.Contains(LogPrefix = "[Behaviour] Joining wrld_"))
            {
                int index = line.IndexOf(LogPrefix);
                try
                {
                    NowWorldId = line.Substring(index + LogPrefix.Length - "wrld_".Length, "wrld_xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx".Length).Trim();
                    history.WriteLine($"■Joining world. '{NowWorldId}'");
                }
                finally { }
            }
        }
    }
}
