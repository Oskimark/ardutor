using System;
using System.IO;
using System.Timers;

namespace ArduinoHelper.Services
{
    public class FileWatcherService : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly System.Timers.Timer _debounceTimer;
        private string? _lastChangedPath;

        public event Action<string>? OnBuildDetected;

        public FileWatcherService(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            _watcher = new FileSystemWatcher(path)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnChanged;
            _watcher.Changed += OnChanged;

            _debounceTimer = new System.Timers.Timer(1500);
            _debounceTimer.AutoReset = false;
            _debounceTimer.Elapsed += (s, e) =>
            {
                if (!string.IsNullOrEmpty(_lastChangedPath))
                {
                    OnBuildDetected?.Invoke(_lastChangedPath);
                }
            };
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            // Debounce to allow file writes to finish
            string? dir = Path.GetDirectoryName(e.FullPath);
            if (string.IsNullOrEmpty(dir)) return;

            _lastChangedPath = dir;
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        public void Dispose()
        {
            _watcher.Dispose();
            _debounceTimer.Dispose();
        }
    }
}
