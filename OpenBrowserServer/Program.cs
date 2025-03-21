using System;
using System.Windows.Forms;
using OpenBrowserServer.Component;

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
            Settings settings = new Settings();
            settings.UpdateSetting();
            NotifyIconForm notifyIconForm = new NotifyIconForm();
            VRChatLogWatcher watcher = new VRChatLogWatcher(settings);
        }
    }
}
