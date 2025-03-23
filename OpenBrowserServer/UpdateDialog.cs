using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenBrowserServer.Component;
using System.Windows.Forms;

namespace OpenBrowserServer
{
    internal class UpdateDialog
    {
        public static bool Confirm()
        {
            DialogResult dialogResult = MessageBox.Show("プログラムの更新があります。プログラムを更新するために終了しますか？", "更新確認", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                URLOpener.StaticOpen(".");
                URLOpener.StaticOpen("https://yukiyukivirtual.booth.pm/items/2539784");
                MessageBox.Show("フォルダと配布ページを開いたので、手順に従って更新してください。");
                return true;
            }
            return false;
        }
    }
}
