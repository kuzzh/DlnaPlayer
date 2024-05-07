using System.Configuration;

namespace DlnaPlayerApp
{
    internal class AppConfig
    {
        public string MediaDir { get; set; }
        public int HttpPort { get; set; } = 1573;

        private static AppConfig instance;
        public static AppConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AppConfig();
                    instance.LoadConfig();
                }
                return instance;
            }
        }

        private void LoadConfig()
        {
            MediaDir = ConfigurationManager.AppSettings[nameof(MediaDir)];
            if (int.TryParse(ConfigurationManager.AppSettings[nameof(HttpPort)], out int port))
            {
                HttpPort = port;
            }
        }

        public void SaveConfig()
        {
            // 获取配置文件
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // 修改键值对
            ModifyValue(config, nameof(MediaDir), MediaDir);
            ModifyValue(config, nameof(HttpPort), HttpPort.ToString());

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
