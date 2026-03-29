using System.IO;
using Newtonsoft.Json;
using ArduinoHelper.Models;

namespace ArduinoHelper.Services
{
    public static class ConfigParser
    {
        public static BuildInfo? ParseBuildOptions(string folderPath)
        {
            string optionsFile = Path.Combine(folderPath, "build.options.json");
            if (!File.Exists(optionsFile)) return null;

            try
            {
                var json = File.ReadAllText(optionsFile);
                dynamic config = JsonConvert.DeserializeObject(json)!;

                string fqbn = config.fqbn;
                string sketchLocation = config.sketchLocation;

                // Look for .bin or .hex
                string firmwareFile = "";
                var files = Directory.GetFiles(folderPath);
                foreach (var file in files)
                {
                    if (file.EndsWith(".bin") || file.EndsWith(".hex"))
                    {
                        firmwareFile = file;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(firmwareFile)) return null;

                return new BuildInfo
                {
                    Path = folderPath,
                    FirmwareFile = firmwareFile,
                    Fqbn = fqbn,
                    SketchPath = sketchLocation
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
