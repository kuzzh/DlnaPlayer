using DlnaLib;
using DlnaLib.Model;
using DlnaPlayerApp.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DlnaPlayerApp.Config
{
    public sealed class DeviceConfig
    {
        private static string DEVICE_CONFIG_FILE = Path.Combine(PathUtils.GetRootDir(), "devices.config");

        public List<DlnaDevice> Devices { get; set; } = new List<DlnaDevice>();
        public DlnaDevice CurrentDevice { get; set; }

        private static DeviceConfig _default;
        public static DeviceConfig Default
        {
            get
            {
                if (_default == null)
                {
                    if (!File.Exists(DEVICE_CONFIG_FILE))
                    {
                        CreateDeviceConfig();
                    }
                    else
                    {
                        try
                        {
                            var json = File.ReadAllText(DEVICE_CONFIG_FILE);
                            _default = JsonConvert.DeserializeObject<DeviceConfig>(json);
                        }
                        catch
                        {
                            CreateDeviceConfig();
                        }
                    }


                }
                return _default;
            }
        }

        private static void CreateDeviceConfig()
        {
            _default = new DeviceConfig();
            _default.SaveConfig();
        }

        public void SaveConfig()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(DEVICE_CONFIG_FILE, json);
        }
    }
}
