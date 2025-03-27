using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using System.Linq;

namespace OpenBrowserServer.Component
{
    public class Settings
    {
        private const string DefaultPath = "setting.yaml";
        public string Edition {
            get =>
#if   BOOTH
                "Booth"
#else
                "Free"
#endif
                ; 
        }

        public readonly FileVersionInfo fileVersionInfo;
        public string FileVersion { get
            {
                return $"v{fileVersionInfo.ProductMajorPart}.{fileVersionInfo.ProductMinorPart}.{fileVersionInfo.ProductBuildPart}";
            }
        }
        public string Version { get; private set; }
        public int IdlePeriod { get; private set; }
        public int HttpRequestPeriod { get; private set; }
        public List<string> Protocol { get; private set; }
        public List<string> Domain { get; private set; }
        public List<string> BannedUser { get; private set; }
        public bool PauseSystem { get; set; }
        public Settings(FileVersionInfo fileVersionInfo)
        {
            this.fileVersionInfo = fileVersionInfo;
            Update();
            //Console.WriteLine(ToString());
        }
        ~Settings()
        {
            Console.WriteLine("Settings destructor");
        }
        public void Clear()
        {
            Version = FileVersion;
            IdlePeriod = 500;
            HttpRequestPeriod = 5000;
            Protocol = new List<string>();
            Domain = new List<string>();
            BannedUser = new List<string>();
        }
        public void Update()
        {
            Clear();
            Download();
            ImportYaml();
        }
        public bool NeedUpgrade()
        {
            return !FileVersion.Equals(Version);
        }
        public static void Download()
        {
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
        }
        private void ImportYaml()
        {
            if(!File.Exists(DefaultPath))
            {
                Console.WriteLine($"{DefaultPath}が存在しないため、設定ファイルのインポートを中止します。デフォルト設定が使用されます。");

                Protocol.Add("https");

                Domain.Add("booth.pm");
                Domain.Add("yukiyukivirtual.github.io");
                Domain.Add("yukiyukivirtual.net");
                return;
            }
            using (var streamReader = new StreamReader(DefaultPath))
            {
                YamlStream yamlStream = new YamlStream();
                yamlStream.Load(streamReader);
                var root = (YamlMappingNode)yamlStream.Documents[0].RootNode;
                try
                {
                    var item = (YamlScalarNode)root.Children[new YamlScalarNode("Version")];
                    Version = item.Value;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Versionキーのパースに失敗");
                }
                try
                {
                    var item = (YamlScalarNode)root.Children[new YamlScalarNode("IdlePeriod")];
                    IdlePeriod = int.Parse(item.Value); ;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("IdlePeriodキーのパースに失敗");
                }
                try
                {
                    var item = (YamlScalarNode)root.Children[new YamlScalarNode("HttpRequestPeriod")];
                    HttpRequestPeriod = int.Parse(item.Value); ;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("HttpRequestPeriodキーのパースに失敗");
                }
                try
                {
                    Protocol.AddRange(from YamlScalarNode item in (YamlSequenceNode)root.Children[new YamlScalarNode("Protocol")]
                                      let value = item.Value
                                      select value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Protocolキーのパースに失敗");
                }
                try
                {
                    Domain.AddRange(from YamlScalarNode item in (YamlSequenceNode)root.Children[new YamlScalarNode("Domain")]
                                    let value = item.Value
                                    select value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Domainキーのパースに失敗");
                }
                try
                {
                    BannedUser.AddRange(from YamlScalarNode item in (YamlSequenceNode)root.Children[new YamlScalarNode("BannedUser")]
                                    let value = item.Value
                                    select value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("BannedUserキーのパースに失敗");
                }
                //Console.WriteLine(this.ToString());
            }
        }
        public override string ToString()
        {

            var serializer = new Serializer();
            return serializer.Serialize(this).Trim();
        }
    }
}
