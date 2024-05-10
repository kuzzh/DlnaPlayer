using DlnaLib.Model;
using log4net;
using System;

namespace DlnaLib.Event
{
    public class DeviceFoundEventArgs : EventArgs
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DeviceFoundEventArgs));

        public DlnaDevice DlnaDevice { get; private set; }

        public DeviceFoundEventArgs(string deviceLocation)
        {
            DlnaDevice = new DlnaDevice(deviceLocation);
        }

        public bool IsValid()
        {
            return DlnaDevice.IsValid();
        }
    }
}
