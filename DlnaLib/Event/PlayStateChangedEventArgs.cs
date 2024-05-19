using DlnaLib.Model;
using System;

namespace DlnaLib.Event
{
    public class PlayStateChangedEventArgs : EventArgs
    {
        public EnumTransportState State { get; set; }

        public PlayStateChangedEventArgs(EnumTransportState state)
        {
            State = state;
        }
    }
}
