using DlnaLib.Model;
using System;

namespace DlnaLib.Event
{
    public class PlayPositionInfoEventArgs : EventArgs
    {
        public DlnaDevice CurrentDevice { get; set; }
        public PositionInfo PositionInfo { get; set; }
        public TransportInfo TransportInfo { get; set; }

        public PlayPositionInfoEventArgs(DlnaDevice device, PositionInfo positionInfo, TransportInfo transportInfo)
        {
            CurrentDevice = device;
            PositionInfo = positionInfo;
            TransportInfo = transportInfo;
        }
    }
}
