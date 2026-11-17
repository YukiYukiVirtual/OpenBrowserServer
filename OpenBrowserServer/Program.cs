using System;
using System.Diagnostics;
using System.Windows.Forms;
using OpenBrowserServer.Component;
using OpenBrowserServer.Logger;
using OpenBrowserServer.WebServer;

namespace OpenBrowserServer
{
    internal static class Program
    {
        private static System.Threading.Mutex mutex;
        private static VRChatLogWatcher vrchatLogWatcher;
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 多重起動抑止処理
            mutex = new System.Threading.Mutex(false, "VRChatOpenBrowser", out bool createdNew);
            if (!createdNew)
            {
                DialogWrapper.ShowStop("すでに起動しています。2つ同時には起動できません。", "VRChatOpenBrowser");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Initialize();

            }
            catch (System.Net.HttpListenerException)
            {
                DialogWrapper.ShowStop("ポートが使用されています。2つ同時には起動できません。", "VRChatOpenBrowser");
                vrchatLogWatcher?.Shutdown();
                return;
            }
            catch (Exception ex)
            {
                // 起動時の例外はここで受け止める。握りつぶさず、記録して知らせて終了する。
                // History の生成前でも起こりうるため、通常ログではなく crash.log に直接書く。
                string crashLogPath = null;
                try
                {
                    string workingPath = Environment.ExpandEnvironmentVariables(@"%AppData%\YukiYukiVirtual\OpenBrowserServer\");
                    System.IO.Directory.CreateDirectory(workingPath);
                    crashLogPath = System.IO.Path.Combine(workingPath, "crash.log");
                    System.IO.File.AppendAllText(crashLogPath, $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}] 起動時に例外が発生しました{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
                }
                catch
                {
                    // ログ書き込み自体の失敗では落とさない(ダイアログ表示を優先する)
                }
                DialogWrapper.ShowStop($"起動に失敗しました。{(crashLogPath != null ? $"詳細は crash.log に記録しました。{Environment.NewLine}{crashLogPath}{Environment.NewLine}" : "")}{ex.Message}", "VRChatOpenBrowser");
                vrchatLogWatcher?.Shutdown();
                return;
            }
#if BOOTH
#else
            URLOpener.StaticOpen("https://yukiyukivirtual.booth.pm/items/2539784");
#endif
            Application.Run();
            vrchatLogWatcher?.Shutdown();
        }
        static void Initialize()
        {
            string workingPath = Environment.ExpandEnvironmentVariables(@"%AppData%\YukiYukiVirtual\OpenBrowserServer"); // ログや設定ファイルなどを置いておくフォルダ(Roaming)
            // プログラムファイルのバージョン情報取得
            // 単一ファイル発行では Assembly.Location が空文字になるため、その場合は実行ファイル自身から取得する
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(string.IsNullOrEmpty(assemblyLocation) ? Environment.ProcessPath : assemblyLocation);
            Config config = new Config(workingPath, fileVersionInfo);
            if(config.NeedUpgrade())
            {
                if(DialogWrapper.UpdateConfirm())
                {
                    Environment.Exit(0);
                    return;
                }
            }
            History history = new History(workingPath);
            history.WriteLine($"■起動時のバージョン {config.FileVersion}_{config.Edition}");
            history.WriteLine($" Token {config.OpenBrowserToken.Token}");
            vrchatLogWatcher = new VRChatLogWatcher(config, history);
            new HttpServer(config, history);
            new NotifyIconForm(config, history, vrchatLogWatcher);
        }
    }
}
