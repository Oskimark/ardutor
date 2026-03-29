using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ArduinoHelper.Models;
using ArduinoHelper.Services;
using System.IO;

namespace ArduinoHelper.UI
{
    public class TrayAppContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly AppConfig _config;
        private readonly FileWatcherService _watcher;
        private readonly HotkeyWindow _hotkey;
        private readonly System.Windows.Forms.Timer _blinkTimer;
        private BuildInfo? _lastBuild;

        private Icon _baseIcon = null!;
        private Icon _readyIcon = null!;
        private Icon _uploadIcon = null!;
        private bool _isBlinking;
        private bool _blinkState;

        public TrayAppContext()
        {
            _config = AppConfig.Load();
            LoadIcons();
            
            _trayIcon = new NotifyIcon()
            {
                Icon = _baseIcon,
                ContextMenuStrip = CreateContextMenu(),
                Visible = true,
                Text = "Arduino Helper"
            };

            _blinkTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _blinkTimer.Tick += (s, e) =>
            {
                _blinkState = !_blinkState;
                _trayIcon.Icon = _blinkState ? _uploadIcon : _baseIcon;
            };

            string arduinoTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\arduino\sketches");
            _watcher = new FileWatcherService(arduinoTemp);
            _watcher.OnBuildDetected += HandleBuildDetected;

            _hotkey = new HotkeyWindow(3, Keys.U);
            _hotkey.OnHotkeyTyped += () => _ = UploadLastBuildAsync();

            if (!string.IsNullOrEmpty(_config.LastBuildPath))
            {
                HandleBuildDetected(_config.LastBuildPath, showBalloon: false);
            }
        }

        private void LoadIcons()
        {
            string assets = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            _baseIcon = LoadIcon(Path.Combine(assets, "base.ico"));
            _readyIcon = LoadIcon(Path.Combine(assets, "ready.ico"));
            _uploadIcon = LoadIcon(Path.Combine(assets, "upload.ico"));
        }

        private Icon LoadIcon(string path)
        {
            if (File.Exists(path))
            {
                return new Icon(path);
            }
            return SystemIcons.Application;
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("🔄 Re-subir último build", null, (s, e) => _ = UploadLastBuildAsync());
            menu.Items.Add("📁 Seleccionar carpeta de build...", null, (s, e) => SelectBuildManually());
            menu.Items.Add("📄 Seleccionar binario (.bin, .hex)...", null, (s, e) => SelectBinaryManually());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("⚙️ Configurar puerto", null, (s, e) => ConfigurePort());
            menu.Items.Add("📂 Abrir carpeta del build", null, (s, e) => OpenBuildFolder());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("❌ Salir", null, (s, e) => Exit());
            return menu;
        }

        private void SelectBinaryManually()
        {
            using (var openFile = new OpenFileDialog())
            {
                openFile.Filter = "Arduino Binary Files|*.bin;*.hex|All Files|*.*";
                openFile.Title = "Selecciona el archivo binario compilado";

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    using (var boardForm = new SelectBoardForm(_config.Port, _config.Baud))
                    {
                        if (boardForm.ShowDialog() == DialogResult.OK)
                        {
                            var build = new BuildInfo
                            {
                                Path = Path.GetDirectoryName(openFile.FileName) ?? "",
                                FirmwareFile = openFile.FileName,
                                Fqbn = boardForm.SelectedFqbn,
                                SketchPath = openFile.FileName // Use filename as sketch name for manual
                            };

                            _lastBuild = build;
                            _trayIcon.Icon = _readyIcon;

                            _config.Port = boardForm.SelectedPort;
                            _config.Baud = boardForm.SelectedBaud;
                            _config.Save();

                            _trayIcon.ShowBalloonTip(3000, "Binario Seleccionado",
                                $"{Path.GetFileName(openFile.FileName)} listo para upload", ToolTipIcon.Info);
                        }
                    }
                }
            }
        }

        private void SelectBuildManually()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Selecciona la carpeta que contiene el build de Arduino (donde está build.options.json)";
                dialog.UseDescriptionForTitle = true;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    HandleBuildDetected(dialog.SelectedPath);
                }
            }
        }

        private void HandleBuildDetected(string path)
        {
            HandleBuildDetected(path, showBalloon: true);
        }

        private void HandleBuildDetected(string path, bool showBalloon)
        {
            var build = ConfigParser.ParseBuildOptions(path);
            if (build != null)
            {
                _lastBuild = build;
                _trayIcon.Icon = _readyIcon;
                
                _config.LastBuildPath = path;
                _config.Save();

                if (showBalloon)
                {
                    _trayIcon.ShowBalloonTip(3000, "Build Detectado", $"{Path.GetFileName(build.SketchPath)} listo para upload", ToolTipIcon.Info);
                }
            }
        }

        private async System.Threading.Tasks.Task UploadLastBuildAsync()
        {
            if (_lastBuild == null)
            {
                MessageBox.Show("No se ha detectado ningún build reciente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_isBlinking) return;

            try
            {
                _isBlinking = true;
                _blinkTimer.Start();

                var result = await System.Threading.Tasks.Task.Run(() => 
                    UploadService.Upload(_lastBuild, _config.Port, _config.Baud));

                _blinkTimer.Stop();
                _isBlinking = false;
                _trayIcon.Icon = _readyIcon;

                if (result.success)
                {
                    _trayIcon.ShowBalloonTip(3000, "Upload Completado", 
                        $"Éxito. Tiempo: {result.seconds:F2}s", ToolTipIcon.Info);
                }
                else
                {
                    _trayIcon.ShowBalloonTip(3000, "Error en Upload", 
                        "El proceso falló. Revisa la configuración del puerto.", ToolTipIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _blinkTimer.Stop();
                _isBlinking = false;
                _trayIcon.Icon = _readyIcon;
                MessageBox.Show($"Error al subir: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurePort()
        {
            // Simple string input for now, ideally a dropdown of SerialPort.GetPortNames()
            string port = Microsoft.VisualBasic.Interaction.InputBox("Ingresa el puerto COM (ej: COM3):", "Configurar Puerto", _config.Port);
            if (!string.IsNullOrEmpty(port))
            {
                _config.Port = port;
                _config.Save();
            }
        }

        private void OpenBuildFolder()
        {
            if (_lastBuild != null)
            {
                Process.Start("explorer.exe", _lastBuild.Path);
            }
        }

        private void Exit()
        {
            _trayIcon.Visible = false;
            _watcher.Dispose();
            _hotkey.Dispose();
            _blinkTimer.Dispose();
            
            _baseIcon.Dispose();
            _readyIcon.Dispose();
            _uploadIcon.Dispose();

            Application.Exit();
        }
    }
}
