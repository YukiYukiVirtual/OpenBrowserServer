using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using OpenBrowserServer.Component;
using OpenBrowserServer.Logger;
using OpenBrowserServer.WebServer;

namespace OpenBrowserServer.Headless
{
    // Ubuntu(ヘッドレス)ビルド用のエントリポイント。
    // Windows版 Program.cs の Initialize() と同じ組み立てを、画面なしで行う。
    //
    // 環境変数:
    //   OPENBROWSER_WORKING_DIR   作業フォルダ(設定/ログ/keys の置き場所)。既定は ./work
    //   OPENBROWSER_SETTING_FILE  起動前に作業フォルダへコピーする setting.yaml のパス
    //   OPENBROWSER_VRCHAT_LOG_DIR VRChatのログフォルダ(模倣先)。既定は <作業フォルダ>/FakeVRChat
    internal static class Program
    {
        static void Main()
        {
            string workingPath = Environment.GetEnvironmentVariable("OPENBROWSER_WORKING_DIR")
                ?? Path.Combine(Environment.CurrentDirectory, "work");
            Directory.CreateDirectory(workingPath);

            string seedSetting = Environment.GetEnvironmentVariable("OPENBROWSER_SETTING_FILE");
            if (!string.IsNullOrEmpty(seedSetting) && File.Exists(seedSetting))
            {
                File.Copy(seedSetting, Path.Combine(workingPath, "setting.yaml"), true);
                Console.WriteLine($"[Headless] setting.yaml をコピーしました: {seedSetting}");
            }

            // 単一ファイル発行では Assembly.Location が空文字になるため、その場合は実行ファイル自身から取得する(本体 Program.cs と同じ対策)
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(string.IsNullOrEmpty(assemblyLocation) ? Environment.ProcessPath : assemblyLocation);
            Config config = new Config(workingPath, fileVersionInfo);
            if (config.NeedUpgrade())
            {
                // Windows版はここで更新確認ダイアログを出すが、ヘッドレス版では警告だけ出して続行する
                Console.WriteLine($"[Headless] バージョン不一致: app={config.FileVersion} setting={config.Setting.Version}(続行します)");
            }

            History history = new History(workingPath);
            history.WriteLine($"■起動時のバージョン {config.FileVersion}_{config.Edition}");
            history.WriteLine($" Token {config.OpenBrowserToken.Token}");

            VRChatLogWatcher vrchatLogWatcher = new VRChatLogWatcher(config, history);
            new HttpServer(config, history);

            Console.WriteLine("[Headless] 起動しました。");
            Console.WriteLine($"[Headless] Token: {config.OpenBrowserToken.Token}");
            Console.WriteLine("[Headless] 終了するには 'exit' と入力してください(標準入力が閉じても終了します)。");

            string line;
            while ((line = Console.ReadLine()) != null)
            {
                if (line.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }
            Console.WriteLine("[Headless] 終了します。");
        }
    }
}
