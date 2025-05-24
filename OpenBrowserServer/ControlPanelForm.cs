using System;
using System.Windows.Forms;
using OpenBrowserServer.Component;
using OpenBrowserServer.Logger;

namespace OpenBrowserServer
{
    public partial class ControlPanelForm : Form
    {
        readonly Config config;
        readonly History history;
        readonly VRChatLogWatcher vrchatLogWatcher;
        public ControlPanelForm(Config config, History history, VRChatLogWatcher vrchatLogWatcher)
        {
            this.config = config;
            this.history = history;
            this.vrchatLogWatcher = vrchatLogWatcher;

            InitializeComponent();
            LoadSettingTexts();
            updateButton();
        }
        private void LoadSettingTexts()
        {
            textBoxAllowedDomainList.Text = string.Join("\r\n", config.Setting.Domain);
            textBoxAllowedProtocolList.Text = string.Join("\r\n", config.Setting.Protocol);
        }
        private void updateWorldInfo()
        {
            // worldIDがnullの時は何も表示しない
            if (string.IsNullOrEmpty(vrchatLogWatcher.NowWorldId))
            {
                this.textWorldName.Text = "ワールド名";
                this.textWorldDescription.Text = "ワールドの説明";
                this.textAuthorName.Text = "作者";
                this.worldImageBox.Image = null;
                this.linkLabelOfWorld.Text = "URL";
                return;
            }
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
        private void updateButton()
        {
            this.buttonAllowOldInterface.Text = config.OpenBrowserToken.AllowOldInterface ?
                "旧バージョンの動作を許可済み" :
                "旧バージョンの動作を許可する";
     
            this.buttonPauseResume.Text = config.PauseSystem ?
                "再開する" :
                "一時停止する";
        }
        private void timerOfUpdate_Tick(object sender, System.EventArgs e)
        {
            updateWorldInfo();
            updateButton();
        }

        private void linkLabelOfWorld_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = linkLabelOfWorld.Text;
            if (!url.StartsWith("http")) return;
            URLOpener.StaticOpen(url);
        }

        private void buttonOpenDirectory_Click(object sender, EventArgs e)
        {
            URLOpener.StaticOpen(config.WorkingPath);
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (DialogWrapper.DownloadConfirm())
            {
                config.Update();
                LoadSettingTexts();
                if (config.NeedUpgrade())
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

        private void buttonPauseResume_Click(object sender, EventArgs e)
        {
            config.PauseSystem = !config.PauseSystem;
            updateButton();
        }
        private void buttonReload_Click(object sender, EventArgs e)
        {
            config.Reload();
            LoadSettingTexts();
            if (config.NeedUpgrade())
            {
                if (DialogWrapper.UpdateConfirm())
                {
                    this.Close();
                    Application.Exit();
                }
            }
            DialogWrapper.Completed();
        }

        private void buttonReadme_Click(object sender, EventArgs e)
        {
            URLOpener.StaticOpen("https://github.com/YukiYukiVirtual/OpenBrowserServer/blob/master/README.md");
        }

        private void buttonAllowOldInterface_Click(object sender, EventArgs e)
        {
            config.OpenBrowserToken.AllowOldInterface = !config.OpenBrowserToken.AllowOldInterface;
            updateButton();
        }
    }
}
