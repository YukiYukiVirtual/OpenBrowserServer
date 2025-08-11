using System.Media;
using System.Windows.Forms;

namespace OpenBrowserServer.Component
{
    public class DialogWrapper
    {
        public static bool DownloadConfirm()
        {
            DialogResult dialogResult = MessageBox.Show("設定ファイルをダウンロードして再読み込みしますか？", "VRChatOpenBrowser", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dialogResult == DialogResult.Yes)
            {
                return true;
            }
            return false;
        }
        public static bool UpdateConfirm()
        {
            DialogResult dialogResult = MessageBox.Show("プログラムの更新があります。プログラムを更新するために終了しますか？", "VRChatOpenBrowser", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dialogResult == DialogResult.Yes)
            {
                URLOpener.StaticOpen("https://yukiyukivirtual.booth.pm/items/2539784");
                ShowInformation("配布ページを開いたので、手順に従って更新してください。", "VRChatOpenBrowser");
                return true;
            }
            return false;
        }
        public static bool ExitConfirm()
        {
            DialogResult dialogResult = MessageBox.Show("プログラムを終了しますか？", "VRChatOpenBrowser", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dialogResult == DialogResult.Yes)
            {
                return true;
            }
            return false;
        }
        public static void Completed()
        {
            ShowInformation("完了しました", "VRChatOpenBrowser");
        }
        public static DialogResult ShowInformation(string text, string caption)
        {
            return MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        public static DialogResult ShowWarning(string text, string caption)
        {
            return MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        public static DialogResult ShowError(string text, string caption)
        {
            return MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        public static DialogResult ShowStop(string text, string caption)
        {
            return MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
    }
}
