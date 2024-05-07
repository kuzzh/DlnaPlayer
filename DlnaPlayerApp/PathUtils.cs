using System;
using System.IO;

namespace DlnaPlayerApp
{
    internal static class PathUtils
    {
        public static string GetRootDir()
        {
            return Environment.CurrentDirectory;
        }
    }
}
