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
        private static readonly HttpClient client = new HttpClient(); // 設定ファイルを更新する用のHTTPクライアント

        public readonly string FileVersion;
        public string Version { get; private set; }
        public int IdlePeriod { get; private set; }
        public HashSet<string> Protocol { get; private set; }
        public HashSet<string> Domain { get; private set; }
        public Settings(FileVersionInfo fileVersionInfo)
        {
            Clear();
            FileVersion = $"v{fileVersionInfo.ProductMajorPart}.{fileVersionInfo.ProductMinorPart}.{fileVersionInfo.ProductPrivatePart}";
            Console.WriteLine(ToString());
        }
        public void Clear()
        {
            Version = "unknown";
            IdlePeriod = 500;
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
            string yaml = Download();
            ImportYaml(yaml);
        }
        public bool NeedUpgrade()
        {
            return !FileVersion.Equals(Version);
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
                    var item = (YamlScalarNode)root.Children[new YamlScalarNode("Version")];
                    Version = item.Value;
                }
                catch (KeyNotFoundException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Versionキーが不在");
                }
                try
                {
                    var item = (YamlScalarNode)root.Children[new YamlScalarNode("IdlePeriod")];
                    try
                    {
                        IdlePeriod = int.Parse(item.Value);
                    }
                    catch(ArgumentNullException e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.WriteLine($"IdlePeriodキーの値が不正 '{item.Value}'");
                    }
                    catch(FormatException e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.WriteLine($"IdlePeriodキーの値が不正 '{item.Value}'");
                    }
                    catch(OverflowException e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.WriteLine($"IdlePeriodキーの値がintの範囲外 '{item.Value}'");
                    }
                }
                catch (KeyNotFoundException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine($"IdlePeriodキーが不在");
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
                    Console.WriteLine(e.ToString());
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
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Domainキーが不在");
                }

                Console.WriteLine(this.ToString());
            }
        }
        public override string ToString()
        {

            var serializer = new Serializer();
            return serializer.Serialize(this).Trim();
        }
    }
}
