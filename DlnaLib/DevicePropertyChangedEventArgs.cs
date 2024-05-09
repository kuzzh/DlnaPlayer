using System;

namespace DlnaLib
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
