using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DlnaLib.Utils;
using log4net;

namespace DlnaPlayerApp
{
    public sealed class WebServer
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(WebServer));

        private static readonly WebServer mInst = new WebServer();

        private bool _running = false; // Is it running?

        private int _timeout = 8000; // Time limit for data transfers.
        private Encoding _charEncoder = Encoding.UTF8; // To encode string
        private Socket _serverSocket; // Our server socket
        private string _contentPath; // Root path of our contents

        // Content types that are supported by our server
        // You can add more...
        // To see other types: http://www.webmaster-toolkit.com/mime-types.shtml
        private readonly Dictionary<string, string> _extensions = new Dictionary<string, string>
        { 
            //{ "extension", "content type" }
            { "htm", "text/html" },
            { "html", "text/html" },
            { "xml", "text/xml" },
            { "txt", "text/plain" },
            { "css", "text/css" },
            { "png", "image/png" },
            { "gif", "image/gif" },
            { "jpg", "image/jpg" },
            { "jpeg", "image/jpeg" },
            { "zip", "application/zip"}
        };

        public static string RelCallbackUrl = "/dlna/callback";

        private WebServer()
        {
        }

        public static WebServer Instance
        {
            get { return mInst; }
        }


        public bool Start(IPAddress ipAddress, int port, int maxNOfCon, string contentPath)
        {
            if (_running) return false; // If it is already running, exit.

            try
            {
                // A tcp/ip socket (ipv4)
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                               ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(ipAddress, port));
                _serverSocket.Listen(maxNOfCon);
                _serverSocket.ReceiveTimeout = _timeout;
                _serverSocket.SendTimeout = _timeout;
                _running = true;
                this._contentPath = contentPath;

                logger.InfoFormat("Web 服务已启动，正在监听端口：{0}", port);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                return false;
            }

            // Our thread that will listen connection requests
            // and create new threads to handle them.
            Thread requestListenerT = new Thread(() =>
            {
                while (_running)
                {
                    Socket clientSocket;
                    try
                    {
                        clientSocket = _serverSocket.Accept();
                        // Create new thread to handle the request and continue to listen the socket.
                        Thread requestHandler = new Thread(() =>
                        {
                            clientSocket.ReceiveTimeout = _timeout;
                            clientSocket.SendTimeout = _timeout;
                            try
                            {
                                handleTheRequest(clientSocket);
                            }
                            catch (Exception ex)
                            {
                                LogUtils.Error(logger, ex.Message);
                                try
                                {
                                    clientSocket.Close();
                                }
                                catch (Exception ex2)
                                {
                                    LogUtils.Error(logger, ex2.Message);
                                }
                            }
                        });
                        requestHandler.Start();
                    }
                    catch (SocketException)
                    {
                        // 忽略
                    }
                    catch (Exception ex3)
                    {
                        LogUtils.Error(logger, ex3.Message);
                    }
                }
            });
            requestListenerT.Start();

            return true;
        }

        public void Stop()
        {
            if (_running)
            {
                _running = false;
                try
                {
                    _serverSocket.Dispose();
                    _serverSocket = null;

                    LogUtils.Info(logger, "Web 服务已停止");
                }
                catch (Exception ex)
                {
                    LogUtils.Error(logger, ex.Message);
                }
            }
        }

        private void handleTheRequest(Socket clientSocket)
        {
            byte[] buffer = new byte[10240]; // 10 kb, just in case
            int receivedBCount = clientSocket.Receive(buffer); // Receive the request
            string strReceived = _charEncoder.GetString(buffer, 0, receivedBCount);

            // Parse method of the request
            string httpMethod = strReceived.Substring(0, strReceived.IndexOf(" "));

            int start = strReceived.IndexOf(httpMethod) + httpMethod.Length + 1;
            int length = strReceived.LastIndexOf("HTTP") - start - 1;
            string requestedUrl = strReceived.Substring(start, length);

            
        }

        private byte[] GetBytes(IDataReader reader)
        {
            byte[] ret = null;
            if (reader.Read())
            {
                if (!reader.IsDBNull(0))
                    ret = (byte[])reader.GetValue(0);
            }
            return ret;
        }

        private void notImplemented(Socket clientSocket)
        {

            sendResponse(clientSocket, "<html><head><meta " +
                "http-equiv=\"Content-Type\" content=\"text/html; " +
                "charset=utf-8\">" +
                "</head><body><h2>Atasoy Simple Web " +
                "Server</h2><div>501 - Method Not " +
                "Implemented</div></body></html>",
                "501 Not Implemented", "text/html");

        }

        private void notFound(Socket clientSocket)
        {

            sendResponse(clientSocket, "<html><head><meta " +
                "http-equiv=\"Content-Type\" content=\"text/html; " +
                "charset=utf-8\"></head><body><h2>Atasoy Simple Web " +
                "Server</h2><div>404 - Not " +
                "Found</div></body></html>",
                "404 Not Found", "text/html");
        }

        private void sendOkResponse(Socket clientSocket, byte[] bContent, string contentType)
        {
            sendResponse(clientSocket, bContent, "200 OK", contentType);
        }

        // For strings
        private void sendResponse(Socket clientSocket, string strContent, string responseCode,
                                  string contentType)
        {
            byte[] bContent = _charEncoder.GetBytes(strContent);
            sendResponse(clientSocket, bContent, responseCode, contentType);
        }

        // For byte arrays
        private void sendResponse(Socket clientSocket, byte[] bContent, string responseCode,
                                  string contentType)
        {
            try
            {
                byte[] bHeader = _charEncoder.GetBytes(
                                    "HTTP/1.1 " + responseCode + "\r\n"
                                  + "Server: Atasoy Simple Web Server\r\n"
                                  + "Content-Length: " + bContent.Length.ToString() + "\r\n"
                                  + "Connection: close\r\n"
                                  + "Content-Type: " + contentType + "\r\n"
                                  + "Access-Control-Allow-Origin: *" + "\r\n"
                                  + "Cache-Control: max-age=31536000\r\n\r\n");
                clientSocket.Send(bHeader);
                clientSocket.Send(bContent);
                clientSocket.Close();
            }
            catch (Exception ex)
            {
                LogUtils.Error(logger, ex.Message);
            }
        }
    }
}
