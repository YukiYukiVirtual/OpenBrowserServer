using System;
using System.Diagnostics;
using System.Media;

namespace OpenBrowserServer.Component
{
    public class URLOpener
    {
        DateTime lastOpenTime;
        SoundPlayer openSoundPlayer;
        public URLOpener()
        {
            // 設定ファイルのインスタンスを保持
            // 最初は必ず開けるようにするために、最小の有効時間で初期化する
            lastOpenTime = DateTime.MinValue;
            openSoundPlayer = new SoundPlayer("C:\\Windows\\Media\\Windows Navigation Start.wav");
        }
        public void Open(string url)
        {
            // URLを開いていいかチェック
            // ドメインチェック
            // 時間チェック
            // 前に開いた時間との差が規定以上であればURLを開く
            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - lastOpenTime;
            if (timeSpan.TotalMilliseconds >= 500)
            {
                // URLを開く
                try
                {
                    openSoundPlayer.Play(); // TODO オリジナルの音にする
                }
                finally
                {
                    StaticOpen(url);
                    lastOpenTime = now;
                }
            }
        }
        private bool CheckURL(string url)
        {
            return true;
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
