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
