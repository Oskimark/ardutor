using System.IO;
using Newtonsoft.Json;

namespace ArduinoHelper.Models
{
    public class AppConfig
    {
        public string LastBuildPath { get; set; } = string.Empty;
        public string LastFirmware { get; set; } = string.Empty;
        public string Port { get; set; } = "COM3";
        public int Baud { get; set; } = 115200;
        public string GlobalHotkey { get; set; } = "Ctrl+Alt+U";

        private static readonly string ConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public static AppConfig Load()
        {
            if (File.Exists(ConfigFile))
            {
                var json = File.ReadAllText(ConfigFile);
                return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            return new AppConfig();
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(ConfigFile, json);
        }
    }

    public class BuildInfo
    {
        public string Path { get; set; } = string.Empty;
        public string FirmwareFile { get; set; } = string.Empty;
        public string Fqbn { get; set; } = string.Empty;
        public string SketchPath { get; set; } = string.Empty;
    }
}
