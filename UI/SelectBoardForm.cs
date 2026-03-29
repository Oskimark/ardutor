using System;
using System.Drawing;
using System.Windows.Forms;
using ArduinoHelper.Models;

namespace ArduinoHelper.UI
{
    public class SelectBoardForm : Form
    {
        private ComboBox _boardCombo = null!;
        private ComboBox _variantCombo = null!;
        private ComboBox _portCombo = null!;
        private NumericUpDown _baudNumeric = null!;
        private Button _okButton = null!;
        private Button _cancelButton = null!;

        private class BoardOption {
            public string Name { get; set; } = "";
            public string BaseFqbn { get; set; } = "";
            public List<VariantOption> Variants { get; set; } = new();
            public override string ToString() => Name;
        }

        private class VariantOption {
            public string Name { get; set; } = "";
            public string Suffix { get; set; } = "";
            public override string ToString() => Name;
        }

        private List<BoardOption> _boards = new()
        {
            new BoardOption { Name = "ESP8266 (Generic/NodeMCU)", BaseFqbn = "esp8266:esp8266:nodemcuv2" },
            new BoardOption { Name = "ESP32 (Generic)", BaseFqbn = "esp32:esp32:esp32" },
            new BoardOption { Name = "Arduino Uno", BaseFqbn = "arduino:avr:uno" },
            new BoardOption { Name = "Arduino Nano", BaseFqbn = "arduino:avr:nano", Variants = new() {
                new VariantOption { Name = "ATmega328P", Suffix = ":cpu=atmega328p" },
                new VariantOption { Name = "ATmega328P (Old Bootloader)", Suffix = ":cpu=atmega328pold" },
                new VariantOption { Name = "ATmega168", Suffix = ":cpu=atmega168" }
            }},
            new BoardOption { Name = "AVR Generic (MiniCore etc.)", BaseFqbn = "arduino:avr:328", Variants = new() {
                new VariantOption { Name = "16MHz External", Suffix = ":clock=16MHz_external" },
                new VariantOption { Name = "8MHz Internal", Suffix = ":clock=8MHz_internal" },
                new VariantOption { Name = "1MHz Internal", Suffix = ":clock=1MHz_internal" }
            }}
        };

        public string SelectedFqbn {
            get {
                var board = _boardCombo.SelectedItem as BoardOption;
                var variant = _variantCombo.SelectedItem as VariantOption;
                if (board == null) return "";
                return board.BaseFqbn + (variant?.Suffix ?? "");
            }
        }
        public string SelectedPort => _portCombo.Text;
        public int SelectedBaud => (int)_baudNumeric.Value;

        public SelectBoardForm(string defaultPort, int defaultBaud)
        {
            InitializeComponent(defaultPort, defaultBaud);
        }

        private void InitializeComponent(string defaultPort, int defaultBaud)
        {
            this.Text = "Configurar Upload Manual";
            this.Size = new Size(320, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int leftLabel = 20;
            int leftControl = 120;
            int controlWidth = 160;

            Label lblBoard = new Label { Text = "Placa:", Left = leftLabel, Top = 20, Width = 80 };
            _boardCombo = new ComboBox { Left = leftControl, Top = 18, Width = controlWidth, DropDownStyle = ComboBoxStyle.DropDownList };
            _boardCombo.Items.AddRange(_boards.ToArray());
            _boardCombo.SelectedIndexChanged += (s, e) => UpdateVariants();

            Label lblVariant = new Label { Text = "Variante:", Left = leftLabel, Top = 60, Width = 80 };
            _variantCombo = new ComboBox { Left = leftControl, Top = 58, Width = controlWidth, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblPort = new Label { Text = "Puerto COM:", Left = leftLabel, Top = 100, Width = 80 };
            _portCombo = new ComboBox { Left = leftControl, Top = 98, Width = controlWidth };
            try { _portCombo.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames()); } catch { }
            _portCombo.Text = defaultPort;

            Label lblBaud = new Label { Text = "Baud rate:", Left = leftLabel, Top = 140, Width = 80 };
            _baudNumeric = new NumericUpDown { Left = leftControl, Top = 138, Width = controlWidth, Minimum = 300, Maximum = 2000000, Value = defaultBaud };

            _okButton = new Button { Text = "Aceptar", Left = 60, Top = 200, Width = 80, DialogResult = DialogResult.OK };
            _cancelButton = new Button { Text = "Cancelar", Left = 160, Top = 200, Width = 80, DialogResult = DialogResult.Cancel };

            this.Controls.Add(lblBoard);
            this.Controls.Add(_boardCombo);
            this.Controls.Add(lblVariant);
            this.Controls.Add(_variantCombo);
            this.Controls.Add(lblPort);
            this.Controls.Add(_portCombo);
            this.Controls.Add(lblBaud);
            this.Controls.Add(_baudNumeric);
            this.Controls.Add(_okButton);
            this.Controls.Add(_cancelButton);

            _boardCombo.SelectedIndex = 0;
            UpdateVariants();

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void UpdateVariants()
        {
            _variantCombo.Items.Clear();
            var board = _boardCombo.SelectedItem as BoardOption;
            if (board != null && board.Variants.Count > 0)
            {
                _variantCombo.Enabled = true;
                _variantCombo.Items.AddRange(board.Variants.ToArray());
                _variantCombo.SelectedIndex = 0;
            }
            else
            {
                _variantCombo.Enabled = false;
                _variantCombo.Items.Add(new VariantOption { Name = "N/A", Suffix = "" });
                _variantCombo.SelectedIndex = 0;
            }
        }
    }
}
