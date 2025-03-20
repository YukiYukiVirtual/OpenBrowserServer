using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace OpenBrowserServer
{
    internal class URLOpener
    {
        public URLOpener()
        {
            // 設定ファイルのインスタンスを保持
        }
        public void Open(string url)
        {
            // URLを開いていいかチェック

            // URLを開く
            try
            {
                SystemSounds.Asterisk.Play();
            }
            finally
            {
                StaticOpen(url);
            }
        }
        public static void StaticOpen(string url)
        {
            cmdstart(url);
        }
        private static void cmdstart(string arg)
        {
            ProcessStartInfo psi = new ProcessStartInfo(arg);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = true;

            Process.Start(psi);
        }
    }
}
