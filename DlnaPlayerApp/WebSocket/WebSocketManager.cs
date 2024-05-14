using DlnaPlayerApp.Config;
using WebSocketSharp.Server;

namespace DlnaPlayerApp.WebSocket
{
    internal class WebSocketManager
    {
        static WebSocketServer wssv;

        public static void Start()
        {
            wssv = new WebSocketServer(AppConfig.Default.WebSocketPort, false);
            wssv.AddWebSocketService<WebSocketServerImpl>("/");
            wssv.Start();
        }

        public static void Stop()
        {
            wssv.Stop();
        }
    }
}
