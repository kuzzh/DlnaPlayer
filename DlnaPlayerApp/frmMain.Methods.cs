﻿using DlnaLib;
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
using DlnaPlayerApp.Properties;
using DlnaPlayerApp.WebSocket.Protocol;
using DlnaPlayerApp.Utils;
using DlnaPlayerApp.Config;
using DlnaLib.Event;
using DlnaLib.Model;
using DlnaLib.Utils;
using System.Windows.Documents;

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
                        var name = Path.GetFileNameWithoutExtension(file);
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
                    _currentPlayIndex = i - 1;
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
                _currentPlayIndex = 0;
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
                DlnaManager.Instance.CurrentDevice.ExpectState = EnumTransportState.PLAYING;
                DeviceConfig.Default.SaveConfig();

                BeginInvoke(new Action(() =>
                {
                    playItem.Selected = true;
                    playItem.Focused = true;
                    lvPlaylist.Select();

                    ResetListViewItemForeColor();
                }));

                AppConfig.Default.LastPlayedDevice = DlnaManager.Instance.CurrentDevice.DeviceName;
                AppConfig.Default.LastPlayedFile = playItem.Tag.ToString();
                AppConfig.Default.SaveConfig();

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
                lblCurrentMediaInfo.Text = $"设备[{e.CurrentDevice.DeviceName}]播放状态：未知";
                return;
            }
            ResetListViewItemForeColor();
            var listViewItem = FindListViewItem(e.PositionInfo.TrackURI);
            if (listViewItem != null)
            {
                listViewItem.ForeColor = Color.Red;
            }
            lblCurrentMediaInfo.Text = $"播放设备：{e.CurrentDevice.DeviceName} 正在播放：{Path.GetFileName(HttpUtility.UrlDecode(e.PositionInfo.TrackURI))} {e.PositionInfo.RelTime}/{e.PositionInfo.TrackDuration}";
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
