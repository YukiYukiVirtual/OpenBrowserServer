using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenBrowserServer.Component;
using System.Windows.Forms;

namespace OpenBrowserServer
{
    internal class DialogWrapper
    {
        public static bool DownloadConfirm()
        {
            DialogResult dialogResult = MessageBox.Show("設定ファイルをダウンロードして再読み込みしますか？", "更新確認", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                return true;
            }
            return false;
        }
        public static bool UpdateConfirm()
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
        public static bool ExitConfirm()
        {
            DialogResult dialogResult = MessageBox.Show("プログラムを終了しますか？", "終了", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                return true;
            }
            return false;
        }
        public static void Completed()
        {
            MessageBox.Show("完了しました");
        }
    }
}
