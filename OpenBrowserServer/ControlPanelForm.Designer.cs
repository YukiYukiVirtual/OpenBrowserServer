namespace OpenBrowserServer
{
    partial class ControlPanelForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlPanelForm));
            this.buttonOpenLogDirectory = new System.Windows.Forms.Button();
            this.textBoxAllowedDomainList = new System.Windows.Forms.TextBox();
            this.labelAllowedDomainList = new System.Windows.Forms.Label();
            this.labelAllowedProtocolList = new System.Windows.Forms.Label();
            this.textBoxAllowedProtocolList = new System.Windows.Forms.TextBox();
            this.timerOfUpdate = new System.Windows.Forms.Timer(this.components);
            this.labelNowWorldBox = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labelOfWorldName = new System.Windows.Forms.Label();
            this.linkLabelOfWorld = new System.Windows.Forms.LinkLabel();
            this.worldImageBox = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.worldImageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOpenLogDirectory
            // 
            this.buttonOpenLogDirectory.Font = new System.Drawing.Font("MS UI Gothic", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.buttonOpenLogDirectory.Location = new System.Drawing.Point(283, 305);
            this.buttonOpenLogDirectory.Name = "buttonOpenLogDirectory";
            this.buttonOpenLogDirectory.Size = new System.Drawing.Size(203, 48);
            this.buttonOpenLogDirectory.TabIndex = 0;
            this.buttonOpenLogDirectory.Text = "ログフォルダを開く";
            this.buttonOpenLogDirectory.UseVisualStyleBackColor = true;
            this.buttonOpenLogDirectory.Click += new System.EventHandler(this.buttonOpenLogDirectory_Click);
            // 
            // textBoxAllowedDomainList
            // 
            this.textBoxAllowedDomainList.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBoxAllowedDomainList.Location = new System.Drawing.Point(0, 21);
            this.textBoxAllowedDomainList.Multiline = true;
            this.textBoxAllowedDomainList.Name = "textBoxAllowedDomainList";
            this.textBoxAllowedDomainList.ReadOnly = true;
            this.textBoxAllowedDomainList.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxAllowedDomainList.Size = new System.Drawing.Size(265, 214);
            this.textBoxAllowedDomainList.TabIndex = 1;
            // 
            // labelAllowedDomainList
            // 
            this.labelAllowedDomainList.AutoSize = true;
            this.labelAllowedDomainList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelAllowedDomainList.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.labelAllowedDomainList.Location = new System.Drawing.Point(0, 0);
            this.labelAllowedDomainList.Name = "labelAllowedDomainList";
            this.labelAllowedDomainList.Size = new System.Drawing.Size(128, 18);
            this.labelAllowedDomainList.TabIndex = 2;
            this.labelAllowedDomainList.Text = "許可ドメイン一覧";
            // 
            // labelAllowedProtocolList
            // 
            this.labelAllowedProtocolList.AutoSize = true;
            this.labelAllowedProtocolList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelAllowedProtocolList.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.labelAllowedProtocolList.Location = new System.Drawing.Point(0, 238);
            this.labelAllowedProtocolList.Name = "labelAllowedProtocolList";
            this.labelAllowedProtocolList.Size = new System.Drawing.Size(141, 18);
            this.labelAllowedProtocolList.TabIndex = 4;
            this.labelAllowedProtocolList.Text = "許可プロトコル一覧";
            // 
            // textBoxAllowedProtocolList
            // 
            this.textBoxAllowedProtocolList.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBoxAllowedProtocolList.Location = new System.Drawing.Point(0, 259);
            this.textBoxAllowedProtocolList.Multiline = true;
            this.textBoxAllowedProtocolList.Name = "textBoxAllowedProtocolList";
            this.textBoxAllowedProtocolList.ReadOnly = true;
            this.textBoxAllowedProtocolList.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxAllowedProtocolList.Size = new System.Drawing.Size(265, 63);
            this.textBoxAllowedProtocolList.TabIndex = 3;
            // 
            // timerOfUpdate
            // 
            this.timerOfUpdate.Enabled = true;
            this.timerOfUpdate.Interval = 1000;
            this.timerOfUpdate.Tick += new System.EventHandler(this.timerOfUpdate_Tick);
            // 
            // labelNowWorldBox
            // 
            this.labelNowWorldBox.AutoSize = true;
            this.labelNowWorldBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelNowWorldBox.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.labelNowWorldBox.Location = new System.Drawing.Point(0, 0);
            this.labelNowWorldBox.Name = "labelNowWorldBox";
            this.labelNowWorldBox.Size = new System.Drawing.Size(107, 18);
            this.labelNowWorldBox.TabIndex = 5;
            this.labelNowWorldBox.Text = "今いるワールド";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelAllowedDomainList);
            this.groupBox1.Controls.Add(this.textBoxAllowedDomainList);
            this.groupBox1.Controls.Add(this.textBoxAllowedProtocolList);
            this.groupBox1.Controls.Add(this.labelAllowedProtocolList);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(265, 341);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.worldImageBox);
            this.groupBox2.Controls.Add(this.linkLabelOfWorld);
            this.groupBox2.Controls.Add(this.labelOfWorldName);
            this.groupBox2.Controls.Add(this.labelNowWorldBox);
            this.groupBox2.Location = new System.Drawing.Point(283, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(484, 287);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // labelOfWorldName
            // 
            this.labelOfWorldName.AutoSize = true;
            this.labelOfWorldName.Font = new System.Drawing.Font("ＭＳ Ｐ明朝", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.labelOfWorldName.Location = new System.Drawing.Point(6, 219);
            this.labelOfWorldName.Name = "labelOfWorldName";
            this.labelOfWorldName.Size = new System.Drawing.Size(84, 16);
            this.labelOfWorldName.TabIndex = 6;
            this.labelOfWorldName.Text = "ワールド名";
            // 
            // linkLabelOfWorld
            // 
            this.linkLabelOfWorld.AutoSize = true;
            this.linkLabelOfWorld.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkLabelOfWorld.Location = new System.Drawing.Point(0, 259);
            this.linkLabelOfWorld.Name = "linkLabelOfWorld";
            this.linkLabelOfWorld.Size = new System.Drawing.Size(27, 12);
            this.linkLabelOfWorld.TabIndex = 7;
            this.linkLabelOfWorld.TabStop = true;
            this.linkLabelOfWorld.Text = "URL";
            this.linkLabelOfWorld.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOfWorld_LinkClicked);
            // 
            // worldImageBox
            // 
            this.worldImageBox.Location = new System.Drawing.Point(0, 18);
            this.worldImageBox.Name = "worldImageBox";
            this.worldImageBox.Size = new System.Drawing.Size(262, 198);
            this.worldImageBox.TabIndex = 8;
            this.worldImageBox.TabStop = false;
            // 
            // ControlPanelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(980, 460);
            this.Controls.Add(this.buttonOpenLogDirectory);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ControlPanelForm";
            this.Text = "VRChatOpenBrowser - Control Panel";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.worldImageBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOpenLogDirectory;
        private System.Windows.Forms.TextBox textBoxAllowedDomainList;
        private System.Windows.Forms.Label labelAllowedDomainList;
        private System.Windows.Forms.Label labelAllowedProtocolList;
        private System.Windows.Forms.TextBox textBoxAllowedProtocolList;
        private System.Windows.Forms.Timer timerOfUpdate;
        private System.Windows.Forms.Label labelNowWorldBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label labelOfWorldName;
        private System.Windows.Forms.LinkLabel linkLabelOfWorld;
        private System.Windows.Forms.PictureBox worldImageBox;
    }
}