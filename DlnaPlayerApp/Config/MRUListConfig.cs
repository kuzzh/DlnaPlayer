using DlnaPlayerApp.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DlnaPlayerApp.Config
{
    public sealed class MRUListConfig
    {
        private static string MRU_CONFIG_FILE = Path.Combine(PathUtils.GetRootDir(), "MRU.json");

        public List<MediaPlayInfo> MRUPlayedList { get; set; } = new List<MediaPlayInfo>();

        private static MRUListConfig _default;
        public static MRUListConfig Default
        {
            get
            {
                if (_default == null)
                {
                    if (!File.Exists(MRU_CONFIG_FILE))
                    {
                        _default = new MRUListConfig();
                    }
                    else
                    {
                        try
                        {
                            var json = File.ReadAllText(MRU_CONFIG_FILE);
                            _default = JsonConvert.DeserializeObject<MRUListConfig>(json);
                        }
                        catch
                        {
                            _default = new MRUListConfig();
                        }
                    }


                }
                return _default;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var index = 1;
            for (int i = MRUPlayedList.Count - 1; i >= 0; i--)
            {
                sb.AppendLine($"{index++}. {MRUPlayedList[i].ToString()}");
            }
            return sb.ToString();
        }

        public void SaveConfig()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(MRU_CONFIG_FILE, json);
        }
    }

    public class MediaPlayInfo
    {
        public string MediaFile { get; set; }
        public string PlayedTime { get; set; }
        public string Device { get; set; }

        public override string ToString()
        {
            return $"{MediaFile} - {PlayedTime} - {Device}";
        }
    }
}
