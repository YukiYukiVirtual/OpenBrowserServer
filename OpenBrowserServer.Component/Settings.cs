using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenBrowserServer.Component
{
    public class Settings
    {
        private const string DefaultPath = "setting.yaml";
        private static readonly HttpClient client = new HttpClient(); // 設定ファイルを更新する用のHTTPクライアント

        public string Version;
        public HashSet<string> Protocol;
        public HashSet<string> Domain;
        public Settings()
        {
            Clear();
        }
        public void Clear()
        {
            Version = "unknown";
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
        public void UpdateSetting()
        {
            string yaml = Download();
            ImportYaml(yaml);
        }
        public static string Download()
        {
            // ダウンロード
            string url = "https://raw.githubusercontent.com/YukiYukiVirtual/OpenBrowserServer/master/setting.yaml#" + DateTime.Now.ToString();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            string responseBody = client.GetStringAsync(url).Result;
            File.WriteAllText(DefaultPath, responseBody);
            return responseBody;
        }
        private void ImportYaml(string yaml)
        {
            using (var streamReader = new StreamReader(DefaultPath))
            {
                YamlStream yamlStream = new YamlStream();
                yamlStream.Load(streamReader);
                var root = (YamlMappingNode)yamlStream.Documents[0].RootNode;
                try
                {
                    var version = (YamlScalarNode)root.Children[new YamlScalarNode("Version")];
                    Version = version.Value;
                    Console.WriteLine(Version);
                }
                catch (KeyNotFoundException e)
                {
                    Console.WriteLine("Versionキーが不在");
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
                catch (KeyNotFoundException e)
                {
                    Console.WriteLine("Protocolキーが不在");
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
                catch (KeyNotFoundException e)
                {
                    Console.WriteLine("Domainキーが不在");
                }

                Console.WriteLine(this.ToString());
            }
        }
        public override string ToString()
        {

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return serializer.Serialize(this).Trim();
        }
    }
}
