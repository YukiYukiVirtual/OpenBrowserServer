using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenBrowserServer
{
    public partial class NotifyIconForm : Form
    {
        ControlPanelForm controlPanelForm = null;
        public NotifyIconForm()
        {
            InitializeComponent();
            this.Visible = false;
        }

        // ================================================================================
        private void OpenControlPanel()
        {
            if(controlPanelForm == null)
            {
                controlPanelForm = new ControlPanelForm();
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
