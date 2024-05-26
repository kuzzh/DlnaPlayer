using DlnaLib;
using DlnaLib.Event;
using DlnaLib.Utils;
using DlnaPlayerApp.Utils;
using DlnaPlayerApp.WebSocket.Protocol;
using log4net;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace DlnaPlayerApp.WebSocket
{
    internal class WebSocketServerImpl : WebSocketBehavior
    {
        private static ILog logger = LogManager.GetLogger(typeof(WebSocketServerImpl));

        private static readonly PlayStateResponse _playStateResponse = new PlayStateResponse();

        public WebSocketServerImpl()
        {
            DlnaManager.Instance.PlayPositionInfo += OnPlayPositionInfo;
        }

        private void OnPlayPositionInfo(object sender, PlayPositionInfoEventArgs e)
        {
            if (e.TransportInfo == null || e.PositionInfo == null)
            {
                return;
            }
            _playStateResponse.CurrentDevice = e.CurrentDevice.DeviceName;
            _playStateResponse.TrackDuration = e.PositionInfo.TrackDuration;
            _playStateResponse.RelTime = e.PositionInfo.RelTime;
            _playStateResponse.CurrentVideo = new VideoItem
            {
                Title = Path.GetFileName(HttpUtility.UrlDecode(e.PositionInfo.TrackURI)),
                RelPath = AppHelper.GetRelativePath(AppHelper.GetWebBaseUrl(e.CurrentDevice.BaseUrl), HttpUtility.UrlDecode(e.PositionInfo.TrackURI)),
            };
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            try
            {
                var webCmd = WebCmd.FromJson(e.Data);
                switch (webCmd.CmdType)
                {
                    case WebCmdType.QueryPlayState:
                        HandleQueryPlayStateCmd();
                        break;
                    case WebCmdType.PausePlay:
                        HandlePausePlayCmd();
                        break;
                    case WebCmdType.ResumePlay:
                        HandleResumePlayCmd();
                        break;
                    case WebCmdType.PlayVideo:
                        HandlePlayVideoCmd(webCmd.VideoItem);
                        break;
                    default:
                        break;
                }
            }
            catch (System.Exception ex)
            {
                LogUtils.Error(logger, ex.Message);
            }
        }

        private void HandlePlayVideoCmd(VideoItem videoItem)
        {
            var commonResponse = new CommonResponse
            {
                CmdType = WebCmdType.PlayVideo
            };

            if (DlnaManager.Instance.CurrentDevice == null)
            {
                commonResponse.Success = false;
                commonResponse.Message = "播放视频失败，未选择设备";
            }
            else
            {
                var success = frmMain.MainForm.PlayVideo(videoItem.RelPath, out string errorMsg);
                commonResponse.Success = success;
                commonResponse.Message = errorMsg;
            }
            var json = commonResponse.ToJson();
            SendAsync(json, completed =>
            {
                if (!completed)
                {
                    LogUtils.Error(logger, "向客户端发送播放视频指令响应失败");
                }
            });
        }

        private void HandlePausePlayCmd()
        {
            var success = DlnaManager.Instance.PausePlayback(out string errorMsg);
            var commonResponse = new CommonResponse
            {
                Success = success,
                CmdType = WebCmdType.PausePlay,
                Message = errorMsg
            };
            var json = commonResponse.ToJson();
            SendAsync(json, completed =>
            {
                if (!completed)
                {
                    LogUtils.Error(logger, "向客户端发送暂停播放指令响应失败");
                }
            });
        }

        private void HandleResumePlayCmd()
        {
            var success = DlnaManager.Instance.ResumePlayback(out string errorMsg);
            var commonResponse = new CommonResponse
            {
                Success = success,
                CmdType = WebCmdType.ResumePlay,
                Message = errorMsg
            };
            var json = commonResponse.ToJson();
            SendAsync(json, completed =>
            {
                if (!completed)
                {
                    LogUtils.Error(logger, "向客户端发送继续播放指令响应失败");
                }
            });
        }

        private void HandleQueryPlayStateCmd()
        {
            _playStateResponse.Success = true;
            _playStateResponse.CmdType = WebCmdType.QueryPlayState;
            _playStateResponse.VideoItems.Clear();
            _playStateResponse.VideoItems.AddRange(frmMain.MainForm.GetVideoItems());

            //_playStateResponse.CurrentVideo = _playStateResponse.VideoItems[4];
            //_playStateResponse.CurrentDevice = "小爱音箱";
            //_playStateResponse.TrackDuration = "00:45:67";
            //_playStateResponse.RelTime = "00:35:9";

            var json = _playStateResponse.ToJson();
            SendAsync(json, completed =>
            {
                if (!completed)
                {
                    LogUtils.Error(logger, "向客户端发送播放状态查询响应失败");
                }
            });
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            base.OnError(e);

            LogUtils.Error(logger, e.Message);
        }
    }
}
