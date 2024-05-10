using DlnaPlayerApp.Config;
using DlnaPlayerApp.Properties;
using System;
using System.IO;

namespace DlnaPlayerApp.Utils
{
    internal static class PathUtils
    {
        public static string GetRootDir()
        {
            return Environment.CurrentDirectory;
        }

        public static string GetControlFilePath(string mediaDir)
        {
            if (string.IsNullOrEmpty(mediaDir))
            {
                return null;
            }
            return Path.Combine(mediaDir, "control.html");
        }
    }
}
