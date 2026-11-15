using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenBrowserServer.Component
{
    public class Config
    {
        private readonly string SettingFilePath = "";
        public string Edition {
            get =>
#if BOOTH
                "Booth"
#else
                "Free"
#endif
                ; 
        }
        public Setting Setting { get; private set; }
        public string FileVersion { get; private set; }
        public bool PauseSystem { get; set; }
        public string WorkingPath { get; private set; }
        public OpenBrowserToken OpenBrowserToken { get; private set; }
        public Config(string workingPath, FileVersionInfo fileVersionInfo)
        {
            this.WorkingPath = workingPath;
            // 初回起動時は作業フォルダがまだ無いため、先に作っておく。
            // 無いと設定ファイルのダウンロード保存が失敗し、デフォルト設定(Version="")で更新チェックが誤検知する。
            Directory.CreateDirectory(workingPath);
            SettingFilePath =
#if DEBUG
                "../../../setting.yaml";
#else
                Path.Combine(workingPath, "setting.yaml");
#endif
            //Console.WriteLine($"SettingFilePath: {SettingFilePath}");
            FileVersion = $"{fileVersionInfo.ProductMajorPart}.{fileVersionInfo.ProductMinorPart}.{fileVersionInfo.ProductBuildPart}";
            JsonDownloader.UserAgent = $"OpenBrowserServer/{FileVersion}";
            OpenBrowserToken = new OpenBrowserToken();
            Update();
        }
        ~Config()
        {
            //Console.WriteLine("Config destructor");
        }
        public void Clear()
        {
        }
        public void Update()
        {
            Clear();
            Download();
            ImportYaml();
        }
        public void Reload()
        {
            Clear();
            ImportYaml();
        }
        public bool NeedUpgrade()
        {
            // 設定ファイルが読み込めなかった場合(Version が空)は更新の有無を判断できないため、更新ありとはしない
            if (string.IsNullOrEmpty(Setting.Version))
            {
                return false;
            }
            return !FileVersion.Equals(Setting.Version);
        }
        public void Download()
        {
#if UBUNTU
            // Ubuntu(ヘッドレス)ビルド用: ネットワークから取得せず、配置済みの設定ファイルをそのまま使う
            Console.WriteLine("設定ファイルのダウンロードはスキップしました(UBUNTUビルド)。");
            return;
#elif DEBUG

#else
            string url = "https://raw.githubusercontent.com/YukiYukiVirtual/OpenBrowserServer/master/setting.yaml#" + DateTime.Now.ToString();
            // ダウンロード
            using(HttpClient client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                try
                {
                    string responseBody = client.GetStringAsync(url).Result;
                    File.WriteAllText(SettingFilePath, responseBody);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("設定ファイルのダウンロードが失敗しました。過去にダウンロードしたものがあればそれを使います。");
                }
            }
#endif
        }
        private void ImportYaml()
        {
            if (File.Exists(SettingFilePath))
            {
                string content = File.ReadAllText(SettingFilePath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
                Setting = deserializer.Deserialize<Setting>(content);
            }
            else
            {
                Console.WriteLine($"{SettingFilePath}が存在しないため、設定ファイルのインポートを中止します。デフォルト設定が使用されます。");
                Setting = new Setting();

                Setting.Protocol.Add("https");

                Setting.Domain.Add("booth.pm");
                Setting.Domain.Add("yukiyukivirtual.github.io");
                Setting.Domain.Add("yukiyukivirtual.net");
            }
            //Console.WriteLine("ImportYaml");
            //Console.WriteLine(Setting.ToString());
        }
    }
}
