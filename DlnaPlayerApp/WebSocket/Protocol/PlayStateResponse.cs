using Newtonsoft.Json;
using System.Collections.Generic;

namespace DlnaPlayerApp.WebSocket.Protocol
{
    public class PlayStateResponse : CommonResponse
    {
        public VideoItem CurrentVideo { get; set; }
        public string TrackDuration { get; set; }
        public string RelTime { get; set; }
        public string CurrentDevice { get; set; }
        public List<VideoItem> VideoItems { get; set; } = new List<VideoItem>();
    }
}
