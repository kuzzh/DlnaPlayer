using DlnaPlayerApp.Properties;
using System.Diagnostics;
using System.IO;

namespace DlnaPlayerApp
{
    internal class NginxUtils
    {
        public static string NginxConfFile = Path.Combine(PathUtils.GetRootDir(), "nginx/conf/nginx.conf");
        public static void SetNginxConf(string serveDir, int port)
        {
            var nginxConf = Resources.NginxConf.Replace("{port}", port.ToString()).Replace("{serve_dir}", serveDir);
            File.WriteAllText(NginxConfFile, nginxConf);
        }

        public static void StartServer()
        {
            var exeFilePath = Path.Combine(PathUtils.GetRootDir(), "nginx/dlnaweb.exe");
            var process = new Process();
            process.StartInfo.FileName = exeFilePath;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(exeFilePath);
            process.StartInfo.CreateNoWindow = true; // 隐藏控制台窗口
            process.StartInfo.UseShellExecute = false; // 不使用操作系统shell启动进程
            process.Start();
        }

        public static void StopServer()
        {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c taskkill /f /im dlnaweb.exe";
            process.StartInfo.CreateNoWindow = true; // 隐藏控制台窗口
            process.StartInfo.UseShellExecute = false; // 不使用操作系统shell启动进程
            process.Start();
        }
    }
}
