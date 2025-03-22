using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

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
        public HashSet<string> Protocol { get; private set; }
        public HashSet<string> Domain { get; private set; }
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
            Protocol = new HashSet<string>()
            {
                "https",
            };
            Domain = new HashSet<string>()
            {
                "booth.pm",
                "yukiyukivirtual.github.io",
                "yukiyukivirtual.net",
            };
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
                    var protocols = (YamlSequenceNode)root.Children[new YamlScalarNode("Protocol")];
                    foreach (YamlScalarNode item in protocols)
                    {
                        string value = item.Value;
                        Protocol.Add(value);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Protocolキーのパースに失敗");
                }
                try
                {
                    var domains = (YamlSequenceNode)root.Children[new YamlScalarNode("Domain")];
                    foreach (YamlScalarNode item in domains)
                    {
                        string value = item.Value;
                        Domain.Add(value);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Domainキーのパースに失敗");
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
