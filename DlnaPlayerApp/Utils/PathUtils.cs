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
    }
}
