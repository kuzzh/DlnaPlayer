using System;

namespace DlnaLib
{
    public class PlayMediaInfoEventArgs : EventArgs
    {
        public DlnaDevice CurrentDevice { get; set; }
        public PlayMediaInfo CurrentMediaInfo { get; set; }

        public PlayMediaInfoEventArgs(DlnaDevice device, PlayMediaInfo mediaInfo)
        {
            CurrentMediaInfo = mediaInfo;
            CurrentDevice = device;
        }
    }
}
