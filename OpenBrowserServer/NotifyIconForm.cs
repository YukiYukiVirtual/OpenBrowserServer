using System;
using System.Windows.Forms;
using OpenBrowserServer.Component;
using OpenBrowserServer.Logger;

namespace OpenBrowserServer
{
    public partial class NotifyIconForm : Form
    {
        readonly Settings settings;
        readonly History history;
        readonly VRChatLogWatcher vrchatLogWatcher;

        ControlPanelForm controlPanelForm = null;
        public NotifyIconForm(Settings settings, History history, VRChatLogWatcher vrchatLogWatcher)
        {
            this.settings = settings;
            this.history = history;
            this.vrchatLogWatcher = vrchatLogWatcher;

            InitializeComponent();
            this.Visible = false;
        }

        // ================================================================================
        private void OpenControlPanel()
        {
            if(controlPanelForm == null)
            {
                controlPanelForm = new ControlPanelForm(settings, history, vrchatLogWatcher);
                controlPanelForm.Visible = true;
            }
        }
        // ================================================================================
        /// <summary>
        /// ダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenControlPanel();
        }
        /// <summary>
        /// コントロールパネル
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenControlPanel();
        }
        /// <summary>
        /// 設定更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("設定ファイルをダウンロードして再読み込みしますか？", "更新確認", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            { 
                settings.Update();
                if (settings.NeedUpgrade())
                {
                    if (UpdateDialog.Confirm())
                    {
                        this.Close();
                        Application.Exit();
                    }
                }
            }
        }
        /// <summary>
        /// フォルダを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void folderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            URLOpener.StaticOpen(".");
        }
        /// <summary>
        /// 終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }
    }
}
