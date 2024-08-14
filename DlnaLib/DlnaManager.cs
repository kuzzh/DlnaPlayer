using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using log4net;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using DlnaLib.Event;
using DlnaLib.Model;
using DlnaLib.Utils;

namespace DlnaLib
{
    public sealed class DlnaManager : IDisposable
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DlnaManager));
        private const int PLAY_STATE_QUERY_INTERVAL = 1000;

        private Thread _findDeviceThread;
        private bool disposedValue;

        private UdpClient _udpClient;

        private Timer _discoverTimer;

        private Timer _playStateQueryTimer;

        public event EventHandler<DeviceFoundEventArgs> DeviceFound;
        public event EventHandler<EventArgs> DiscoverFinished;
        public event EventHandler<PlayMediaInfoEventArgs> PlayMediaInfo;
        public event EventHandler<PlayPositionInfoEventArgs> PlayPositionInfo;
        public event EventHandler<DevicePropertyChangedEventArgs> DevicePropertyChanged;

        public DlnaDevice CurrentDevice { get; set; }

        private DlnaManager()
        {
            _udpClient = new UdpClient
            {
                MulticastLoopback = true,
            };
            _discoverTimer = new Timer(DiscoverTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _playStateQueryTimer = new Timer(PlayStateQueryTimerCallback, null, PLAY_STATE_QUERY_INTERVAL, Timeout.Infinite);
        }

        private static DlnaManager _instance;
        public static DlnaManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DlnaManager();
                }
                return _instance;
            }
        }

        private void PlayStateQueryTimerCallback(object state)
        {
            try
            {
                if (CurrentDevice == null)
                {
                    return;
                }

                PositionInfo positionInfo = null;
                TransportInfo transportInfo = null;
                if (CurrentDevice.SupportGetPositionInfo)
                {
                    positionInfo = GetPositionInfo();
                    if (positionInfo != null)
                    {
                        CurrentDevice.CurrentTrackURI = positionInfo.TrackURI;
                    }
                }
                if (CurrentDevice.SupportGetTransportInfo)
                {
                    transportInfo = GetTransportInfo();
                }

                PlayPositionInfo?.Invoke(this, new PlayPositionInfoEventArgs(CurrentDevice, positionInfo, transportInfo));

            }
            catch (Exception ex)
            {
                LogUtils.Error(logger, ex.Message);
            }
            finally
            {
                _playStateQueryTimer.Change(PLAY_STATE_QUERY_INTERVAL, Timeout.Infinite);
            }
        }

        private void DiscoverTimerCallback(object state)
        {
            StopDiscoverDLNADevices();

            DiscoverFinished?.Invoke(this, null);
        }

        public void DiscoverDLNADevices(int discoverTimeSec = 3)
        {
            Task.Factory.StartNew(() =>
            {
                StopDiscoverDLNADevices();

                _findDeviceThread = new Thread(InternalDiscoverDLNADevices);
                _findDeviceThread.Start();

                _discoverTimer.Change(discoverTimeSec * 1000, Timeout.Infinite);
            });
        }

        public void StopDiscoverDLNADevices()
        {
            _findDeviceThread?.Abort();
            _findDeviceThread = null;
        }

        private void InternalDiscoverDLNADevices()
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);

            var message = "M-SEARCH * HTTP/1.1\r\n" +
                             "HOST: 239.255.255.250:1900\r\n" +
                             "MAN: \"ssdp:discover\"\r\n" +
                             "MX: 2\r\n" +
                             "ST: ssdp:all\r\n\r\n";

            var bytes = Encoding.ASCII.GetBytes(message);

            _udpClient.Send(bytes, bytes.Length, endPoint);

            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    var receivedBytes = _udpClient.Receive(ref remoteEndPoint);
                    var response = Encoding.Default.GetString(receivedBytes);

                    // 解析响应中的LOCATION头部信息
                    var lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var location = lines.FirstOrDefault(line => line.StartsWith("LOCATION:", StringComparison.InvariantCultureIgnoreCase));
                    //var serviceType = lines.FirstOrDefault(line => line.StartsWith("ST:", StringComparison.InvariantCultureIgnoreCase));

                    //if (serviceType != null)
                    //{
                    //    if (!serviceType.Contains("urn:schemas-upnp-org:service:AVTransport:1"))
                    //    {
                    //        continue;
                    //    }
                    //}

                    if (location != null)
                    {
                        var parts = location.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        var deviceLocation = parts[1];

                        var eventArgs = new DeviceFoundEventArgs(deviceLocation);
                        if (eventArgs.IsValid())
                        {
                            DeviceFound?.Invoke(this, eventArgs);
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    // Ignore
                }
                catch (Exception ex)
                {
                    LogUtils.Error(logger, ex.Message);
                }
            }
        }

        public static async Task SubscribeAVTransportEvents(DlnaDevice dlnaDevice, string callbackUrl)
        {
            await Task.Factory.StartNew(() =>
            {
                if (dlnaDevice == null || dlnaDevice.EventSubURL == null)
                {
                    return;
                }

                if (dlnaDevice.IsSubscribedEvents)
                {
                    return;
                }

                var request = WebRequest.Create(new Uri(dlnaDevice.EventSubURL)) as HttpWebRequest;
                request.Method = "SUBSCRIBE";
                request.UserAgent = "DlnaPlayer/1.0";
                request.Headers["CALLBACK"] = $"<{callbackUrl}>";
                request.Headers["NT"] = "upnp:event";
                request.Headers["TIMEOUT"] = "Second-360000";

                try
                {
                    var requestStream = request.GetRequestStream();
                    var response = request.GetResponse() as HttpWebResponse;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        dlnaDevice.IsSubscribedEvents = true;
                        dlnaDevice.SID = response.Headers["SID"].ToString();
                        LogUtils.Info(logger, "Subscribe dlna events success");
                    }
                    else
                    {
                        LogUtils.Error(logger, "Subscribe dlna events failed, Status Code: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.Error(logger, ex.Message);
                }
            });
        }

        public static async Task UnsubscribeAVTransportEventsAsync(DlnaDevice dlnaDevice)
        {
            await Task.Factory.StartNew(() =>
            {
                if (dlnaDevice == null)
                {
                    return;
                }

                if (!dlnaDevice.IsSubscribedEvents)
                {
                    return;
                }

                var request = WebRequest.Create(new Uri(dlnaDevice.EventSubURL)) as HttpWebRequest;
                request.Method = "UNSUBSCRIBE";
                request.UserAgent = "DlnaPlayer/1.0";
                request.Headers["SID"] = dlnaDevice.SID;

                try
                {
                    var requestStream = request.GetRequestStream();
                    var response = request.GetResponse() as HttpWebResponse;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        dlnaDevice.IsSubscribedEvents = false;
                        dlnaDevice.SID = string.Empty;
                        LogUtils.Info(logger, "Unsubscribe dlna events success");
                    }
                    else
                    {
                        LogUtils.Error(logger, "Unsubscribe dlna events failed, Status Code: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.Error(logger, ex.Message);
                }
            });
        }

        public bool SendVideoToDLNA(string videoUrl, out string errorMsg)
        {
            try
            {
                errorMsg = "";

                if (CurrentDevice == null)
                {
                    errorMsg = "未选择播放设备";
                    return false;
                }

                // 创建HTTP请求
                var request = (HttpWebRequest)WebRequest.Create(CurrentDevice.ControlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:AVTransport:1#SetAVTransportURI\"");

                // 发送视频URL
                var body = $@"<?xml version=""1.0""?>
                            <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                                <s:Body>
                                    <u:SetAVTransportURI xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                                        <InstanceID>0</InstanceID>
                                        <CurrentURI>{videoUrl}</CurrentURI>
                                        <CurrentURIMetaData></CurrentURIMetaData>
                                    </u:SetAVTransportURI>
                                </s:Body>
                            </s:Envelope>";

                var byteBody = Encoding.UTF8.GetBytes(body);
                request.ContentLength = byteBody.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(byteBody, 0, byteBody.Length);
                }

                var response = request.GetResponse() as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    CurrentDevice.CurrentTrackURI = videoUrl;

                    return true;
                }

                // 获取响应
                var responseText = GetResponseText(response);
                LogUtils.Debug(logger, responseText);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            return false;
        }

        private TransportInfo GetTransportInfo()
        {
            try
            {
                if (CurrentDevice == null || CurrentDevice.ControlUrl == null)
                {
                    return null;
                }
                // 创建HTTP请求
                var request = (HttpWebRequest)WebRequest.Create(CurrentDevice.ControlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:AVTransport:1#GetTransportInfo\"");

                // 发送获取播放状态信息的请求
                var body = @"<?xml version=""1.0""?>
                    <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                        <s:Body>
                            <u:GetTransportInfo xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                                <InstanceID>0</InstanceID>
                            </u:GetTransportInfo>
                        </s:Body>
                    </s:Envelope>";

                var byteBody = Encoding.UTF8.GetBytes(body);
                request.ContentLength = byteBody.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(byteBody, 0, byteBody.Length);
                }

                // 获取响应
                var response = (HttpWebResponse)request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var responseXml = reader.ReadToEnd();
                    var transportInfo = TransportInfo.ParseXml(responseXml);
                    if (transportInfo != null)
                    {
                        CurrentDevice.State = transportInfo.CurrentTransportState;
                    }
                    return transportInfo;
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error(logger, ex.Message);

                if (ex is WebException webException)
                {
                    if (webException.Response is HttpWebResponse webResponse)
                    {
                        if (webResponse.StatusCode == HttpStatusCode.NotImplemented)
                        {
                            CurrentDevice.SupportGetTransportInfo = false;
                            DevicePropertyChanged?.Invoke(this, new DevicePropertyChangedEventArgs(CurrentDevice));
                        }
                    }
                }
            }

            return null; // 默认情况下假定视频未完成播放
        }

        private PlayMediaInfo GetMediaInfo()
        {
            try
            {
                if (CurrentDevice == null)
                {
                    return null;
                }
                // 创建HTTP请求
                var request = (HttpWebRequest)WebRequest.Create(CurrentDevice.ControlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:AVTransport:1#GetMediaInfo\"");

                // 发送获取播放状态信息的请求
                var body = @"<?xml version=""1.0""?>
                    <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                        <s:Body>
                            <u:GetMediaInfo xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                                <InstanceID>0</InstanceID>
                            </u:GetMediaInfo>
                        </s:Body>
                    </s:Envelope>";

                var byteBody = Encoding.UTF8.GetBytes(body);
                request.ContentLength = byteBody.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(byteBody, 0, byteBody.Length);
                }

                // 获取响应
                var response = (HttpWebResponse)request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var responseXml = reader.ReadToEnd();
                    var mediaInfo = Model.PlayMediaInfo.ParseXml(responseXml);
                    return mediaInfo;
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error(logger, ex.Message);

                if (ex is WebException webException)
                {
                    if (webException.Response is HttpWebResponse webResponse)
                    {
                        if (webResponse.StatusCode == HttpStatusCode.NotImplemented)
                        {
                            CurrentDevice.SupportGetMediaInfo = false;
                            DevicePropertyChanged?.Invoke(this, new DevicePropertyChangedEventArgs(CurrentDevice));
                        }
                    }
                }
            }

            return null;
        }

        private PositionInfo GetPositionInfo()
        {
            try
            {
                if (CurrentDevice == null || CurrentDevice.ControlUrl == null)
                {
                    return null;
                }
                // 创建HTTP请求
                var request = (HttpWebRequest)WebRequest.Create(CurrentDevice.ControlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:AVTransport:1#GetPositionInfo\"");

                // 发送获取播放状态信息的请求
                var body = @"<?xml version=""1.0""?>
                    <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                        <s:Body>
                            <u:GetPositionInfo xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                                <InstanceID>0</InstanceID>
                            </u:GetPositionInfo>
                        </s:Body>
                    </s:Envelope>";

                var byteBody = Encoding.UTF8.GetBytes(body);
                request.ContentLength = byteBody.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(byteBody, 0, byteBody.Length);
                }

                // 获取响应
                var response = (HttpWebResponse)request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var responseXml = reader.ReadToEnd();
                    return PositionInfo.ParseXml(responseXml);
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error(logger, ex.Message);

                if (ex is WebException webException)
                {
                    if (webException.Response is HttpWebResponse webResponse)
                    {
                        if (webResponse.StatusCode == HttpStatusCode.NotImplemented)
                        {
                            CurrentDevice.SupportGetPositionInfo = false;
                            DevicePropertyChanged?.Invoke(this, new DevicePropertyChangedEventArgs(CurrentDevice));
                        }
                    }
                }
            }
            return null;
        }

        public bool StartPlayback(out string errorMsg)
        {
            try
            {
                errorMsg = "";

                if (CurrentDevice != null)
                {
                    errorMsg = "开始播放失败：未选择设备";
                    return false;
                }
                // 创建HTTP请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(CurrentDevice.ControlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:AVTransport:1#Play\"");

                // 发送控制命令
                string body = @"<?xml version=""1.0""?>
                            <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                                <s:Body>
                                    <u:Play xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                                        <InstanceID>0</InstanceID>
                                        <Speed>1</Speed>
                                    </u:Play>
                                </s:Body>
                            </s:Envelope>";

                byte[] byteBody = Encoding.UTF8.GetBytes(body);
                request.ContentLength = byteBody.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(byteBody, 0, byteBody.Length);
                }

                // 获取响应
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                errorMsg = GetResponseText(response);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            return false;
        }

        public bool PausePlayback(out string errorMsg)
        {
            try
            {
                errorMsg = "";
                if (CurrentDevice == null)
                {
                    errorMsg = "暂停播放失败：未选择设备";
                    return false;
                }
                // 创建HTTP请求
                var request = (HttpWebRequest)WebRequest.Create(CurrentDevice.ControlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:AVTransport:1#Pause\"");

                // 发送控制命令
                var body = @"<?xml version=""1.0""?>
                            <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                                <s:Body>
                                    <u:Pause xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                                        <InstanceID>0</InstanceID>
                                    </u:Pause>
                                </s:Body>
                            </s:Envelope>";

                var byteBody = Encoding.UTF8.GetBytes(body);
                request.ContentLength = byteBody.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(byteBody, 0, byteBody.Length);
                }

                // 获取响应
                var response = (HttpWebResponse)request.GetResponse();

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                // 获取响应
                errorMsg = GetResponseText(response);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            return false;
        }

        public bool ResumePlayback(out string errorMsg)
        {
            try
            {
                errorMsg = "";
                if (CurrentDevice == null)
                {
                    errorMsg = "继续播放失败：未选择设备";
                    return false;
                }
                // 创建HTTP请求
                var request = (HttpWebRequest)WebRequest.Create(CurrentDevice.ControlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:AVTransport:1#Play\"");

                // 发送控制命令
                var body = @"<?xml version=""1.0""?>
                            <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                                <s:Body>
                                    <u:Play xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                                        <InstanceID>0</InstanceID>
                                        <Speed>1</Speed>
                                    </u:Play>
                                </s:Body>
                            </s:Envelope>";

                var byteBody = Encoding.UTF8.GetBytes(body);
                request.ContentLength = byteBody.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(byteBody, 0, byteBody.Length);
                }

                // 获取响应
                var response = (HttpWebResponse)request.GetResponse();

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                // 获取响应
                errorMsg = GetResponseText(response);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            return false;
        }

        public bool StopPlayback(out string errorMsg)
        {
            try
            {
                errorMsg = "";

                if (CurrentDevice == null)
                {
                    errorMsg = "停止播放失败：未选择设备";
                    return false;
                }
                // 创建HTTP请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(CurrentDevice.ControlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:AVTransport:1#Stop\"");

                // 发送控制命令
                string body = @"<?xml version=""1.0""?>
                            <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                                <s:Body>
                                    <u:Stop xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                                        <InstanceID>0</InstanceID>
                                    </u:Stop>
                                </s:Body>
                            </s:Envelope>";

                byte[] byteBody = Encoding.UTF8.GetBytes(body);
                request.ContentLength = byteBody.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(byteBody, 0, byteBody.Length);
                }

                // 获取响应
                var response = (HttpWebResponse)request.GetResponse();
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }

                errorMsg = GetResponseText(response);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            return false;
        }

        public bool Seek(string position, out string errorMsg)
        {
            try
            {
                errorMsg = "";

                if (CurrentDevice == null)
                {
                    errorMsg = "跳转进度失败：未选择设备";
                    return false;
                }
                // 创建HTTP请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(CurrentDevice.ControlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:AVTransport:1#Seek\"");

                // 发送控制命令
                string body = $@"<?xml version=""1.0""?>
                            <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                                <s:Body>
                                    <u:Seek xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                                        <InstanceID>0</InstanceID>
                                        <Unit>REL_TIME</Unit>
                                        <Target>{position}</Target>
                                    </u:Seek>
                                </s:Body>
                            </s:Envelope>";

                byte[] byteBody = Encoding.UTF8.GetBytes(body);
                request.ContentLength = byteBody.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(byteBody, 0, byteBody.Length);
                }

                // 获取响应
                var response = (HttpWebResponse)request.GetResponse();
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }

                errorMsg = GetResponseText(response);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            return false;
        }

        private static string GetResponseText(HttpWebResponse response)
        {
            if (response == null)
            {
                return "HttpWebResponse is null";
            }
            using (var responseStream = response.GetResponseStream())
            {
                using (var memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);

                    var responseBytes = memoryStream.ToArray();
                    var responseText = Encoding.UTF8.GetString(responseBytes);

                    return responseText;
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _udpClient?.Dispose();

                    StopDiscoverDLNADevices();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
