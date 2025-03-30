using System;
using System.Windows.Forms;
using OpenBrowserServer.Component;
using OpenBrowserServer.Logger;

namespace OpenBrowserServer
{
    public partial class NotifyIconForm : Form
    {
        readonly Config config;
        readonly History history;
        readonly VRChatLogWatcher vrchatLogWatcher;

        ControlPanelForm controlPanelForm = null;
        public NotifyIconForm(Config config, History history, VRChatLogWatcher vrchatLogWatcher)
        {
            this.config = config;
            this.history = history;
            this.vrchatLogWatcher = vrchatLogWatcher;

            InitializeComponent();
            this.Visible = false;
        }

        // ================================================================================
        private void OpenControlPanel()
        {
            if(controlPanelForm == null || controlPanelForm.IsDisposed)
            {
                Console.WriteLine("OpenControlPanel open");
                controlPanelForm = new ControlPanelForm(config, history, vrchatLogWatcher)
                {
                    Visible = true
                };
            }
            else
            {
                Console.WriteLine("OpenControlPanel Already open");
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
            if (DialogWrapper.DownloadConfirm())
            { 
                config.Update();
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
        /// <summary>
        /// フォルダを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void folderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            URLOpener.StaticOpen(config.WorkingPath);
        }
        /// <summary>
        /// 終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogWrapper.ExitConfirm())
            {
                this.Close();
                Application.Exit();
            }
        }
    }
}
