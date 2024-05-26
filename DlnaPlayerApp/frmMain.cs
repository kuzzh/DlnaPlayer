using DlnaLib;
using DlnaLib.Event;
using DlnaLib.Model;
using DlnaLib.Utils;
using DlnaPlayerApp.Config;
using DlnaPlayerApp.Utils;
using DlnaPlayerApp.WebSocket;
using DlnaPlayerApp.WebSocket.Protocol;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        // 上一个播放的 ListViewItem
        private ListViewItem _prevPlayingListViewItem;

        private string _seekPosition;

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

            if (!string.IsNullOrEmpty(AppConfig.Default.LastPlayedInfo.LastPlayedFile) &&
                !string.IsNullOrEmpty(AppConfig.Default.LastPlayedInfo.LastPlayedDevice) &&
                !string.IsNullOrEmpty(AppConfig.Default.LastPlayedInfo.LastPlayedTime))
            {
                lblCurrentMediaInfo.Text = $"{AppConfig.Default.LastPlayedInfo.LastPlayedFile} {AppConfig.Default.LastPlayedInfo.LastPlayedTime} {AppConfig.Default.LastPlayedInfo.LastPlayedDevice}";

                btnContinuePlay.Visible = true;
            }
            else
            {
                btnContinuePlay.Visible = false;
            }

            btnPlayOrPause.Enabled = false;
            btnStop.Enabled = false;

            DlnaManager.Instance.DeviceFound += OnDeviceFound;
            DlnaManager.Instance.DiscoverFinished += OnDiscoverFinished;
            DlnaManager.Instance.PlayPositionInfo += OnPlayPositionInfo;
            DlnaManager.Instance.DevicePropertyChanged += OnDevicePropertyChanged;
            EventWebServer.Instance.PlayNext += OnPlayNext;
            EventWebServer.Instance.PlayStateChanged += OnPlayStateChanged;
        }

        private void OnPlayStateChanged(object sender, PlayStateChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnPlayStateChanged(sender, e)));
                return;
            }
            DlnaManager.Instance.CurrentDevice.State = e.State;

            if (e.State == EnumTransportState.PLAYING)
            {
                cbCurrentDevice.Enabled = false;
                btnDiscoverDevices.Enabled = false;
                btnPlayOrPause.Text = "暂停";
                btnPlayOrPause.Enabled = true;
                btnStop.Enabled = true;
                btnContinuePlay.Visible = false;
            }
            else if (e.State == EnumTransportState.PAUSED ||
                e.State == EnumTransportState.PAUSED_PLAYBACK)
            {
                cbCurrentDevice.Enabled = false;
                btnDiscoverDevices.Enabled = false;
                btnPlayOrPause.Text = "播放";
                btnPlayOrPause.Enabled = true;
                btnStop.Enabled = true;
            }
            else if (e.State == EnumTransportState.STOPPED)
            {
                cbCurrentDevice.Enabled = true;
                btnDiscoverDevices.Enabled = true;
                btnPlayOrPause.Text = "播放";
                btnPlayOrPause.Enabled = false;
                btnStop.Enabled = false;
            }
        }

        private void OnDevicePropertyChanged(object sender, DevicePropertyChangedEventArgs e)
        {
            DeviceConfig.Default.SaveConfig();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                NginxUtils.StopServer();
                NginxUtils.StartServer();
                WebSocketManager.Start();
                EventWebServer.Instance.Start(IPAddress.Any, AppConfig.Default.CallbackPort, 100, "");

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
            catch (Exception ex)
            {
                string msg = "";
                GetInnerExceptions(ex, ref msg);
                MessageBox.Show(msg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (MessageBox.Show("确定要退出吗？", "询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            DlnaManager.Instance.Dispose();
            NginxUtils.StopServer();
            EventWebServer.Instance.Stop();
            WebSocketManager.Stop();

            base.OnFormClosing(e);
        }

        private async void btnDiscoverDevices_Click(object sender, EventArgs e)
        {
            foreach (var dlnaDevice in _dlnaDevices)
            {
                await DlnaManager.UnsubscribeAVTransportEventsAsync(dlnaDevice);
            }
            _dlnaDevices.Clear();
            cbCurrentDevice.Items.Clear();
            DeviceConfig.Default.Devices.Clear();
            DeviceConfig.Default.CurrentDevice = null;
            DeviceConfig.Default.SaveConfig();
            btnDiscoverDevices.Enabled = false;

            DlnaManager.Instance.DiscoverDLNADevices();
        }

        private async void cbCurrentDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            var currentDevice = (DlnaDevice)cbCurrentDevice.SelectedItem;

            DlnaManager.Instance.CurrentDevice = currentDevice;
            DeviceConfig.Default.CurrentDevice = currentDevice;
            DeviceConfig.Default.SaveConfig();

            await DlnaManager.SubscribeAVTransportEvents(currentDevice, AppHelper.BuildCallbackUrl(currentDevice.BaseUrl));
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

        private void 播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvPlaylist_MouseDoubleClick(null, null);
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

        private void btnPlayOrPause_Click(object sender, EventArgs e)
        {
            _waitForm.Show(this, () =>
            {
                try
                {
                    if (DlnaManager.Instance.CurrentDevice == null)
                    {
                        LogUtils.Warn(logger, "操作失败，未选择设备");
                        return;
                    }
                    if (DlnaManager.Instance.CurrentDevice.State == EnumTransportState.PAUSED ||
                        DlnaManager.Instance.CurrentDevice.State == EnumTransportState.PAUSED_PLAYBACK)
                    {
                        if (!DlnaManager.Instance.ResumePlayback(out string errorMsg))
                        {
                            LogUtils.Error(logger, $"播放失败：{errorMsg}");
                        }
                    }
                    else if (DlnaManager.Instance.CurrentDevice.State == EnumTransportState.PLAYING)
                    {
                        if (!DlnaManager.Instance.PausePlayback(out string errorMsg))
                        {
                            LogUtils.Error(logger, $"暂停失败：{errorMsg}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.Error(logger, $"操作失败：{ex.Message}");
                }
            });
        }

        private void btnContinuePlay_Click(object sender, EventArgs e)
        {
            try
            {
                var lastPlayedFile = AppConfig.Default.LastPlayedInfo.LastPlayedFile;
                var lastPlayedTime = AppConfig.Default.LastPlayedInfo.LastPlayedTime;
                var lastPlayedDevice = AppConfig.Default.LastPlayedInfo.LastPlayedDevice;

                if (string.IsNullOrEmpty(lastPlayedFile) || string.IsNullOrEmpty(lastPlayedTime))
                {
                    return;
                }

                if (!_dlnaDevices.Exists(d => d.DeviceName == lastPlayedDevice))
                {
                    LogUtils.Error(logger, "继续播放失败，设备不存在");
                    return;
                }
                foreach (ListViewItem item in lvPlaylist.Items)
                {
                    if (Path.GetFileName(item.Tag.ToString()) == lastPlayedFile)
                    {
                        _waitForm.Show(this, () =>
                        {
                            try
                            {
                                DlnaManager.Instance.CurrentDevice = _dlnaDevices.FirstOrDefault(d => d.DeviceName == lastPlayedDevice);
                                var nextFileUrl = AppHelper.BuildMediaUrl(item.Tag.ToString(), DlnaManager.Instance.CurrentDevice.BaseUrl);
                                DlnaManager.Instance.StopPlayback(out string errorMsg);
                                if (!DlnaManager.Instance.SendVideoToDLNA(nextFileUrl, out errorMsg))
                                {
                                    LogUtils.Error(logger, errorMsg);
                                    return;
                                }
                                _seekPosition = lastPlayedTime.Split('/')[0];
                            }
                            catch (Exception ex)
                            {
                                LogUtils.Error(logger, $"继续播放失败：{ex.Message}");
                            }
                        });

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error(logger, $"继续播放失败：{ex.Message}");
            }
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

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (DlnaManager.Instance.CurrentDevice == null)
            {
                LogUtils.Error(logger, "停止播放失败，未选择设备");
                return;
            }
            _waitForm.Show(this, () =>
            {
                if (!DlnaManager.Instance.StopPlayback(out string errorMsg))
                {
                    LogUtils.Error(logger, errorMsg);
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
                Text = "扫码手机访问",
                Size = new Size(400, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            var url = $"{AppHelper.GetWebBaseUrl(DlnaManager.Instance.CurrentDevice.BaseUrl)}/html/control.html";
            var qrImage = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = AppHelper.GenerateQRCodeImage(url),
            };
            qrForm.Controls.Add(qrImage);

            var label = new Label
            {
                Text = url,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom
            };
            label.MouseDoubleClick += (sender1, e1) =>
            {
                Clipboard.SetText(label.Text);
                MessageBox.Show("复制成功！", "提示");
            };
            qrForm.Controls.Add(label);

            qrForm.ShowDialog(this);
        }
    }
}
