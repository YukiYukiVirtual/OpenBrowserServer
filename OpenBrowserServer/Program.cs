using System;
using System.Diagnostics;
using System.Windows.Forms;
using OpenBrowserServer.Component;
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Initialize();
            Application.Run();
        }
        static void Initialize()
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location); // プログラムファイルのバージョン情報取得
            Settings settings = new Settings(fileVersionInfo);
            if(settings.NeedUpgrade())
            {
                if(UpdateDialog.Confirm())
                {
                    Environment.Exit(0);
                    return;
                }
            }
            NotifyIconForm notifyIconForm = new NotifyIconForm(settings);
            VRChatLogWatcher watcher = new VRChatLogWatcher(settings);
            HttpServer httpServer = new HttpServer(settings);
        }
    }
}
