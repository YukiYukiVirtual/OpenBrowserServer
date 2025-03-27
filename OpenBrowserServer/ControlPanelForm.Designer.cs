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
            this.buttonOpenDirectory = new System.Windows.Forms.Button();
            this.textBoxAllowedDomainList = new System.Windows.Forms.TextBox();
            this.textBoxAllowedProtocolList = new System.Windows.Forms.TextBox();
            this.timerOfUpdate = new System.Windows.Forms.Timer(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.textWorldName = new System.Windows.Forms.TextBox();
            this.worldImageBox = new System.Windows.Forms.PictureBox();
            this.textAuthorName = new System.Windows.Forms.TextBox();
            this.textWorldDescription = new System.Windows.Forms.TextBox();
            this.linkLabelOfWorld = new System.Windows.Forms.LinkLabel();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonOpenLogFile = new System.Windows.Forms.Button();
            this.buttonPauseResume = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.worldImageBox)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOpenDirectory
            // 
            this.buttonOpenDirectory.Font = new System.Drawing.Font("MS UI Gothic", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.buttonOpenDirectory.Location = new System.Drawing.Point(167, 3);
            this.buttonOpenDirectory.Name = "buttonOpenDirectory";
            this.buttonOpenDirectory.Size = new System.Drawing.Size(158, 48);
            this.buttonOpenDirectory.TabIndex = 0;
            this.buttonOpenDirectory.Text = "フォルダを開く";
            this.buttonOpenDirectory.UseVisualStyleBackColor = true;
            this.buttonOpenDirectory.Click += new System.EventHandler(this.buttonOpenDirectory_Click);
            // 
            // textBoxAllowedDomainList
            // 
            this.textBoxAllowedDomainList.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBoxAllowedDomainList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxAllowedDomainList.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBoxAllowedDomainList.Location = new System.Drawing.Point(3, 19);
            this.textBoxAllowedDomainList.Multiline = true;
            this.textBoxAllowedDomainList.Name = "textBoxAllowedDomainList";
            this.textBoxAllowedDomainList.ReadOnly = true;
            this.textBoxAllowedDomainList.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxAllowedDomainList.Size = new System.Drawing.Size(258, 260);
            this.textBoxAllowedDomainList.TabIndex = 1;
            this.textBoxAllowedDomainList.Text = "aaaaa";
            // 
            // textBoxAllowedProtocolList
            // 
            this.textBoxAllowedProtocolList.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBoxAllowedProtocolList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxAllowedProtocolList.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBoxAllowedProtocolList.Location = new System.Drawing.Point(3, 19);
            this.textBoxAllowedProtocolList.Multiline = true;
            this.textBoxAllowedProtocolList.Name = "textBoxAllowedProtocolList";
            this.textBoxAllowedProtocolList.ReadOnly = true;
            this.textBoxAllowedProtocolList.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxAllowedProtocolList.Size = new System.Drawing.Size(258, 75);
            this.textBoxAllowedProtocolList.TabIndex = 3;
            this.textBoxAllowedProtocolList.Text = "bbbbb";
            // 
            // timerOfUpdate
            // 
            this.timerOfUpdate.Enabled = true;
            this.timerOfUpdate.Interval = 1000;
            this.timerOfUpdate.Tick += new System.EventHandler(this.timerOfUpdate_Tick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.flowLayoutPanel3);
            this.groupBox2.Controls.Add(this.linkLabelOfWorld);
            this.groupBox2.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.groupBox2.Location = new System.Drawing.Point(307, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(486, 395);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "今いるワールド";
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.textWorldName);
            this.flowLayoutPanel3.Controls.Add(this.worldImageBox);
            this.flowLayoutPanel3.Controls.Add(this.textAuthorName);
            this.flowLayoutPanel3.Controls.Add(this.textWorldDescription);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 19);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(480, 361);
            this.flowLayoutPanel3.TabIndex = 15;
            // 
            // textWorldName
            // 
            this.textWorldName.BackColor = System.Drawing.SystemColors.Control;
            this.textWorldName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textWorldName.Font = new System.Drawing.Font("MS UI Gothic", 16F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.textWorldName.Location = new System.Drawing.Point(3, 3);
            this.textWorldName.Name = "textWorldName";
            this.textWorldName.Size = new System.Drawing.Size(477, 22);
            this.textWorldName.TabIndex = 16;
            this.textWorldName.Text = "ワールド名";
            // 
            // worldImageBox
            // 
            this.worldImageBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.worldImageBox.Location = new System.Drawing.Point(3, 31);
            this.worldImageBox.Name = "worldImageBox";
            this.worldImageBox.Size = new System.Drawing.Size(256, 192);
            this.worldImageBox.TabIndex = 8;
            this.worldImageBox.TabStop = false;
            // 
            // textAuthorName
            // 
            this.textAuthorName.BackColor = System.Drawing.SystemColors.Control;
            this.textAuthorName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textAuthorName.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Bold);
            this.textAuthorName.Location = new System.Drawing.Point(265, 31);
            this.textAuthorName.Name = "textAuthorName";
            this.textAuthorName.Size = new System.Drawing.Size(212, 16);
            this.textAuthorName.TabIndex = 17;
            this.textAuthorName.Text = "作者";
            // 
            // textWorldDescription
            // 
            this.textWorldDescription.BackColor = System.Drawing.SystemColors.Control;
            this.textWorldDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textWorldDescription.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textWorldDescription.Location = new System.Drawing.Point(3, 229);
            this.textWorldDescription.Multiline = true;
            this.textWorldDescription.Name = "textWorldDescription";
            this.textWorldDescription.ReadOnly = true;
            this.textWorldDescription.Size = new System.Drawing.Size(474, 130);
            this.textWorldDescription.TabIndex = 12;
            this.textWorldDescription.Text = "ワールドの説明";
            // 
            // linkLabelOfWorld
            // 
            this.linkLabelOfWorld.AutoSize = true;
            this.linkLabelOfWorld.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkLabelOfWorld.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.linkLabelOfWorld.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.linkLabelOfWorld.Location = new System.Drawing.Point(3, 380);
            this.linkLabelOfWorld.Name = "linkLabelOfWorld";
            this.linkLabelOfWorld.Size = new System.Drawing.Size(27, 12);
            this.linkLabelOfWorld.TabIndex = 7;
            this.linkLabelOfWorld.TabStop = true;
            this.linkLabelOfWorld.Text = "URL";
            this.linkLabelOfWorld.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOfWorld_LinkClicked);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Font = new System.Drawing.Font("MS UI Gothic", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.buttonUpdate.Location = new System.Drawing.Point(3, 3);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(158, 48);
            this.buttonUpdate.TabIndex = 8;
            this.buttonUpdate.Text = "設定ファイル更新";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // buttonExit
            // 
            this.buttonExit.Font = new System.Drawing.Font("MS UI Gothic", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.buttonExit.Location = new System.Drawing.Point(495, 3);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(158, 48);
            this.buttonExit.TabIndex = 9;
            this.buttonExit.Text = "終了";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.groupBox3);
            this.flowLayoutPanel1.Controls.Add(this.groupBox4);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(289, 395);
            this.flowLayoutPanel1.TabIndex = 10;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textBoxAllowedDomainList);
            this.groupBox3.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(264, 282);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "許可ドメイン一覧";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.textBoxAllowedProtocolList);
            this.groupBox4.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.groupBox4.Location = new System.Drawing.Point(3, 291);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(264, 97);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "許可プロトコル一覧";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.buttonUpdate);
            this.flowLayoutPanel2.Controls.Add(this.buttonOpenDirectory);
            this.flowLayoutPanel2.Controls.Add(this.buttonOpenLogFile);
            this.flowLayoutPanel2.Controls.Add(this.buttonExit);
            this.flowLayoutPanel2.Controls.Add(this.buttonPauseResume);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(12, 413);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(781, 134);
            this.flowLayoutPanel2.TabIndex = 11;
            // 
            // buttonOpenLogFile
            // 
            this.buttonOpenLogFile.Font = new System.Drawing.Font("MS UI Gothic", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.buttonOpenLogFile.Location = new System.Drawing.Point(331, 3);
            this.buttonOpenLogFile.Name = "buttonOpenLogFile";
            this.buttonOpenLogFile.Size = new System.Drawing.Size(158, 48);
            this.buttonOpenLogFile.TabIndex = 10;
            this.buttonOpenLogFile.Text = "ログファイルを開く";
            this.buttonOpenLogFile.UseVisualStyleBackColor = true;
            this.buttonOpenLogFile.Click += new System.EventHandler(this.buttonOpenLogFile_Click);
            // 
            // buttonPauseResume
            // 
            this.buttonPauseResume.Font = new System.Drawing.Font("MS UI Gothic", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.buttonPauseResume.Location = new System.Drawing.Point(3, 57);
            this.buttonPauseResume.Name = "buttonPauseResume";
            this.buttonPauseResume.Size = new System.Drawing.Size(158, 48);
            this.buttonPauseResume.TabIndex = 11;
            this.buttonPauseResume.Text = "一時停止する";
            this.buttonPauseResume.UseVisualStyleBackColor = true;
            this.buttonPauseResume.Click += new System.EventHandler(this.buttonPauseResume_Click);
            // 
            // ControlPanelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(917, 646);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.groupBox2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ControlPanelForm";
            this.Text = "VRChatOpenBrowser - Control Panel";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.worldImageBox)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOpenDirectory;
        private System.Windows.Forms.TextBox textBoxAllowedDomainList;
        private System.Windows.Forms.TextBox textBoxAllowedProtocolList;
        private System.Windows.Forms.Timer timerOfUpdate;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.LinkLabel linkLabelOfWorld;
        private System.Windows.Forms.PictureBox worldImageBox;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button buttonOpenLogFile;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.TextBox textWorldDescription;
        private System.Windows.Forms.TextBox textWorldName;
        private System.Windows.Forms.TextBox textAuthorName;
        private System.Windows.Forms.Button buttonPauseResume;
    }
}