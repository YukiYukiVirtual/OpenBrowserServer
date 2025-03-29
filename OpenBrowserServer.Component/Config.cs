using System;
using System.Diagnostics;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenBrowserServer.Component
{
    public class Config
    {
#if DEBUG
        private const string DefaultPath = "../../../setting.yaml";
#else
        private const string DefaultPath = "setting.yaml";
#endif
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
        private readonly FileVersionInfo fileVersionInfo;
        public string FileVersion { get
            {
                return $"v{fileVersionInfo.ProductMajorPart}.{fileVersionInfo.ProductMinorPart}.{fileVersionInfo.ProductBuildPart}";
            }
        }
        public bool PauseSystem { get; set; }
        public Config()
        {
            this.fileVersionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location); // プログラムファイルのバージョン情報取得
            Update();
        }
        ~Config()
        {
            Console.WriteLine("Config destructor");
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
        public bool NeedUpgrade()
        {
            return !FileVersion.Equals(Setting.Version);
        }
        public static void Download()
        {
#if DEBUG

#else
            string url = "https://raw.githubusercontent.com/YukiYukiVirtual/OpenBrowserServer/master/setting.yaml#" + DateTime.Now.ToString();
            // ダウンロード
            using(HttpClient client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                try
                {
                    string responseBody = client.GetStringAsync(url).Result;
                    File.WriteAllText(DefaultPath, responseBody);
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
            if (File.Exists(DefaultPath))
            {
                string content = File.ReadAllText(DefaultPath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
                Setting = deserializer.Deserialize<Setting>(content);
            }
            else
            {
                Console.WriteLine($"{DefaultPath}が存在しないため、設定ファイルのインポートを中止します。デフォルト設定が使用されます。");
                Setting = new Setting();

                Setting.Protocol.Add("https");

                Setting.Domain.Add("booth.pm");
                Setting.Domain.Add("yukiyukivirtual.github.io");
                Setting.Domain.Add("yukiyukivirtual.net");
            }
            Console.WriteLine("ImportYaml");
            Console.WriteLine(Setting.ToString());
        }
    }
}
