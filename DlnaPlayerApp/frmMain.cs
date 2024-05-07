using DlnaLib;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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

        private void InitLogAppender()
        {
            if (LogManager.GetLogger("root") is ILog log)
            {
                if (log.Logger is Logger logger)
                {
                    if (logger.Parent != null)
                    {
                        foreach (var appender in logger.Parent.Appenders)
                        {
                            if (appender is LogAppender logAppender)
                            {
                                logAppender.LogTextBox = tbLog;
                            }
                        }
                    }
                }
            }
        }

        private void OnPlayNext(object sender, EventArgs e)
        {
            if (_dlnaManager.CurrentDevice == null)
            {
                return;
            }
            _currentPlayIndex++;

            if (_currentPlayIndex >= lvPlaylist.Items.Count)
            {
                _currentPlayIndex = 0;
            }

            if (_currentPlayIndex >= lvPlaylist.Items.Count)
            {
                return;
            }

            var playItem = lvPlaylist.Items[_currentPlayIndex];

            var nextFileUrl = AppHelper.BuildMediaUrl(playItem.Tag.ToString(), _dlnaManager.CurrentDevice.BaseUrl);
            _dlnaManager.SendVideoToDLNA(nextFileUrl);

            playItem.Selected = true;
            playItem.Focused = true;
            lvPlaylist.Select();
        }

        private void OnPlayMediaInfo(object sender, PlayMediaInfoEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnPlayMediaInfo(sender, e)));
                return;
            }
            ResetListViewItemForeColor();

            if (e.CurrentMediaInfo == null)
            {
                lblCurrentMediaInfo.Text = $"设备[{e.CurrentDevice.DeviceName}]播放状态：未知";
                return;
            }
            if (e.CurrentMediaInfo.NrTracks > 0)
            {
                var listViewItem = FindListViewItem(e.CurrentMediaInfo.CurrentURI);
                if (listViewItem != null)
                {
                    listViewItem.ForeColor = Color.Red;
                }
                lblCurrentMediaInfo.Text = $"设备[{e.CurrentDevice.DeviceName}]正在播放[{Path.GetFileName(HttpUtility.UrlDecode(e.CurrentMediaInfo.CurrentURI))}]";
            }
            else
            {
                lblCurrentMediaInfo.Text = "";
            }
        }

        private ListViewItem FindListViewItem(string videoUrl)
        {
            foreach (ListViewItem item in lvPlaylist.Items)
            {
                var url = AppHelper.BuildMediaUrl(item.Tag.ToString(), _dlnaManager.CurrentDevice.BaseUrl);
                if (url == videoUrl)
                {
                    return item;
                }
            }
            return null;
        }

        private void ResetListViewItemForeColor()
        {
            foreach (ListViewItem item in lvPlaylist.Items)
            {
                item.ForeColor = Color.Black;
            }
        }

        private void OnDiscoverFinished(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { OnDiscoverFinished(sender, e); });
                return;
            }
            btnDiscoverDevices.Enabled = true;
        }

        private void OnDeviceFound(object sender, DeviceFoundEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { OnDeviceFound(sender, e); });
                return;
            }
            logger.Debug($"{e.DlnaDevice.DeviceName} - {e.DlnaDevice.DeviceLocation}");
            if (!_dlnaDevices.Contains(e.DlnaDevice))
            {
                cbCurrentDevice.Items.Add(e.DlnaDevice);
                _dlnaDevices.Add(e.DlnaDevice);

                if (cbCurrentDevice.SelectedItem == null)
                {
                    cbCurrentDevice.SelectedItem = e.DlnaDevice;
                }
            }
        }

        private void btnDiscoverDevices_Click(object sender, EventArgs e)
        {
            _dlnaDevices.Clear();
            cbCurrentDevice.Items.Clear();
            lblCurrentMediaInfo.Text = "";
            btnDiscoverDevices.Enabled = false;

            _dlnaManager.DiscoverDLNADevices();
        }

        private void SaveConfig()
        {
            AppConfig.Instance.MediaDir = tbMediaDir.Text.Trim();
            AppConfig.Instance.SaveConfig();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            _dlnaManager.Dispose();
            NginxUtils.StopServer();
        }

        private void cbCurrentDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblCurrentMediaInfo.Text = "";
            _dlnaManager.CurrentDevice = (DlnaDevice)cbCurrentDevice.SelectedItem;
        }

        private void LoadPlaylist(string mediaDir, bool restartNginx)
        {
            if (string.IsNullOrEmpty(mediaDir) || !Directory.Exists(mediaDir))
            {
                return;
            }

            var listViewItems = new List<ListViewItem>();
            Task.Factory.StartNew(() =>
            {
                var index = 1;
                foreach (var file in Directory.EnumerateFiles(mediaDir, "*.*", SearchOption.AllDirectories))
                {
                    var ext = $"*{Path.GetExtension(file)}";
                    if (SupporetdVideoFormats.Contains(ext) || SupportedAudioFormats.Contains(ext))
                    {
                        var name = $"{index++}. {Path.GetFileNameWithoutExtension(file)}";
                        var item = new ListViewItem(name)
                        {
                            ToolTipText = file,
                            Tag = file
                        };

                        listViewItems.Add(item);
                    }
                }

                if (listViewItems.Count > 0)
                {
                    if (restartNginx)
                    {
                        NginxUtils.StopServer();
                        Thread.Sleep(200);
                        NginxUtils.SetNginxConf(mediaDir, AppConfig.Instance.HttpPort);
                        Thread.Sleep(200);
                        NginxUtils.StartServer();
                    }
                }
            }).ContinueWith(t =>
            {
                BeginInvoke(new Action(() =>
                {
                    _currentPlayIndex = -1;
                    lvPlaylist.Clear();
                    lvPlaylist.Items.AddRange(listViewItems.ToArray());
                }));
            });
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
            if (lvPlaylist.Items.Count <= 0 || _dlnaManager.CurrentDevice == null)
            {
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

        //private void btnAddToPlaylist_Click(object sender, EventArgs e)
        //{
        //    if (lvMediaFiles.CheckedItems.Count <= 0)
        //    {
        //        return;
        //    }
        //    if (_dlnaManager.CurrentDevice == null)
        //    {
        //        return;
        //    }
        //    var playlistBuilder = new StringBuilder();
        //    playlistBuilder.AppendLine("#EXTM3U");
        //    foreach (ListViewItem item in lvMediaFiles.CheckedItems)
        //    {
        //        playlistBuilder.AppendLine($"#EXTINF:0,{item.Text}");
        //        playlistBuilder.AppendLine(AppHelper.BuildMediaUrl(item.Tag.ToString(), _dlnaManager.CurrentDevice.BaseUrl));
        //    }

        //    var playlistFilePath = Path.Combine(AppConfig.Instance.MediaDir, "playlist.m3u");
        //    File.WriteAllText(playlistFilePath, playlistBuilder.ToString());

        //    _dlnaManager.SendVideoToDLNA(AppHelper.BuildPlaylistUrl(playlistFilePath, _dlnaManager.CurrentDevice.BaseUrl));
        //}

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

        private void SetListViewItemHeight(int height)
        {
            var imgList = new ImageList
            {
                ImageSize = new Size(1, height)
            };
            lvPlaylist.SmallImageList = imgList;
        }

        private void btnRefreshPlaylist_Click(object sender, EventArgs e)
        {
            LoadPlaylist(AppConfig.Instance.MediaDir, false);
        }
    }
}
