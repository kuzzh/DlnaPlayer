using DlnaPlayerApp.Utils;
using System.Configuration;

namespace DlnaPlayerApp.Config
{
    internal class AppConfig
    {
        public string MediaDir { get; set; }
        public int HttpPort { get; set; } = 1573;
        public int WebSocketPort { get; set; } = 1574;
        public int CallbackPort { get; set; } = 1575;
        public LastPlayedInfo LastPlayedInfo { get; set; } = new LastPlayedInfo();

        private static AppConfig _default;
        public static AppConfig Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new AppConfig();
                    _default.LoadConfig();
                }
                return _default;
            }
        }

        private void LoadConfig()
        {
            MediaDir = ConfigurationManager.AppSettings[nameof(MediaDir)];
            if (int.TryParse(ConfigurationManager.AppSettings[nameof(HttpPort)], out int port))
            {
                HttpPort = port;
            }
            if (int.TryParse(ConfigurationManager.AppSettings[nameof(WebSocketPort)], out int wsPort))
            {
                WebSocketPort = wsPort;
            }
            if (int.TryParse(ConfigurationManager.AppSettings[nameof(CallbackPort)], out int callbackPort))
            {
                CallbackPort = callbackPort;
            }

            LastPlayedInfo.LastPlayedFile = ConfigurationManager.AppSettings[nameof(LastPlayedInfo.LastPlayedFile)];
            LastPlayedInfo.LastPlayedTime = ConfigurationManager.AppSettings[nameof(LastPlayedInfo.LastPlayedTime)];
            LastPlayedInfo.LastPlayedDevice = ConfigurationManager.AppSettings[nameof(LastPlayedInfo.LastPlayedDevice)];
        }

        public void SaveConfig()
        {
            // 获取配置文件
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // 修改键值对
            ModifyValue(config, nameof(MediaDir), MediaDir);
            ModifyValue(config, nameof(HttpPort), HttpPort.ToString());
            ModifyValue(config, nameof(WebSocketPort), WebSocketPort.ToString());
            ModifyValue(config, nameof(CallbackPort), CallbackPort.ToString());
            ModifyValue(config, nameof(LastPlayedInfo.LastPlayedFile), LastPlayedInfo.LastPlayedFile);
            ModifyValue(config, nameof(LastPlayedInfo.LastPlayedTime), LastPlayedInfo.LastPlayedTime);
            ModifyValue(config, nameof(LastPlayedInfo.LastPlayedDevice), LastPlayedInfo.LastPlayedDevice);

            // 保存配置更改
            config.Save(ConfigurationSaveMode.Modified);

            // 强制重新加载配置文件
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void ModifyValue(Configuration config, string key, string value)
        {
            // 如果键存在，则更新值；否则添加新的键值对
            if (config.AppSettings.Settings[key] != null)
            {
                config.AppSettings.Settings[key].Value = value;
            }
            else
            {
                config.AppSettings.Settings.Add(key, value);
            }
        }
    }

    internal class LastPlayedInfo
    {
        public string LastPlayedFile { get; set; }
        public string LastPlayedTime { get; set; }
        public string LastPlayedDevice { get; set; }
    }
}
