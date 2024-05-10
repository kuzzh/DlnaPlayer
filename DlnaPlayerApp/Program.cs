using DlnaLib;
using DlnaPlayerApp.Config;
using DlnaPlayerApp.Utils;
using DlnaPlayerApp.WebSocket;
using log4net.Config;
using System;
using System.Windows.Forms;
using WebSocketSharp.Server;

namespace DlnaPlayerApp
{
    internal static class Program
    {
        static WebSocketServer wssv;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.config"));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                NginxUtils.StopServer();

                StartWebSocketServer();

                Application.Run(new frmMain());
            }
            catch (Exception ex)
            {
                string msg = "";
                GetInnerExceptions(ex, ref msg);
                MessageBox.Show(msg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DlnaManager.Instance.Dispose();
                NginxUtils.StopServer();

                StopWebSocketServer();
            }
        }

        private static void StartWebSocketServer()
        {
            wssv = new WebSocketServer(AppConfig.Default.WebSocketPort, false);
            wssv.AddWebSocketService<WebSocketServerImpl>("/");
            wssv.Start();
        }

        private static void StopWebSocketServer()
        {
            wssv.Stop();
        }

        private static void GetInnerExceptions(Exception ex, ref string exceptionMessage)
        {
            if (ex == null) {
                return;
            }
            exceptionMessage = $"{exceptionMessage}{Environment.NewLine}{ex.Message}";
            if (ex.InnerException != null)
            {
                GetInnerExceptions(ex.InnerException, ref exceptionMessage);
            }
        }
    }
}
