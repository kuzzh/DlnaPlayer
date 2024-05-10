using DlnaPlayerApp.WebSocket.Protocol;
using Newtonsoft.Json;

namespace DlnaPlayerApp.WebSocket
{
    public class CommonResponse
    {
        public WebCmdType CmdType { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
