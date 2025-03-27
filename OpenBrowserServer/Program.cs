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
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 多重起動抑止処理
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, "VRChatOpenBrowser");
            if (!mutex.WaitOne(0, false))
            {
                MessageBox.Show("すでに起動しています。2つ同時には起動できません。", "VRChatOpenBrowser");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Initialize();
#if BOOTH
#else
            URLOpener.StaticOpen("https://yukiyukivirtual.booth.pm/items/2539784");
#endif
            Application.Run();
        }
        static void Initialize()
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location); // プログラムファイルのバージョン情報取得
            Settings settings = new Settings(fileVersionInfo);
            if(settings.NeedUpgrade())
            {
                if(DialogWrapper.UpdateConfirm())
                {
                    Environment.Exit(0);
                    return;
                }
            }
            History history = new History();
            history.WriteLine($"■起動時のバージョン {settings.FileVersion}");
            VRChatLogWatcher vrchatLogWatcher = new VRChatLogWatcher(settings, history);
            new HttpServer(settings, history);
            new NotifyIconForm(settings, history, vrchatLogWatcher);
        }
    }
}
