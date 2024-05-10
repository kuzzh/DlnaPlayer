using DlnaLib.Model;
using System;

namespace DlnaLib.Event
{
    public class DevicePropertyChangedEventArgs : EventArgs
    {
        public DlnaDevice Device { get; set; }

        public DevicePropertyChangedEventArgs(DlnaDevice device)
        {
            Device = device;
        }
    }
}
