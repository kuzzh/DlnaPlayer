using Newtonsoft.Json;

namespace DlnaPlayerApp.WebSocket.Protocol
{
    public class WebCmd
    {
        public WebCmdType CmdType { get; set; }
        public VideoItem VideoItem { get; set; }

        public static WebCmd FromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;

            return JsonConvert.DeserializeObject<WebCmd>(json);
        }
    }
}
