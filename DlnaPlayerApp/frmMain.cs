using DlnaLib;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WaitWnd;

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

        private WaitWndFun _waitForm = new WaitWndFun();

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

            // 为了使lblCurrentMediaInfo文字过长时也显示并且在末尾添加省略号
            lblCurrentMediaInfo.Spring = true;
            statusStrip1.Renderer = new StatusStripRenderer();

            if (!string.IsNullOrEmpty(AppConfig.Instance.LastPlayedFile) &&
                !string.IsNullOrEmpty(AppConfig.Instance.LastPlayedDevice))
            {
                lblCurrentMediaInfo.Text = $"上次播放：{Path.GetFileName(AppConfig.Instance.LastPlayedFile)} 播放设备：{AppConfig.Instance.LastPlayedDevice}";
            }

            _dlnaManager.DeviceFound += OnDeviceFound;
            _dlnaManager.DiscoverFinished += OnDiscoverFinished;
            _dlnaManager.PlayPositionInfo += OnPlayPositionInfo;
            _dlnaManager.PlayNext += OnPlayNext;
            _dlnaManager.DevicePropertyChanged += _OnDevicePropertyChanged;
        }

        private void _OnDevicePropertyChanged(object sender, DevicePropertyChangedEventArgs e)
        {
            DeviceConfig.Default.SaveConfig();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            tbMediaDir.Text = AppConfig.Instance.MediaDir;

            LoadPlaylist(AppConfig.Instance.MediaDir, true);

            if (DeviceConfig.Default.Devices.Count <= 0)
            {
                btnDiscoverDevices.PerformClick();
            }
            else
            {
                foreach (var device in DeviceConfig.Default.Devices)
                {
                    AddDevice(device, false);
                }
                cbCurrentDevice.SelectedItem = DeviceConfig.Default.CurrentDevice ?? DeviceConfig.Default.Devices[0];
            }
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
            DeviceConfig.Default.Devices.Clear();
            DeviceConfig.Default.CurrentDevice = null;
            DeviceConfig.Default.SaveConfig();
            btnDiscoverDevices.Enabled = false;

            _dlnaManager.DiscoverDLNADevices();
        }

        private void cbCurrentDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            _dlnaManager.CurrentDevice = (DlnaDevice)cbCurrentDevice.SelectedItem;
            DeviceConfig.Default.CurrentDevice = (DlnaDevice)cbCurrentDevice.SelectedItem;
            DeviceConfig.Default.SaveConfig();
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
            _waitForm.Show(this, () =>
            {
                try
                {
                    if (lvPlaylist.Items.Count <= 0)
                    {
                        LogUtils.Warn(logger, "播放媒体文件失败，播放列表为空");
                        return;
                    }

                    if (_dlnaManager.CurrentDevice == null)
                    {
                        LogUtils.Warn(logger, "播放媒体文件失败，未选择播放设备");
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
                catch (Exception ex)
                {
                    LogUtils.Error(logger, $"播放失败：{ex.Message}");
                }
            });
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            tbLog.Clear();
        }

        private void lvPlaylist_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 获取双击的ListView项目
            //ListViewHitTestInfo hitTest = lvPlaylist.HitTest(e.Location);
            //ListViewItem item = hitTest.Item;
            if (lvPlaylist.SelectedItems.Count <= 0)
            {
                return;
            }

            var item = lvPlaylist.SelectedItems[0];

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

        private void btnResume_Click(object sender, EventArgs e)
        {
            if (_dlnaManager.CurrentDevice == null)
            {
                LogUtils.Error(logger, "恢复播放失败，未选择设备");
                return;
            }
            _waitForm.Show(this, () =>
            {
                _dlnaManager.ResumePlayback();
                _dlnaManager.CurrentDevice.ExpectState = EnumTransportState.PLAYING;
                DeviceConfig.Default.SaveConfig();
            });
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (_dlnaManager.CurrentDevice == null)
            {
                LogUtils.Error(logger, "暂停播放失败，未选择设备");
                return;
            }
            _waitForm.Show(this, () =>
            {
                _dlnaManager.PausePlayback();
                _dlnaManager.CurrentDevice.ExpectState = EnumTransportState.PAUSED_PLAYBACK;
                DeviceConfig.Default.SaveConfig();
            });
        }
    }
}
