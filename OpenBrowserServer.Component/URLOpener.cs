using System;
using System.Diagnostics;
using System.Media;

namespace OpenBrowserServer.Component
{
    public class URLOpener
    {
        Settings settings;
        DateTime lastOpenTime;
        SoundPlayer openSoundPlayer;
        public URLOpener(Settings settings)
        {
            // 設定ファイルのインスタンスを保持
            this.settings = settings;
            // 最初は必ず開けるようにするために、最小の有効時間で初期化する
            lastOpenTime = DateTime.MinValue;
            openSoundPlayer = new SoundPlayer("C:\\Windows\\Media\\Windows Navigation Start.wav");
        }
        public void Open(string url)
        {
            // URLを開いていいかチェック
            if (CheckURL(url))
            {
                // 前に開いた時間との差が規定以上であればURLを開く
                DateTime now = DateTime.Now;
                TimeSpan timeSpan = now - lastOpenTime;
                if (timeSpan.TotalMilliseconds >= settings.IdlePeriod)
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
        }
        private bool CheckURL(string url)
        {
            bool result; // ループチェック用一時変数

            // Uriオブジェクトを作成
            Uri uri;
            try
            {
                uri = new Uri(url);
            }
            catch (UriFormatException e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine($"URLをUriに出来ませんでした。'{url}'");
                return false;
            }

            // プロトコルチェック
            // result = false;
            // foreach (string protocol in settings.Protocol)
            // {
            //     Console.WriteLine(protocol);
            //     if (uri.Scheme == protocol)
            //     {
            //         result = true;
            //         break;
            //     }
            // }
            result = settings.Protocol.Contains(uri.Scheme);
            if (!result)
            {
                Console.WriteLine($"{uri.Scheme}を開くことはできません。'{url}'");
                return false;
            }

            // ドメインチェック
            result = false;
            foreach (string host in settings.Domain)
            {
                Console.WriteLine(host);
                if(uri.Host == host || uri.Host.EndsWith("."+host))
                {
                    result= true;
                    break;
                }
            }
            if(!result)
            {
                Console.WriteLine($"{uri.Scheme}を開くことはできません。'{url}'");
                return false;
            }

            // ここまで来たらチェックはOK
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
