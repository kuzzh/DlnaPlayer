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
				logger.Warn("加载播放列表失败：媒体目录不存在");
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

					logger.Info($"播放列表加载完成，共加载 {listViewItems.Count} 个媒体文件");
				}));
			});
		}


		private void OnPlayNext(object sender, EventArgs e)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action(()=>OnPlayNext(sender, e)));
				return;
			}
			if(lvPlaylist.Items.Count <= 0)
			{
				logger.Warn("播放媒体文件失败，播放列表为空");
				return;
			}
			if (_dlnaManager.CurrentDevice == null)
			{
				logger.Warn("播放媒体文件失败，未选择播放设备");
				return;
			}
			_currentPlayIndex++;

			if (_currentPlayIndex >= lvPlaylist.Items.Count)
			{
				_currentPlayIndex = 0;
			}

			var playItem = lvPlaylist.Items[_currentPlayIndex];

			var nextFileUrl = AppHelper.BuildMediaUrl(playItem.Tag.ToString(), _dlnaManager.CurrentDevice.BaseUrl);
			_dlnaManager.SendVideoToDLNA(nextFileUrl);

			playItem.Selected = true;
			playItem.Focused = true;
			lvPlaylist.Select();

			ResetListViewItemForeColor();
		}

		private void OnPlayMediaInfo(object sender, PlayMediaInfoEventArgs e)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action(() => OnPlayMediaInfo(sender, e)));
				return;
			}
			//ResetListViewItemForeColor();

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

		private void SaveConfig()
		{
			AppConfig.Instance.MediaDir = tbMediaDir.Text.Trim();
			AppConfig.Instance.SaveConfig();
		}

		private void SetListViewItemHeight(int height)
		{
			var imgList = new ImageList
			{
				ImageSize = new Size(1, height)
			};
			lvPlaylist.SmallImageList = imgList;
		}
	}
}
