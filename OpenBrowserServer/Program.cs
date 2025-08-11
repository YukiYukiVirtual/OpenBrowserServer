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
                return;
            }
#if BOOTH
#else
            URLOpener.StaticOpen("https://yukiyukivirtual.booth.pm/items/2539784");
#endif
            Application.Run();
        }
        static void Initialize()
        {
            string workingPath = Environment.ExpandEnvironmentVariables(@"%AppData%\YukiYukiVirtual\OpenBrowserServer"); // ログや設定ファイルなどを置いておくフォルダ(Roaming)
            // プログラムファイルのバージョン情報取得
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
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
            VRChatLogWatcher vrchatLogWatcher = new VRChatLogWatcher(config, history);
            new HttpServer(config, history);
            new NotifyIconForm(config, history, vrchatLogWatcher);
        }
    }
}
