using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Windows.Forms;
using System.Web;
using System.Linq;
using System.IO;
using System;

namespace DlnaPlayerApp
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

        public static string BuildMediaUrl(string filePath, string baseUrl)
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
            
            var relFilePath = GetRelativePath(AppConfig.Instance.MediaDir, filePath);

            return $"http://{hostIp}:{AppConfig.Instance.HttpPort}/{HttpUtility.UrlEncode(relFilePath)}";
        }

        public static string GetRelativePath(string basePath, string targetPath)
        {
            return targetPath.Replace(basePath, "/").Replace('\\', '/');
        }
    }
}
