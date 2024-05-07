using DlnaLib;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DlnaPlayerApp
{
    public partial class frmMain : Form
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(frmMain));

        private readonly DlnaManager _dlnaManager = new DlnaManager();

        private readonly List<DlnaDevice> _dlnaDevices = new List<DlnaDevice>();

        private readonly List<string> SupporetdVideoFormats = new List<string>
        {
            "*.mp4","*.avi","*.wmv","*.mkv"
        };
        private readonly List<string> SupportedAudioFormats = new List<string>
        {
            "*.mp3", "*.wav", "*.aac"
        };
        private int _currentPlayIndex = -1;

        private ListViewItem SelectedItem
        {
            get
            {
                if (lvPlaylist.Items.Count <= 0 || lvPlaylist.SelectedItems.Count <= 0)
                {
                    return null;
                }
                return lvPlaylist.SelectedItems[0];
            }
        }

        public frmMain()
        {
            InitializeComponent();

            InitLogAppender();

            SetListViewItemHeight(20);

            _dlnaManager.DeviceFound += OnDeviceFound;
            _dlnaManager.DiscoverFinished += OnDiscoverFinished;
            _dlnaManager.PlayMediaInfo += OnPlayMediaInfo;
            _dlnaManager.PlayNext += OnPlayNext;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            tbMediaDir.Text = AppConfig.Instance.MediaDir;

            LoadPlaylist(AppConfig.Instance.MediaDir, true);
            btnDiscoverDevices.PerformClick();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            _dlnaManager.Dispose();
            NginxUtils.StopServer();
        }

        private void btnDiscoverDevices_Click(object sender, EventArgs e)
        {
            _dlnaDevices.Clear();
            cbCurrentDevice.Items.Clear();
            lblCurrentMediaInfo.Text = "";
            btnDiscoverDevices.Enabled = false;

            _dlnaManager.DiscoverDLNADevices();
        }

        private void cbCurrentDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblCurrentMediaInfo.Text = "";
            _dlnaManager.CurrentDevice = (DlnaDevice)cbCurrentDevice.SelectedItem;
        }
        
        private void btnSelectDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var selectedDir = dlg.SelectedPath;
                    if (selectedDir == tbMediaDir.Text)
                    {
                        return;
                    }

                    tbMediaDir.Text = selectedDir;
                    LoadPlaylist(selectedDir, true);

                    SaveConfig();
                }
            }
        }

        private void 复制链接ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvPlaylist.SelectedItems.Count <= 0 || _dlnaManager.CurrentDevice == null)
            {
                return;
            }

            var mediaLinkBuilder = new StringBuilder();
            foreach (ListViewItem item in lvPlaylist.SelectedItems)
            {
                mediaLinkBuilder.AppendLine(AppHelper.BuildMediaUrl(item.Tag.ToString(), _dlnaManager.CurrentDevice.BaseUrl));
            }

            Clipboard.SetText(mediaLinkBuilder.ToString());
        }

        private void btnPlayToDevice_Click(object sender, EventArgs e)
        {
            if (lvPlaylist.Items.Count <= 0)
            {
                logger.Warn("播放媒体文件失败，播放列表为空");
                return;
            }

            if (_dlnaManager.CurrentDevice == null)
            {
                logger.Warn("播放媒体文件失败，未选择播放设备");
                return;
            }

            if (SelectedItem != null)
            {
                _currentPlayIndex = SelectedItem.Index - 1;
            }
            else
            {
                _currentPlayIndex = -1;
            }

            OnPlayNext(sender, EventArgs.Empty);
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            tbLog.Clear();
        }

        private void lvPlaylist_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 获取双击的ListView项目
            ListViewHitTestInfo hitTest = lvPlaylist.HitTest(e.Location);
            ListViewItem item = hitTest.Item;

            // 如果双击了有效的项目
            if (item != null)
            {
                // 执行你的操作，比如显示项目的内容
                _currentPlayIndex = item.Index - 1;
                OnPlayNext(null, new EventArgs());
            }
        }

        private void btnRefreshPlaylist_Click(object sender, EventArgs e)
        {
            LoadPlaylist(AppConfig.Instance.MediaDir, false);
        }
    }
}
