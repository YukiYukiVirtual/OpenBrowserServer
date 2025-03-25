using System;
using System.Drawing;
using System.Windows.Forms;
using OpenBrowserServer.Component;
using OpenBrowserServer.Logger;

namespace OpenBrowserServer
{
    public partial class ControlPanelForm : Form
    {
        readonly Settings settings;
        readonly History history;
        readonly VRChatLogWatcher vrchatLogWatcher;
        public ControlPanelForm(Settings settings, History history, VRChatLogWatcher vrchatLogWatcher)
        {
            this.settings = settings;
            this.history = history;
            this.vrchatLogWatcher = vrchatLogWatcher;

            InitializeComponent();

            textBoxAllowedDomainList.Text = string.Join("\r\n", settings.Domain);
            textBoxAllowedProtocolList.Text = string.Join("\r\n", settings.Protocol);
        }

        private void timerOfUpdate_Tick(object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(vrchatLogWatcher.NowWorldId)) return;
            string url = $"https://vrchat.com/home/world/{vrchatLogWatcher.NowWorldId}";
            if (linkLabelOfWorld.Text == url) return; // ワールドIDが変わってなければ何もしない

            this.linkLabelOfWorld.Text = url;

            // JSONを取ってきてワールド情報を書く
            if(JsonDownloader.CacheWorldInformation(vrchatLogWatcher.NowWorldId))
            {
                this.textWorldName.Text = JsonDownloader.CachedWorldName;
                this.textWorldDescription.Text = JsonDownloader.CachedDescription;
                this.textAuthorName.Text = $"by {JsonDownloader.CachedAuthorName}";
                this.worldImageBox.Image = JsonDownloader.CachedThumbnailImage;
            }
            else
            {
                this.textWorldName.Text = "読み込みエラー";
                this.textWorldDescription.Text = "読み込みエラー";
                this.textAuthorName.Text = "";
                this.worldImageBox.Image = null;
                history.WriteLine($"▲ワールド情報取得失敗 {url} {vrchatLogWatcher.NowWorldId}");
            }

        }

        private void linkLabelOfWorld_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = linkLabelOfWorld.Text;
            if (!url.StartsWith("http")) return;
            URLOpener.StaticOpen(url);
        }

        private void buttonOpenDirectory_Click(object sender, EventArgs e)
        {
            URLOpener.StaticOpen(".");
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (DialogWrapper.DownloadConfirm())
            {
                settings.Update();
                if (settings.NeedUpgrade())
                {
                    if (DialogWrapper.UpdateConfirm())
                    {
                        this.Close();
                        Application.Exit();
                    }
                }
                DialogWrapper.Completed();
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            if (DialogWrapper.ExitConfirm())
            {
                this.Close();
                Application.Exit();
            }
        }

        private void buttonOpenLogFile_Click(object sender, EventArgs e)
        {
            URLOpener.StaticOpen(history.LogFileName);
        }
    }
}
