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
            get => "Beta"; 
        }
        public Setting Setting { get; private set; }
        public string FileVersion { get; private set; }
        public bool PauseSystem { get; set; }
        public string WorkingPath { get; private set; }
        public Config(string workingPath, FileVersionInfo fileVersionInfo)
        {
            this.WorkingPath = workingPath;
            SettingFilePath =
#if DEBUG
                "../../../setting.yaml";
#else
                Path.Combine(workingPath, "setting.yaml");
#endif
            Console.WriteLine($"SettingFilePath: {SettingFilePath}");
            FileVersion = $"v{fileVersionInfo.ProductMajorPart}.{fileVersionInfo.ProductMinorPart}.{fileVersionInfo.ProductBuildPart}";
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
        public void Reload()
        {
            Clear();
            ImportYaml();
        }
        public bool NeedUpgrade()
        {
            return !FileVersion.Equals(Setting.Version);
        }
        public void Download()
        {
#if DEBUG

#else
            string url = "https://raw.githubusercontent.com/YukiYukiVirtual/OpenBrowserServer/visualstudio_beta/setting.yaml#" + DateTime.Now.ToString();
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
            Console.WriteLine("ImportYaml");
            Console.WriteLine(Setting.ToString());
        }
    }
}
