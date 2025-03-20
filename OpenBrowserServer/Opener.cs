using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace OpenBrowserServer
{
    internal class Opener
    {
        public static void Open(string str_url)
        {
            cmdstart(str_url);
            try
            {
                SystemSounds.Asterisk.Play();
            }
            finally {/*ここの例外処理は重要ではない*/}
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
