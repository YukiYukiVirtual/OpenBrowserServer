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

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenControlPanel();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenControlPanel();
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void folderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opener.Open(".");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }
    }
}
