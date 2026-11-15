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
        bool updatingWorldInfo = false;
        string worldInfoLoadedWorldId = null; // 取得に成功したワールドID(同じワールドを取り直さないため)
        DateTime worldInfoRetryAfter = DateTime.MinValue; // 取得失敗後、取り直しを始める時刻
        private async void updateWorldInfo()
        {
            // worldIDがnullの時は何も表示しない
            if (string.IsNullOrEmpty(vrchatLogWatcher.NowWorldId))
            {
                this.textWorldName.Text = "ワールド名";
                this.textWorldDescription.Text = "ワールドの説明";
                this.textAuthorName.Text = "作者";
                this.worldImageBox.Image = null;
                this.linkLabelOfWorld.Text = "URL";
                worldInfoLoadedWorldId = null;
                return;
            }
            string worldId = vrchatLogWatcher.NowWorldId;
            string url = $"https://vrchat.com/home/world/{worldId}";
            if (worldInfoLoadedWorldId == worldId) return; // このワールドは取得済み
            if (updatingWorldInfo) return; // 取得中は重ねて取得しない
            if (DateTime.Now < worldInfoRetryAfter) return; // 失敗直後は少し間隔を空ける

            this.linkLabelOfWorld.Text = url;

            // JSONを取ってきてワールド情報を書く(UIスレッドを止めないよう非同期で取得する)
            updatingWorldInfo = true;
            try
            {
                if (await JsonDownloader.CacheWorldInformationAsync(worldId))
                {
                    worldInfoLoadedWorldId = worldId;
                    this.textWorldName.Text = JsonDownloader.CachedWorldName;
                    this.textWorldDescription.Text = JsonDownloader.CachedDescription;
                    this.textAuthorName.Text = $"by {JsonDownloader.CachedAuthorName}";
                    this.worldImageBox.Image = JsonDownloader.CachedThumbnailImage;
                }
                else
                {
                    // 失敗したら、少し間隔を空けて次のタイマーで取り直す
                    worldInfoRetryAfter = DateTime.Now.AddSeconds(10);
                    this.textWorldName.Text = "読み込みエラー";
                    this.textWorldDescription.Text = "読み込みエラー";
                    this.textAuthorName.Text = "";
                    this.worldImageBox.Image = null;
                    history.WriteLine($"▲ワールド情報取得失敗 {url} {worldId}");
                }
            }
            finally
            {
                updatingWorldInfo = false;
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

        private async void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (DialogWrapper.DownloadConfirm())
            {
                await config.UpdateAsync();
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
