using System;
using System.Diagnostics;
using System.Media;

namespace OpenBrowserServer.Component
{
    public enum URLOpenResult
    {
        OK,
        TimeSpanError,
        DomainError,
        ProtocolError,
        FormatError,
    }
    public class URLOpener
    {
        readonly Config config;
        static readonly SoundPlayer openSoundPlayer = new SoundPlayer("C:\\Windows\\Media\\Windows Navigation Start.wav");
        DateTime lastOpenTime;
        public URLOpener(Config config)
        {
            // 設定ファイルのインスタンスを保持
            this.config = config;
            // 最初は必ず開けるようにするために、最小の有効時間で初期化する
            lastOpenTime = DateTime.MinValue;
        }
        ~URLOpener()
        {
            Console.WriteLine("URLOpener destructor");
        }
        public URLOpenResult Open(string url)
        {
            // URLを開いていいかチェック
            URLOpenResult urlOpenResult = CheckURL(url);
            if (urlOpenResult != URLOpenResult.OK)
            {
                return urlOpenResult;
            }
            // 前に開いた時間との差が規定以上であればURLを開く
            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - lastOpenTime;
            if (timeSpan.TotalMilliseconds < config.IdlePeriod)
            {
                return URLOpenResult.TimeSpanError;
            }
            // URLを開く
            StaticOpen(url);
            lastOpenTime = now;
            return URLOpenResult.OK;
        }
        private URLOpenResult CheckURL(string url)
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
                return URLOpenResult.FormatError;
            }

            result = config.Protocol.Contains(uri.Scheme);
            if (!result)
            {
                Console.WriteLine($"{uri.Scheme}を開くことはできません。'{url}'");
                return URLOpenResult.ProtocolError;
            }

            // ドメインチェック
            result = false;
            foreach (string host in config.Domain)
            {
                if(uri.Host == host || uri.Host.EndsWith("."+host))
                {
                    result= true;
                    break;
                }
            }
            if(!result)
            {
                Console.WriteLine($"{uri.Host}を開くことはできません。'{url}'");
                return URLOpenResult.DomainError;
            }

            // ここまで来たらチェックはOK
            return URLOpenResult.OK;
        }
        public static void StaticOpen(string url)
        {
            try
            {
                openSoundPlayer.Play(); // TODO オリジナルの音にする
            }
            finally
            {
                CmdStartProcess(url);
            }
        }
        private static void CmdStartProcess(string arg)
        {
            ProcessStartInfo psi = new ProcessStartInfo(arg)
            {
                CreateNoWindow = true,
                UseShellExecute = true,
            };
            try
            {
                Process.Start(psi);
            }
            catch(Exception e)
            {
                Console.WriteLine($"CmdStartProcess: {arg} {e}");
            }
        }
    }
}
