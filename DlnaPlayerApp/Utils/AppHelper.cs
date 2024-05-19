using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Windows.Forms;
using System.Web;
using System.Linq;
using System.IO;
using System;
using System.Drawing;
using QRCoder;
using DlnaPlayerApp.Config;
using DlnaPlayerApp.Properties;
using System.Text;

namespace DlnaPlayerApp.Utils
{
    internal class AppHelper
    {
        private static Dictionary<string, string> _localIPv4s = new Dictionary<string, string>();

        public static List<IPAddress> GetLocalIPv4Addresses(string baseUrl)
        {
            var localIPs = new List<IPAddress>();
            var uri = new Uri(baseUrl);
            var host = uri.Host.Substring(0, uri.Host.LastIndexOf('.') + 1);

            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 排除回环和非操作状态的接口
                if (netInterface.OperationalStatus != OperationalStatus.Up || netInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                foreach (var ip in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        // 检查 IPv4 地址是否以 host 开头
                        if (ip.Address.ToString().StartsWith(host))
                        {
                            localIPs.Add(ip.Address);
                        }
                    }
                }
            }

            return localIPs;
        }

        public static string GetWebBaseUrl(string baseUrl)
        {
            if (!_localIPv4s.TryGetValue(baseUrl, out string hostIp))
            {
                var localAddresses = GetLocalIPv4Addresses(baseUrl);
                if (localAddresses.Count <= 0)
                {
                    return string.Empty;
                }
                hostIp = localAddresses.First().ToString();

                _localIPv4s[baseUrl] = hostIp;
            }
            if (string.IsNullOrEmpty(hostIp))
            {
                return null;
            }
            return $"http://{hostIp}:{AppConfig.Default.HttpPort}";
        }

        public static string BuildCallbackUrl(string baseUrl)
        {
            if (!_localIPv4s.TryGetValue(baseUrl, out string hostIp))
            {
                var localAddresses = GetLocalIPv4Addresses(baseUrl);
                if (localAddresses.Count <= 0)
                {
                    return string.Empty;
                }
                hostIp = localAddresses.First().ToString();

                _localIPv4s[baseUrl] = hostIp;
            }
            if (string.IsNullOrEmpty(hostIp))
            {
                return string.Empty;
            }
            return $"http://{hostIp}:{AppConfig.Default.CallbackPort}{EventWebServer.RelCallbackUrl}";
        }

        public static string BuildMediaUrl(string filePath, string baseUrl)
        {
            var webBaseUrl = GetWebBaseUrl(baseUrl);

            var relFilePath = GetRelativePath(AppConfig.Default.MediaDir, filePath);

            return $"{webBaseUrl}/{HttpUtility.UrlEncode(relFilePath)}";
        }

        public static string GetRelativePath(string basePath, string targetPath)
        {
            return targetPath.Replace(basePath, "").Replace('\\', '/').TrimStart('/');
        }

        public static Image GenerateQRCodeImage(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            return Image.FromStream(new MemoryStream(qrCodeImage));
        }
    }
}
