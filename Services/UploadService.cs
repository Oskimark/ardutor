using System;
using System.Diagnostics;
using System.IO;
using ArduinoHelper.Models;

namespace ArduinoHelper.Services
{
    public static class UploadService
    {
        public static (bool success, double seconds) Upload(BuildInfo build, string port, int baud)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            bool success = false;

            if (build.Fqbn.Contains("esp8266") || build.Fqbn.Contains("esp32"))
            {
                success = RunEspTool(build, port, baud);
            }
            else if (build.Fqbn.Contains("arduino:avr"))
            {
                success = RunAvrDude(build, port, baud);
            }

            stopwatch.Stop();
            return (success, stopwatch.Elapsed.TotalSeconds);
        }

        private static bool RunEspTool(BuildInfo build, string port, int baud)
        {
            string args = $"--port {port} --baud {baud} write_flash 0x00000 \"{build.FirmwareFile}\"";
            return ExecuteProcess("esptool.exe", args);
        }

        private static bool RunAvrDude(BuildInfo build, string port, int baud)
        {
            string board = "atmega328p";
            string args = $"-c arduino -p {board} -P {port} -b {baud} -U flash:w:\"{build.FirmwareFile}\":i";
            return ExecuteProcess("avrdude.exe", args);
        }

        private static bool ExecuteProcess(string fileName, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using (Process? process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        return process.ExitCode == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error ejecutando proceso: {ex.Message}");
            }
            return false;
        }
    }
}
