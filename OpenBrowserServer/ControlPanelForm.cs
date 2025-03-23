using System;
using System.Net.Http;
using System.Net;
using System.Security.Policy;
using System.Windows.Forms;
using OpenBrowserServer.Component;
using OpenBrowserServer.Logger;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.IO;
using System.Drawing;

namespace OpenBrowserServer
{
    public partial class ControlPanelForm : Form
    {
        readonly History history;
        readonly VRChatLogWatcher vrchatLogWatcher;
        public ControlPanelForm(Settings settings, History history, VRChatLogWatcher vrchatLogWatcher)
        {
            this.history = history;
            this.vrchatLogWatcher = vrchatLogWatcher;

            InitializeComponent();

            textBoxAllowedDomainList.Text = string.Join("\r\n", settings.Domain);
            textBoxAllowedProtocolList.Text = string.Join("\r\n", settings.Protocol);
        }

        private void buttonOpenLogDirectory_Click(object sender, System.EventArgs e)
        {
            URLOpener.StaticOpen("Log");
        }

        private void timerOfUpdate_Tick(object sender, System.EventArgs e)
        {
            //if (vrchatLogWatcher.NowWorldName == null || vrchatLogWatcher.NowWorldId == null) return; // 未設定なら何もしない

            string url = $"https://vrchat.com/home/world/wrld_ddbe108f-0c70-43e7-8a13-bd5cb0f60c45";// {vrchatLogWatcher.NowWorldId}";
            if (linkLabelOfWorld.Text == url) return; // ワールドIDが変わってなければ何もしない

            this.linkLabelOfWorld.Text = url;

            // JSONを取ってきてワールド情報を書く
            using (HttpClient client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                try
                {
                    string apiUrl = $"https://api.vrchat.cloud/api/1/worlds/wrld_ddbe108f-0c70-43e7-8a13-bd5cb0f60c45";// {vrchatLogWatcher.NowWorldId}";
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
                    // JSONダウンロード
                    string responseBody = client.GetStringAsync(apiUrl).Result;
                    var jsonDocument = JsonDocument.Parse(responseBody).RootElement;

                    // サムネイル
                    string imageUrl = jsonDocument.GetProperty("thumbnailImageUrl").GetString();
                    Stream stream = client.GetStreamAsync(imageUrl).Result;
                    this.worldImageBox.Image = Image.FromStream(stream);
                    // ワールド名
                    this.labelOfWorldName.Text = jsonDocument.GetProperty("name").GetString();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }

        private void linkLabelOfWorld_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var url = linkLabelOfWorld.Text;
            if(string.IsNullOrEmpty(url)) return;
            URLOpener.StaticOpen(url);
        }
    }
}
