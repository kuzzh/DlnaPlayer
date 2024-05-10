using DlnaLib;
using DlnaLib.Event;
using DlnaLib.Model;
using DlnaLib.Utils;
using DlnaPlayerApp.Config;
using DlnaPlayerApp.Utils;
using DlnaPlayerApp.WebSocket.Protocol;
using log4net;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WaitWnd;

namespace DlnaPlayerApp
{
    public partial class frmMain : Form
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(frmMain));

        private readonly List<DlnaDevice> _dlnaDevices = new List<DlnaDevice>();

        private readonly List<string> SupporetdVideoFormats = new List<string>
        {
            "*.mp4","*.avi","*.wmv","*.mkv"
        };
        private int _currentPlayIndex = -1;

        private readonly WaitWndFun _waitForm = new WaitWndFun();

        private readonly List<VideoItem> _videoItems = new List<VideoItem>();

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

        public static frmMain MainForm;

        public frmMain()
        {
            MainForm = this;

            InitializeComponent();

            InitLogAppender();

            SetListViewItemHeight(20);

            // 为了使lblCurrentMediaInfo文字过长时也显示并且在末尾添加省略号
            lblCurrentMediaInfo.Spring = true;
            statusStrip1.Renderer = new StatusStripRenderer();

            if (!string.IsNullOrEmpty(AppConfig.Default.LastPlayedFile) &&
                !string.IsNullOrEmpty(AppConfig.Default.LastPlayedDevice))
            {
                lblCurrentMediaInfo.Text = $"上次播放：{Path.GetFileName(AppConfig.Default.LastPlayedFile)} 播放设备：{AppConfig.Default.LastPlayedDevice}";
            }

            DlnaManager.Instance.DeviceFound += OnDeviceFound;
            DlnaManager.Instance.DiscoverFinished += OnDiscoverFinished;
            DlnaManager.Instance.PlayPositionInfo += OnPlayPositionInfo;
            DlnaManager.Instance.PlayNext += OnPlayNext;
            DlnaManager.Instance.DevicePropertyChanged += OnDevicePropertyChanged;
        }

        private void OnDevicePropertyChanged(object sender, DevicePropertyChangedEventArgs e)
        {
            DeviceConfig.Default.SaveConfig();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            tbMediaDir.Text = AppConfig.Default.MediaDir;

            LoadPlaylist(AppConfig.Default.MediaDir, true);

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
            if (MessageBox.Show("确定要退出吗？", "询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }

        private void btnDiscoverDevices_Click(object sender, EventArgs e)
        {
            _dlnaDevices.Clear();
            cbCurrentDevice.Items.Clear();
            DeviceConfig.Default.Devices.Clear();
            DeviceConfig.Default.CurrentDevice = null;
            DeviceConfig.Default.SaveConfig();
            btnDiscoverDevices.Enabled = false;

            DlnaManager.Instance.DiscoverDLNADevices();
        }

        private void cbCurrentDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            DlnaManager.Instance.CurrentDevice = (DlnaDevice)cbCurrentDevice.SelectedItem;
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
                }
            }
        }

        private void 复制链接ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvPlaylist.SelectedItems.Count <= 0 || DlnaManager.Instance.CurrentDevice == null)
            {
                return;
            }

            var mediaLinkBuilder = new StringBuilder();
            foreach (ListViewItem item in lvPlaylist.SelectedItems)
            {
                mediaLinkBuilder.AppendLine(AppHelper.BuildMediaUrl(item.Tag.ToString(), DlnaManager.Instance.CurrentDevice.BaseUrl));
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

                    if (DlnaManager.Instance.CurrentDevice == null)
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
            if (lvPlaylist.SelectedItems.Count <= 0)
            {
                return;
            }

            var item = lvPlaylist.SelectedItems[0];

            if (item != null)
            {
                _currentPlayIndex = item.Index - 1;
                OnPlayNext(null, new EventArgs());
            }
        }

        private void btnRefreshPlaylist_Click(object sender, EventArgs e)
        {
            LoadPlaylist(AppConfig.Default.MediaDir, false);
        }

        private void btnResume_Click(object sender, EventArgs e)
        {
            if (DlnaManager.Instance.CurrentDevice == null)
            {
                LogUtils.Error(logger, "继续播放失败，未选择设备");
                return;
            }
            _waitForm.Show(this, () =>
            {
                if (!DlnaManager.Instance.ResumePlayback(out string errorMsg))
                {
                    LogUtils.Error(logger, errorMsg);
                }
                else
                {
                    DlnaManager.Instance.CurrentDevice.ExpectState = EnumTransportState.PLAYING;
                    DeviceConfig.Default.SaveConfig();
                }
            });
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (DlnaManager.Instance.CurrentDevice == null)
            {
                LogUtils.Error(logger, "暂停播放失败，未选择设备");
                return;
            }
            _waitForm.Show(this, () =>
            {
                if (!DlnaManager.Instance.PausePlayback(out string errorMsg))
                {
                    LogUtils.Error(logger, errorMsg);
                }
                else
                {
                    DlnaManager.Instance.CurrentDevice.ExpectState = EnumTransportState.PAUSED_PLAYBACK;
                    DeviceConfig.Default.SaveConfig();
                }
            });
        }

        private void btnQRCode_Click(object sender, EventArgs e)
        {
            if (DlnaManager.Instance.CurrentDevice == null)
            {
                LogUtils.Warn(logger, "未选择设备");
                return;
            }

            var qrForm = new Form
            {
                Text = "Web 二维码",
                Size = new Size(400, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            var qrImage = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = AppHelper.GenerateQRCodeImage($"{AppHelper.GetWebBaseUrl(DlnaManager.Instance.CurrentDevice.BaseUrl)}/control.html"),
            };
            qrForm.Controls.Add(qrImage);

            qrForm.ShowDialog(this);
        }
    }
}
