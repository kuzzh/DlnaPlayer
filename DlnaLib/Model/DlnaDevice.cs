using DlnaLib.Utils;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DlnaLib.Model
{
    public class DlnaDevice : IEquatable<DlnaDevice>
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DlnaDevice));

        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceLocation { get; set; }
        public string ControlUrl { get; set; }
        public string EventSubURL { get; set; }
        public string BaseUrl { get; set; }
        public bool SupportGetMediaInfo { get; set; } = true;
        public bool SupportGetTransportInfo { get; set; } = true;
        public bool SupportGetPositionInfo { get; set; } = true;

        // 订阅事件成功后返回的事件编号
        [JsonIgnore]
        public string SID { get; set; }
        [JsonIgnore]
        public bool IsSubscribedEvents { get; set; } = false;
        [JsonIgnore]
        public string CurrentTrackURI { get; set; }
        [JsonIgnore]
        public EnumTransportState State { get; set; }

        public DlnaDevice()
        {

        }

        public DlnaDevice(string deviceLocation)
        {
            DeviceLocation = deviceLocation;

            if (string.IsNullOrEmpty(deviceLocation))
            {
                return;
            }
            try
            {
                var uri = new Uri(deviceLocation);
                BaseUrl = uri.GetLeftPart(UriPartial.Authority);

                var httpClient = new HttpClient();
                // 访问设备描述文档获取更多信息
                var descriptionXml = httpClient.GetStringAsync(deviceLocation).Result;

                // 解析设备描述文档，获取设备名称等信息
                // 在这里你可以使用合适的XML解析方法来提取设备信息
                // 这里仅作示例，你需要根据实际情况进行解析
                DeviceName = GetDeviceNameFromDescriptionXml(descriptionXml);
                DeviceId = GetDeviceIdFromDescriptionXml(descriptionXml);
                var relControlUrl = GetControlUrlFromDescriptionXml(descriptionXml);
                if (relControlUrl != null)
                {
                    ControlUrl = $"{BaseUrl}/{relControlUrl}";
                }
                var relEventSubUrl = GetEventSubUrlFromDescriptionXml(descriptionXml);
                if (relEventSubUrl != null)
                {
                    EventSubURL = $"{BaseUrl}/{relEventSubUrl}";
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error(logger, ex);
            }
        }

        public bool IsValid()
        {
            return !(string.IsNullOrEmpty(DeviceLocation) || string.IsNullOrEmpty(DeviceId) || string.IsNullOrEmpty(DeviceLocation));
        }

        private static string GetDeviceNameFromDescriptionXml(string xml)
        {
            // 这里是一个简单的示例，你需要根据实际情况解析XML获取设备名称
            // 假设设备名称在XML的<friendlyName>标签中
            // 你需要使用合适的XML解析方法来提取设备信息
            // 这里仅作示例，你需要根据实际情况进行解析
            // 示例中使用正则表达式来提取<friendlyName>标签中的文本内容
            var match = Regex.Match(xml, @"<friendlyName>(.*?)</friendlyName>");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return "Unknown";
        }

        private static string GetDeviceIdFromDescriptionXml(string xml)
        {
            // 这里是一个简单的示例，你需要根据实际情况解析XML获取设备标识
            // 假设设备标识在XML的<UDN>标签中
            // 你需要使用合适的XML解析方法来提取设备信息
            // 这里仅作示例，你需要根据实际情况进行解析
            // 示例中使用正则表达式来提取<UDN>标签中的文本内容
            var match = Regex.Match(xml, @"<UDN>(.*?)</UDN>");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return "Unknown";
        }

        private static string GetControlUrlFromDescriptionXml(string xml)
        {
            try
            {
                // 查找包含服务类型为 AVTransport 的 service 节点
                XDocument doc = XDocument.Parse(xml);
                XNamespace ns = "urn:schemas-upnp-org:device-1-0";

                var serviceNodes = doc.Descendants(ns + "service")
                    .Where(s => s.Element(ns + "serviceType").Value == "urn:schemas-upnp-org:service:AVTransport:1");

                foreach (XElement serviceNode in serviceNodes)
                {
                    // 获取控制URL
                    var controlURLNodes = serviceNode.Descendants(ns + "controlURL");
                    if (controlURLNodes != null && controlURLNodes.Count() > 0)
                    {
                        return controlURLNodes.ElementAt(0).Value;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                LogUtils.Error(logger, ex.Message);
                return null;
            }
        }

        private static string GetEventSubUrlFromDescriptionXml(string xml)
        {
            try
            {
                // 查找包含服务类型为 AVTransport 的 service 节点
                XDocument doc = XDocument.Parse(xml);
                XNamespace ns = "urn:schemas-upnp-org:device-1-0";

                var serviceNodes = doc.Descendants(ns + "service")
                    .Where(s => s.Element(ns + "serviceType").Value == "urn:schemas-upnp-org:service:AVTransport:1");

                foreach (XElement serviceNode in serviceNodes)
                {
                    // 获取控制URL
                    var eventSubURLNodes = serviceNode.Descendants(ns + "eventSubURL");
                    if (eventSubURLNodes != null && eventSubURLNodes.Count() > 0)
                    {
                        return eventSubURLNodes.ElementAt(0).Value;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                LogUtils.Error(logger, ex.Message);
                return null;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            var dlnaDevice = (DlnaDevice)obj;
            return dlnaDevice == this;
        }

        public bool Equals(DlnaDevice other)
        {
            return !(other is null) &&
                   DeviceId == other.DeviceId &&
                   DeviceName == other.DeviceName &&
                   DeviceLocation == other.DeviceLocation;
        }

        public override int GetHashCode()
        {
            int hashCode = 1482865818;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeviceId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeviceName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DeviceLocation);
            return hashCode;
        }

        public static bool operator ==(DlnaDevice left, DlnaDevice right)
        {
            return EqualityComparer<DlnaDevice>.Default.Equals(left, right);
        }

        public static bool operator !=(DlnaDevice left, DlnaDevice right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return DeviceName;
        }
    }
}
