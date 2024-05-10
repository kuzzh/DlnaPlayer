using DlnaPlayerApp.Utils;
using System.Configuration;

namespace DlnaPlayerApp.Config
{
    internal class AppConfig
    {
        private string _mediaDir;
        public string MediaDir {
            get { return _mediaDir; }
            set
            {
                AppHelper.RemoveControlFile(_mediaDir);
                _mediaDir = value;
                AppHelper.MakeSureControlFileExist(_mediaDir);
            }
        }
        public int HttpPort { get; set; } = 1573;
        public int WebSocketPort { get; set; } = 1574;
        public string LastPlayedFile { get; set; }
        public string LastPlayedDevice { get; set; }

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
            LastPlayedFile = ConfigurationManager.AppSettings[nameof(LastPlayedFile)];
            LastPlayedDevice = ConfigurationManager.AppSettings[nameof(LastPlayedDevice)];
        }

        public void SaveConfig()
        {
            // 获取配置文件
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // 修改键值对
            ModifyValue(config, nameof(MediaDir), MediaDir);
            ModifyValue(config, nameof(HttpPort), HttpPort.ToString());
            ModifyValue(config, nameof(WebSocketPort), WebSocketPort.ToString());
            ModifyValue(config, nameof(LastPlayedFile), LastPlayedFile);
            ModifyValue(config, nameof(LastPlayedDevice), LastPlayedDevice);

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
}
