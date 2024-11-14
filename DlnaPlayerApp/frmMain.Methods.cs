using DlnaLib;
using log4net.Repository.Hierarchy;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using DlnaPlayerApp.WebSocket.Protocol;
using DlnaPlayerApp.Utils;
using DlnaPlayerApp.Config;
using DlnaLib.Event;
using DlnaLib.Model;
using DlnaLib.Utils;
using WebSocketSharp;
using Logger = log4net.Repository.Hierarchy.Logger;

namespace DlnaPlayerApp
{
    partial class frmMain
    {
        private void InitLogAppender()
        {
            if (!(LogManager.GetLogger("root") is ILog log))
            {
                return;
            }
            if (!(log.Logger is Logger logger))
            {
                return;
            }
            if (logger.Parent == null)
            {
                return;
            }
            foreach (var appender in logger.Parent.Appenders)
            {
                if (appender is LogAppender logAppender)
                {
                    logAppender.LogTextBox = tbLog;
                }
            }
        }

        private void LoadPlaylist(string mediaDir, bool restartNginx)
        {
            if (string.IsNullOrEmpty(mediaDir) || !Directory.Exists(mediaDir))
            {
                LogUtils.Warn(logger, "加载播放列表失败：媒体目录不存在");
                return;
            }

            var listViewItems = new List<ListViewItem>();
            Task.Factory.StartNew(() =>
            {
                foreach (var file in Directory.EnumerateFiles(mediaDir, "*.*", SearchOption.AllDirectories))
                {
                    var ext = $"*{Path.GetExtension(file)}";
                    if (SupporetdVideoFormats.Contains(ext))
                    {
                        var name = Path.GetFileName(file);
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
                        NginxUtils.SetNginxConf(mediaDir, AppConfig.Default.HttpPort);
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
                    var items = listViewItems.OrderBy(item => Path.GetFileName(item.Tag.ToString())).ToArray();
                    var index = 1;
                    foreach (var item in items)
                    {
                        item.Text = $"{index++}. {item.Text}";
                    }

                    lvPlaylist.Items.AddRange(items);

                    _videoItems.Clear();
                    _videoItems.AddRange(items.Select(item => new VideoItem
                    {
                        Title = Path.GetFileName(item.Tag.ToString()),
                        RelPath = AppHelper.GetRelativePath(mediaDir, item.Tag.ToString())
                    }));

                    SaveConfig();

                    LogUtils.Info(logger, $"播放列表加载完成，共加载 {listViewItems.Count} 个媒体文件");
                }));
            });
        }

        public List<VideoItem> GetVideoItems()
        {
            return _videoItems;
        }

        public bool PlayVideo(string relPath, out string errorMsg)
        {
            List<string> relPaths;
            if (InvokeRequired)
            {
                relPaths = (List<string>)lvPlaylist.Invoke(new Func<List<string>>(() =>
                {
                    var list = new List<string>();
                    foreach (ListViewItem item in lvPlaylist.Items)
                    {
                        list.Add(AppHelper.GetRelativePath(AppConfig.Default.MediaDir, item.Tag.ToString()));
                    }
                    return list;
                }));
            }
            else
            {
                relPaths = new List<string>();
                foreach (ListViewItem item in lvPlaylist.Items)
                {
                    relPaths.Add(AppHelper.GetRelativePath(AppConfig.Default.MediaDir, item.Tag.ToString()));
                }
            }
            for (var i = 0; i < relPaths.Count; i++)
            {
                if (relPaths[i] == relPath)
                {
                    _currentPlayIndex = Math.Max(i - 1, 0);
                    break;
                }
            }
            return PlayNext(out errorMsg);
        }

        private void OnPlayNext(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnPlayNext(sender, e)));
                return;
            }
            _waitForm.Show(this, () =>
            {
                var success = PlayNext(out string errorMsg);
                if (!success)
                {
                    LogUtils.Error(logger, errorMsg);
                }
            });
        }

        private bool PlayNext(out string errorMsg)
        {
            errorMsg = "";

            _currentPlayIndex++;

            if (_currentPlayIndex >= lvPlaylist.Items.Count)
            {
                _currentPlayIndex = -1;
                errorMsg = "已播放到最后一集";
                return false;
            }

            var playItem = InvokeRequired ? (ListViewItem)lvPlaylist.Invoke(new Func<ListViewItem>(() => lvPlaylist.Items[_currentPlayIndex])) : lvPlaylist.Items[_currentPlayIndex];

            if (lvPlaylist.Items.Count <= 0)
            {
                errorMsg = "播放媒体文件失败，播放列表为空";
                return false;
            }
            if (DlnaManager.Instance.CurrentDevice == null)
            {
                errorMsg = "播放媒体文件失败，未选择播放设备";
                return false;
            }

            var nextFileUrl = AppHelper.BuildMediaUrl(playItem.Tag.ToString(), DlnaManager.Instance.CurrentDevice.BaseUrl);
            if (!DlnaManager.Instance.SendVideoToDLNA(nextFileUrl, out errorMsg))
            {
                return false;
            }
            else
            {
                if (AppConfig.NeedSkip(AppConfig.Default.SkipTime))
                {
                    _seekPosition = AppConfig.Default.SkipTime;
                    LogUtils.Info(logger, $"Seek Position={_seekPosition}");
                }
                BeginInvoke(new Action(() =>
                {
                    playItem.Selected = true;
                    playItem.Focused = true;
                    lvPlaylist.Select();
                }));

                return true;
            }
        }

        private void OnPlayPositionInfo(object sender, PlayPositionInfoEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnPlayPositionInfo(sender, e)));
                return;
            }

            if (e.TransportInfo == null || e.PositionInfo == null)
            {
                //LogUtils.Info(logger, $"设备[{e.CurrentDevice.DeviceName}]播放状态：未知");
                return;
            }

            var listViewItem = FindListViewItem(e.PositionInfo.TrackURI);
            if (listViewItem != null)
            {
                if (_prevPlayingListViewItem != listViewItem)
                {
                    if (_prevPlayingListViewItem != null)
                    {
                        _prevPlayingListViewItem.ForeColor = Color.Black;
                    }
                    listViewItem.ForeColor = Color.Red;
                    _prevPlayingListViewItem = listViewItem;
                }

                _currentPlayIndex = lvPlaylist.Items.IndexOf(listViewItem);
            }

            if (_currentPlayIndex >= 0 && _currentPlayIndex < lvPlaylist.Items.Count && lvPlaylist.Items.Count > 0)
            {
                var playingItem = lvPlaylist.Items[_currentPlayIndex];
                var fileUrl = AppHelper.BuildMediaUrl(playingItem.Tag.ToString(), DlnaManager.Instance.CurrentDevice.BaseUrl);

                if (e.CurrentDevice.CurrentTrackURI != fileUrl)
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(_seekPosition))
            {
                var ts = TimeSpan.Parse(_seekPosition);
                if (e.PositionInfo.RelTimeSpan > new TimeSpan(0, 0, 0))
                {
                    if (e.PositionInfo.RelTimeSpan < new TimeSpan(0, 40, 0) && e.PositionInfo.RelTimeSpan >= ts)
                    {
                        LogUtils.Info(logger, $"e.PositionInfo.RelTimeSpan={e.PositionInfo.RelTimeSpan} ts={ts}");
                        _seekPosition = null;
                        LogUtils.Info(logger, e.PositionInfo.RelTimeSpan.ToString());
                    }
                    else
                    {
                        if (!DlnaManager.Instance.Seek(_seekPosition, out string errorMsg))
                        {
                            LogUtils.Error(logger, errorMsg);
                        }
                        else
                        {
                            LogUtils.Info(logger, $"已跳转到播放进度：{ts}");
                        }
                    }
                }
            }

            OnPlayStateChanged(this, new PlayStateChangedEventArgs(e.TransportInfo.CurrentTransportState));

            lblCurrentMediaInfo.Text = $"{Path.GetFileName(HttpUtility.UrlDecode(e.PositionInfo.TrackURI))} {e.PositionInfo.RelTime}/{e.PositionInfo.TrackDuration} {e.CurrentDevice.DeviceName}";

            var mi = new MediaPlayInfo
            {
                Device = DlnaManager.Instance.CurrentDevice.DeviceName,
                MediaFile = Path.GetFileName(HttpUtility.UrlDecode(e.PositionInfo.TrackURI)),
                PlayedTime = $"{e.PositionInfo.RelTime}/{e.PositionInfo.TrackDuration}"
            };
            if (!mi.MediaFile.IsNullOrEmpty())
            {
                if (MRUListConfig.Default.MRUPlayedList.Count > 0)
                {
                    var lastMedia = MRUListConfig.Default.MRUPlayedList.Last();
                    if (lastMedia.MediaFile == mi.MediaFile && lastMedia.Device == mi.Device)
                    {
                        lastMedia.PlayedTime = mi.PlayedTime;
                    }
                    else
                    {
                        if (MRUListConfig.Default.MRUPlayedList.Count > 10)
                        {
                            MRUListConfig.Default.MRUPlayedList.Remove(MRUListConfig.Default.MRUPlayedList.First());
                        }
                        MRUListConfig.Default.MRUPlayedList.Add(mi);
                    }
                }
                else
                {
                    MRUListConfig.Default.MRUPlayedList.Add(mi);
                }
                MRUListConfig.Default.SaveConfig();
            }
        }

        private ListViewItem FindListViewItem(string videoUrl)
        {
            foreach (ListViewItem item in lvPlaylist.Items)
            {
                var url = AppHelper.BuildMediaUrl(item.Tag.ToString(), DlnaManager.Instance.CurrentDevice.BaseUrl);
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
                if (item.ForeColor != Color.Black)
                {
                    item.ForeColor = Color.Black;
                }
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
            LogUtils.Debug(logger, $"Found Device: {e.DlnaDevice.DeviceName} - {e.DlnaDevice.DeviceLocation}");

            AddDevice(e.DlnaDevice, true);
        }

        private void AddDevice(DlnaDevice device, bool saveToConfig)
        {
            if (!_dlnaDevices.Contains(device))
            {
                cbCurrentDevice.Items.Add(device);
                _dlnaDevices.Add(device);

                if (cbCurrentDevice.SelectedItem == null)
                {
                    cbCurrentDevice.SelectedItem = device;
                }
            }

            if (saveToConfig)
            {
                if (!DeviceConfig.Default.Devices.Contains(device))
                {
                    DeviceConfig.Default.Devices.Add(device);
                    DeviceConfig.Default.SaveConfig();
                }
            }
        }

        private void SaveConfig()
        {
            AppConfig.Default.MediaDir = tbMediaDir.Text.Trim();
            AppConfig.Default.SaveConfig();
        }

        private void SetListViewItemHeight(int height)
        {
            var imgList = new ImageList
            {
                ImageSize = new Size(1, height)
            };
            lvPlaylist.SmallImageList = imgList;
        }

        private static void GetInnerExceptions(Exception ex, ref string exceptionMessage)
        {
            if (ex == null)
            {
                return;
            }
            exceptionMessage = $"{exceptionMessage}{Environment.NewLine}{ex.Message}";
            if (ex.InnerException != null)
            {
                GetInnerExceptions(ex.InnerException, ref exceptionMessage);
            }
        }
    }
}
