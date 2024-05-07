using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using log4net;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace DlnaLib
{
    public sealed class DlnaManager : IDisposable
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DlnaManager));
        private const int PLAY_STATE_QUERY_INTERVAL = 3000;

        private Thread _findDeviceThread;
        private bool disposedValue;

        private UdpClient _udpClient;

        private Timer _discoverTimer;

        private Timer _playStateQueryTimer;

        private EnumTransportState _lastState = EnumTransportState.NO_MEDIA_PRESENT;

        public event EventHandler<DeviceFoundEventArgs> DeviceFound;
        public event EventHandler<EventArgs> DiscoverFinished;
        public event EventHandler<EventArgs> PlayNext;
        public event EventHandler<PlayMediaInfoEventArgs> PlayMediaInfo;

        public DlnaDevice CurrentDevice { get; set; }

        public DlnaManager()
        {
            _udpClient = new UdpClient
            {
                MulticastLoopback = true,
            };
            _discoverTimer = new Timer(DiscoverTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _playStateQueryTimer = new Timer(PlayStateQueryTimerCallback, null, PLAY_STATE_QUERY_INTERVAL, Timeout.Infinite);
        }

        private void PlayStateQueryTimerCallback(object state)
        {
            try
            {
                if (CurrentDevice == null)
                {
                    return;
                }

                PlayMediaInfo mediaInfo = null;
                if (CurrentDevice.SupportGetMediaInfo)
                {
                    mediaInfo = GetMediaInfo(CurrentDevice.ControlUrl);

                }
                PlayMediaInfo?.Invoke(this, new PlayMediaInfoEventArgs(CurrentDevice, mediaInfo));

                if (CurrentDevice.SupportGetTransportInfo)
                {
                    var transportInfo = GetTransportInfo(CurrentDevice.ControlUrl);
                    if (transportInfo != null)
                    {
                        if (transportInfo.CurrentTransportState == EnumTransportState.STOPPED && _lastState == EnumTransportState.PLAYING)
                        {
                            PlayNext?.Invoke(this, new EventArgs());
                        }

                        _lastState = transportInfo.CurrentTransportState;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                logger.Error($"{nameof(PlayStateQueryTimerCallback)} - {ex.Message}");
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

        public void DiscoverDLNADevices(int discoverTimeSec = 5)
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
            _findDeviceThread?.Join(3000);
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
                    var response = Encoding.ASCII.GetString(receivedBytes);

                    // 解析响应中的LOCATION头部信息
                    var lines = response.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var location = lines.FirstOrDefault(line => line.StartsWith("LOCATION:", StringComparison.InvariantCultureIgnoreCase));
                    var serviceType = lines.FirstOrDefault(line => line.StartsWith("ST:", StringComparison.InvariantCultureIgnoreCase));

                    if (serviceType != null)
                    {
                        if (!serviceType.Contains("urn:schemas-upnp-org:service:AVTransport:1"))
                        {
                            continue;
                        }
                    }

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
                    logger.Error($"{nameof(InternalDiscoverDLNADevices)} - {ex.Message}");
                }
            }
        }

        public void SendVideoToDLNA(string videoUrl)
        {
            try
            {
                // 创建HTTP请求
                var request = (HttpWebRequest)WebRequest.Create(CurrentDevice.ControlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=\"utf-8\"";
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

                // 获取响应
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            responseStream.CopyTo(memoryStream);

                            var responseBytes = memoryStream.ToArray();
                            var resonseText = Encoding.UTF8.GetString(responseBytes);

                            logger.Debug(resonseText);

                            logger.Info("媒体文件已发送至设备");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"{nameof(SendVideoToDLNA)} - {ex.Message}");
            }
        }

        public void SendVideoPlaylistToDLNA(string[] videoUrls)
        {
            try
            {
                // 创建HTTP请求
                var request = (HttpWebRequest)WebRequest.Create(CurrentDevice.ControlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=\"utf-8\"";
                request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:AVTransport:1#SetAVTransportURI\"");

                // 构建播放列表
                var playlistBuilder = new StringBuilder();
                foreach (string videoUrl in videoUrls)
                {
                    playlistBuilder.Append($"<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item><res protocolInfo=\"http-get:*:video/mp4:*\" duration=\"INFINITY\" bitrate=\"0\" resolution=\"0x0\" size=\"0\"{videoUrl}</res></item></DIDL-Lite>");
                }

                var playlistXml = playlistBuilder.ToString();

                // 发送播放列表
                var body = $@"<?xml version=""1.0""?>
                            <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                                <s:Body>
                                    <u:SetAVTransportURI xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                                        <InstanceID>0</InstanceID>
                                        <CurrentURI>{playlistXml}</CurrentURI>
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

                // 获取响应
                var response = (HttpWebResponse)request.GetResponse();
                Console.WriteLine("Video playlist sent successfully to DLNA device.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending video playlist to DLNA device: " + ex.Message);
            }
        }

        public void SendVideoPlaylistToDLNA2(string[] videoUrls)
        {
            try
            {
                // 创建HTTP请求
                var request = (HttpWebRequest)WebRequest.Create(CurrentDevice.ControlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=utf-8";
                request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:AVTransport:1#SetAVTransportURI\"");

                // 构建播放列表XML
                var playlistBuilder = new StringBuilder();
                foreach (string videoUrl in videoUrls)
                {
                    playlistBuilder.Append($"<item><res protocolInfo=\"http-get:*:video/mp4:*\" duration=\"INFINITY\" bitrate=\"0\" resolution=\"0x0\" size=\"0\">{videoUrl}</res></item>");
                }

                var playlistXml = playlistBuilder.ToString();

                // 发送播放列表
                var body = $@"<?xml version=""1.0""?>
                    <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                        <s:Body>
                            <u:SetAVTransportURI xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                                <InstanceID>0</InstanceID>
                                <CurrentURI></CurrentURI>
                                <CurrentURIMetaData>{playlistXml}</CurrentURIMetaData>
                            </u:SetAVTransportURI>
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
                Console.WriteLine("Video playlist sent successfully to DLNA device.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending video playlist to DLNA device: " + ex.Message);
            }
        }

        private TransportInfo GetTransportInfo(string controlUrl)
        {
            try
            {
                // 创建HTTP请求
                var request = (HttpWebRequest)WebRequest.Create(controlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=utf-8";
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
                    return transportInfo;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"{nameof(GetTransportInfo)} - {ex.Message}");

                if (ex is WebException webException)
                {
                    if (webException.Response is HttpWebResponse webResponse)
                    {
                        if (webResponse.StatusCode == HttpStatusCode.NotImplemented)
                        {
                            CurrentDevice.SupportGetTransportInfo = false;
                        }
                    }
                }
            }

            return null; // 默认情况下假定视频未完成播放
        }

        private PlayMediaInfo GetMediaInfo(string controlUrl)
        {
            try
            {
                // 创建HTTP请求
                var request = (HttpWebRequest)WebRequest.Create(controlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=utf-8";
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
                    var mediaInfo = DlnaLib.PlayMediaInfo.ParseXml(responseXml);
                    return mediaInfo;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"{nameof(GetMediaInfo)} - {ex.Message}");

                if (ex is WebException webException)
                {
                    if (webException.Response is HttpWebResponse webResponse)
                    {
                        if (webResponse.StatusCode == HttpStatusCode.NotImplemented)
                        {
                            CurrentDevice.SupportGetMediaInfo = false;
                        }
                    }
                }
            }

            return null; // 默认情况下假定视频未完成播放
        }

        public void StartPlayback(string controlUrl)
        {
            try
            {
                // 创建HTTP请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(controlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=\"utf-8\"";
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
                Console.WriteLine("Playback started successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting playback: " + ex.Message);
            }
        }

        public void PausePlayback(string controlUrl)
        {
            try
            {
                // 创建HTTP请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(controlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=\"utf-8\"";
                request.Headers.Add("SOAPAction", "\"urn:schemas-upnp-org:service:AVTransport:1#Pause\"");

                // 发送控制命令
                string body = @"<?xml version=""1.0""?>
                            <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                                <s:Body>
                                    <u:Pause xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1"">
                                        <InstanceID>0</InstanceID>
                                    </u:Pause>
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
                Console.WriteLine("Playback paused successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error pausing playback: " + ex.Message);
            }
        }

        public void ResumePlayback(string controlUrl)
        {
            try
            {
                // 创建HTTP请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(controlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=\"utf-8\"";
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
                Console.WriteLine("Playback resumed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error resuming playback: " + ex.Message);
            }
        }

        public void StopPlayback(string controlUrl)
        {
            try
            {
                // 创建HTTP请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(controlUrl);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=\"utf-8\"";
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
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Console.WriteLine("Playback stopped successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error stopping playback: " + ex.Message);
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
