namespace DlnaPlayerApp
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.btnPlayOrPause = new System.Windows.Forms.Button();
            this.tbMediaDir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSelectDir = new System.Windows.Forms.Button();
            this.lvPlaylist = new System.Windows.Forms.ListView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.播放ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.复制链接ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnDiscoverDevices = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblCurrentMediaInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.cbCurrentDevice = new System.Windows.Forms.ComboBox();
            this.tbLog = new System.Windows.Forms.TextBox();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.btnRefreshPlaylist = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnQRCode = new System.Windows.Forms.Button();
            this.btnContinuePlay = new System.Windows.Forms.Button();
            this.btnMRU = new System.Windows.Forms.Button();
            this.tbSkipTime = new System.Windows.Forms.MaskedTextBox();
            this.btnSkipTime = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPlayOrPause
            // 
            this.btnPlayOrPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlayOrPause.Location = new System.Drawing.Point(844, 646);
            this.btnPlayOrPause.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPlayOrPause.Name = "btnPlayOrPause";
            this.btnPlayOrPause.Size = new System.Drawing.Size(100, 29);
            this.btnPlayOrPause.TabIndex = 8;
            this.btnPlayOrPause.Text = "播放";
            this.btnPlayOrPause.UseVisualStyleBackColor = true;
            this.btnPlayOrPause.Click += new System.EventHandler(this.btnPlayOrPause_Click);
            // 
            // tbMediaDir
            // 
            this.tbMediaDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMediaDir.Location = new System.Drawing.Point(87, 14);
            this.tbMediaDir.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbMediaDir.Name = "tbMediaDir";
            this.tbMediaDir.ReadOnly = true;
            this.tbMediaDir.Size = new System.Drawing.Size(889, 25);
            this.tbMediaDir.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "媒体目录";
            // 
            // btnSelectDir
            // 
            this.btnSelectDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectDir.Location = new System.Drawing.Point(985, 14);
            this.btnSelectDir.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSelectDir.Name = "btnSelectDir";
            this.btnSelectDir.Size = new System.Drawing.Size(128, 29);
            this.btnSelectDir.TabIndex = 2;
            this.btnSelectDir.Text = "选择目录...";
            this.btnSelectDir.UseVisualStyleBackColor = true;
            this.btnSelectDir.Click += new System.EventHandler(this.btnSelectDir_Click);
            // 
            // lvPlaylist
            // 
            this.lvPlaylist.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lvPlaylist.ContextMenuStrip = this.contextMenuStrip1;
            this.lvPlaylist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPlaylist.HideSelection = false;
            this.lvPlaylist.Location = new System.Drawing.Point(8, 26);
            this.lvPlaylist.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lvPlaylist.MultiSelect = false;
            this.lvPlaylist.Name = "lvPlaylist";
            this.lvPlaylist.ShowItemToolTips = true;
            this.lvPlaylist.Size = new System.Drawing.Size(1191, 557);
            this.lvPlaylist.TabIndex = 1;
            this.lvPlaylist.UseCompatibleStateImageBehavior = false;
            this.lvPlaylist.View = System.Windows.Forms.View.List;
            this.lvPlaylist.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvPlaylist_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.播放ToolStripMenuItem,
            this.toolStripSeparator1,
            this.复制链接ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(125, 54);
            // 
            // 播放ToolStripMenuItem
            // 
            this.播放ToolStripMenuItem.Name = "播放ToolStripMenuItem";
            this.播放ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.播放ToolStripMenuItem.Text = "播放";
            this.播放ToolStripMenuItem.Click += new System.EventHandler(this.播放ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(121, 6);
            // 
            // 复制链接ToolStripMenuItem
            // 
            this.复制链接ToolStripMenuItem.Name = "复制链接ToolStripMenuItem";
            this.复制链接ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.复制链接ToolStripMenuItem.Text = "复制链接";
            this.复制链接ToolStripMenuItem.Click += new System.EventHandler(this.复制链接ToolStripMenuItem_Click);
            // 
            // btnDiscoverDevices
            // 
            this.btnDiscoverDevices.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDiscoverDevices.Location = new System.Drawing.Point(1121, 646);
            this.btnDiscoverDevices.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnDiscoverDevices.Name = "btnDiscoverDevices";
            this.btnDiscoverDevices.Size = new System.Drawing.Size(100, 29);
            this.btnDiscoverDevices.TabIndex = 1;
            this.btnDiscoverDevices.Text = "重找设备";
            this.btnDiscoverDevices.UseVisualStyleBackColor = true;
            this.btnDiscoverDevices.Click += new System.EventHandler(this.btnDiscoverDevices_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblCurrentMediaInfo});
            this.statusStrip1.Location = new System.Drawing.Point(0, 954);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1229, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblCurrentMediaInfo
            // 
            this.lblCurrentMediaInfo.Name = "lblCurrentMediaInfo";
            this.lblCurrentMediaInfo.Size = new System.Drawing.Size(0, 17);
            this.lblCurrentMediaInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbCurrentDevice
            // 
            this.cbCurrentDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbCurrentDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCurrentDevice.FormattingEnabled = true;
            this.cbCurrentDevice.Location = new System.Drawing.Point(952, 646);
            this.cbCurrentDevice.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbCurrentDevice.Name = "cbCurrentDevice";
            this.cbCurrentDevice.Size = new System.Drawing.Size(160, 23);
            this.cbCurrentDevice.TabIndex = 10;
            this.cbCurrentDevice.SelectedIndexChanged += new System.EventHandler(this.cbCurrentDevice_SelectedIndexChanged);
            // 
            // tbLog
            // 
            this.tbLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLog.Location = new System.Drawing.Point(12, 684);
            this.tbLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ReadOnly = true;
            this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbLog.Size = new System.Drawing.Size(1208, 260);
            this.tbLog.TabIndex = 11;
            this.tbLog.WordWrap = false;
            // 
            // btnClearLog
            // 
            this.btnClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClearLog.Location = new System.Drawing.Point(12, 646);
            this.btnClearLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(100, 29);
            this.btnClearLog.TabIndex = 12;
            this.btnClearLog.Text = "清空日志";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // btnRefreshPlaylist
            // 
            this.btnRefreshPlaylist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshPlaylist.Location = new System.Drawing.Point(1121, 14);
            this.btnRefreshPlaylist.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnRefreshPlaylist.Name = "btnRefreshPlaylist";
            this.btnRefreshPlaylist.Size = new System.Drawing.Size(100, 29);
            this.btnRefreshPlaylist.TabIndex = 13;
            this.btnRefreshPlaylist.Text = "刷新";
            this.btnRefreshPlaylist.UseVisualStyleBackColor = true;
            this.btnRefreshPlaylist.Click += new System.EventHandler(this.btnRefreshPlaylist_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lvPlaylist);
            this.groupBox1.Location = new System.Drawing.Point(12, 48);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.groupBox1.Size = new System.Drawing.Size(1207, 591);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "播放列表";
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Location = new System.Drawing.Point(736, 646);
            this.btnStop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 29);
            this.btnStop.TabIndex = 15;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnQRCode
            // 
            this.btnQRCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnQRCode.Location = new System.Drawing.Point(120, 646);
            this.btnQRCode.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnQRCode.Name = "btnQRCode";
            this.btnQRCode.Size = new System.Drawing.Size(119, 29);
            this.btnQRCode.TabIndex = 17;
            this.btnQRCode.Text = "扫码手机访问";
            this.btnQRCode.UseVisualStyleBackColor = true;
            this.btnQRCode.Click += new System.EventHandler(this.btnQRCode_Click);
            // 
            // btnContinuePlay
            // 
            this.btnContinuePlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnContinuePlay.Location = new System.Drawing.Point(604, 646);
            this.btnContinuePlay.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnContinuePlay.Name = "btnContinuePlay";
            this.btnContinuePlay.Size = new System.Drawing.Size(124, 29);
            this.btnContinuePlay.TabIndex = 18;
            this.btnContinuePlay.Text = "继续上次播放";
            this.btnContinuePlay.UseVisualStyleBackColor = true;
            this.btnContinuePlay.Click += new System.EventHandler(this.btnContinuePlay_Click);
            // 
            // btnMRU
            // 
            this.btnMRU.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMRU.Location = new System.Drawing.Point(247, 646);
            this.btnMRU.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnMRU.Name = "btnMRU";
            this.btnMRU.Size = new System.Drawing.Size(100, 29);
            this.btnMRU.TabIndex = 19;
            this.btnMRU.Text = "最近播放";
            this.btnMRU.UseVisualStyleBackColor = true;
            this.btnMRU.Click += new System.EventHandler(this.btnMRU_Click);
            // 
            // tbSkipTime
            // 
            this.tbSkipTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSkipTime.Location = new System.Drawing.Point(463, 648);
            this.tbSkipTime.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbSkipTime.Mask = "00:90:00";
            this.tbSkipTime.Name = "tbSkipTime";
            this.tbSkipTime.Size = new System.Drawing.Size(132, 25);
            this.tbSkipTime.TabIndex = 21;
            this.tbSkipTime.Text = "000000";
            this.tbSkipTime.ValidatingType = typeof(System.DateTime);
            this.tbSkipTime.TextChanged += new System.EventHandler(this.tbSkipTime_TextChanged);
            // 
            // btnSkipTime
            // 
            this.btnSkipTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSkipTime.Location = new System.Drawing.Point(356, 646);
            this.btnSkipTime.Name = "btnSkipTime";
            this.btnSkipTime.Size = new System.Drawing.Size(100, 29);
            this.btnSkipTime.TabIndex = 22;
            this.btnSkipTime.Text = "跳过片头";
            this.btnSkipTime.UseVisualStyleBackColor = true;
            this.btnSkipTime.Click += new System.EventHandler(this.btnSkipTime_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1229, 976);
            this.Controls.Add(this.btnSkipTime);
            this.Controls.Add(this.tbSkipTime);
            this.Controls.Add(this.btnMRU);
            this.Controls.Add(this.btnContinuePlay);
            this.Controls.Add(this.btnQRCode);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnRefreshPlaylist);
            this.Controls.Add(this.btnClearLog);
            this.Controls.Add(this.tbLog);
            this.Controls.Add(this.cbCurrentDevice);
            this.Controls.Add(this.btnPlayOrPause);
            this.Controls.Add(this.btnDiscoverDevices);
            this.Controls.Add(this.tbMediaDir);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSelectDir);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "frmMain";
            this.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DLNA Player";
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnDiscoverDevices;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblCurrentMediaInfo;
        private System.Windows.Forms.ListView lvPlaylist;
        private System.Windows.Forms.Button btnSelectDir;
        private System.Windows.Forms.TextBox tbMediaDir;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 复制链接ToolStripMenuItem;
        private System.Windows.Forms.Button btnPlayOrPause;
        private System.Windows.Forms.ComboBox cbCurrentDevice;
        private System.Windows.Forms.TextBox tbLog;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.Button btnRefreshPlaylist;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnQRCode;
        private System.Windows.Forms.ToolStripMenuItem 播放ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Button btnContinuePlay;
        private System.Windows.Forms.Button btnMRU;
        private System.Windows.Forms.MaskedTextBox tbSkipTime;
        private System.Windows.Forms.Button btnSkipTime;
    }
}

