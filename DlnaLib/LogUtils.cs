using log4net;
using System;

namespace DlnaLib
{
    public static class LogUtils
    {
        public static void Debug(ILog logger, string message, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            logger?.Debug($"[{caller}] {message}");
        }
        public static void Info(ILog logger, string message, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            logger?.Info($"[{caller}] {message}");
        }
        public static void Warn(ILog logger, string message, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            logger?.Warn($"[{caller}] {message}");
        }
        public static void Error(ILog logger, string message, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            logger?.Error($"[{caller}] {message}");
        }

        public static void Error(ILog logger, Exception exception)
        {
            logger?.Error(exception);
        }
    }
}
