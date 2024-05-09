using System;

namespace DlnaLib
{
    public class PlayMediaInfoEventArgs : EventArgs
    {
        public DlnaDevice CurrentDevice { get; set; }
        public PlayMediaInfo CurrentMediaInfo { get; set; }
        public TransportInfo TransportInfo { get; set; }

        public PlayMediaInfoEventArgs(DlnaDevice device, PlayMediaInfo mediaInfo, TransportInfo transportInfo)
        {
            CurrentMediaInfo = mediaInfo;
            CurrentDevice = device;
            TransportInfo = transportInfo;
        }
    }
}
