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
            this.btnPlayToDevice = new System.Windows.Forms.Button();
            this.tbMediaDir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSelectDir = new System.Windows.Forms.Button();
            this.lvPlaylist = new System.Windows.Forms.ListView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.复制链接ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnDiscoverDevices = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblCurrentMediaInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.cbCurrentDevice = new System.Windows.Forms.ComboBox();
            this.tbLog = new System.Windows.Forms.TextBox();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.btnRefreshPlaylist = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPlayToDevice
            // 
            this.btnPlayToDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlayToDevice.Location = new System.Drawing.Point(455, 414);
            this.btnPlayToDevice.Name = "btnPlayToDevice";
            this.btnPlayToDevice.Size = new System.Drawing.Size(75, 23);
            this.btnPlayToDevice.TabIndex = 8;
            this.btnPlayToDevice.Text = "播放到设备";
            this.btnPlayToDevice.UseVisualStyleBackColor = true;
            this.btnPlayToDevice.Click += new System.EventHandler(this.btnPlayToDevice_Click);
            // 
            // tbMediaDir
            // 
            this.tbMediaDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMediaDir.Location = new System.Drawing.Point(65, 11);
            this.tbMediaDir.Name = "tbMediaDir";
            this.tbMediaDir.ReadOnly = true;
            this.tbMediaDir.Size = new System.Drawing.Size(490, 21);
            this.tbMediaDir.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "媒体目录";
            // 
            // btnSelectDir
            // 
            this.btnSelectDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectDir.Location = new System.Drawing.Point(561, 11);
            this.btnSelectDir.Name = "btnSelectDir";
            this.btnSelectDir.Size = new System.Drawing.Size(96, 23);
            this.btnSelectDir.TabIndex = 2;
            this.btnSelectDir.Text = "选择目录...";
            this.btnSelectDir.UseVisualStyleBackColor = true;
            this.btnSelectDir.Click += new System.EventHandler(this.btnSelectDir_Click);
            // 
            // lvPlaylist
            // 
            this.lvPlaylist.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lvPlaylist.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvPlaylist.ContextMenuStrip = this.contextMenuStrip1;
            this.lvPlaylist.HideSelection = false;
            this.lvPlaylist.Location = new System.Drawing.Point(9, 38);
            this.lvPlaylist.MultiSelect = false;
            this.lvPlaylist.Name = "lvPlaylist";
            this.lvPlaylist.ShowItemToolTips = true;
            this.lvPlaylist.Size = new System.Drawing.Size(729, 370);
            this.lvPlaylist.TabIndex = 1;
            this.lvPlaylist.UseCompatibleStateImageBehavior = false;
            this.lvPlaylist.View = System.Windows.Forms.View.List;
            this.lvPlaylist.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvPlaylist_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.复制链接ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(125, 26);
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
            this.btnDiscoverDevices.Location = new System.Drawing.Point(663, 414);
            this.btnDiscoverDevices.Name = "btnDiscoverDevices";
            this.btnDiscoverDevices.Size = new System.Drawing.Size(75, 23);
            this.btnDiscoverDevices.TabIndex = 1;
            this.btnDiscoverDevices.Text = "重找设备";
            this.btnDiscoverDevices.UseVisualStyleBackColor = true;
            this.btnDiscoverDevices.Click += new System.EventHandler(this.btnDiscoverDevices_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblCurrentMediaInfo});
            this.statusStrip1.Location = new System.Drawing.Point(0, 656);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(744, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblCurrentMediaInfo
            // 
            this.lblCurrentMediaInfo.Name = "lblCurrentMediaInfo";
            this.lblCurrentMediaInfo.Size = new System.Drawing.Size(0, 17);
            // 
            // cbCurrentDevice
            // 
            this.cbCurrentDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbCurrentDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCurrentDevice.FormattingEnabled = true;
            this.cbCurrentDevice.Location = new System.Drawing.Point(536, 414);
            this.cbCurrentDevice.Name = "cbCurrentDevice";
            this.cbCurrentDevice.Size = new System.Drawing.Size(121, 20);
            this.cbCurrentDevice.TabIndex = 10;
            this.cbCurrentDevice.SelectedIndexChanged += new System.EventHandler(this.cbCurrentDevice_SelectedIndexChanged);
            // 
            // tbLog
            // 
            this.tbLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLog.Location = new System.Drawing.Point(9, 444);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ReadOnly = true;
            this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbLog.Size = new System.Drawing.Size(729, 209);
            this.tbLog.TabIndex = 11;
            this.tbLog.WordWrap = false;
            // 
            // btnClearLog
            // 
            this.btnClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClearLog.Location = new System.Drawing.Point(9, 414);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(75, 23);
            this.btnClearLog.TabIndex = 12;
            this.btnClearLog.Text = "清空日志";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // btnRefreshPlaylist
            // 
            this.btnRefreshPlaylist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshPlaylist.Location = new System.Drawing.Point(663, 11);
            this.btnRefreshPlaylist.Name = "btnRefreshPlaylist";
            this.btnRefreshPlaylist.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshPlaylist.TabIndex = 13;
            this.btnRefreshPlaylist.Text = "刷新";
            this.btnRefreshPlaylist.UseVisualStyleBackColor = true;
            this.btnRefreshPlaylist.Click += new System.EventHandler(this.btnRefreshPlaylist_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 678);
            this.Controls.Add(this.btnRefreshPlaylist);
            this.Controls.Add(this.lvPlaylist);
            this.Controls.Add(this.btnClearLog);
            this.Controls.Add(this.tbLog);
            this.Controls.Add(this.cbCurrentDevice);
            this.Controls.Add(this.btnPlayToDevice);
            this.Controls.Add(this.btnDiscoverDevices);
            this.Controls.Add(this.tbMediaDir);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSelectDir);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DLNA Player";
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
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
        private System.Windows.Forms.Button btnPlayToDevice;
        private System.Windows.Forms.ComboBox cbCurrentDevice;
        private System.Windows.Forms.TextBox tbLog;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.Button btnRefreshPlaylist;
    }
}

