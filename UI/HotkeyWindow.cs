using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ArduinoHelper.UI
{
    public class HotkeyWindow : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public event Action? OnHotkeyTyped;

        private readonly int _id;

        public HotkeyWindow(int modifiers, Keys key)
        {
            _id = GetHashCode();
            CreateHandle(new CreateParams());
            RegisterHotKey(Handle, _id, (uint)modifiers, (uint)key);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY && (int)m.WParam == _id)
            {
                OnHotkeyTyped?.Invoke();
            }
        }

        public void Dispose()
        {
            UnregisterHotKey(Handle, _id);
            DestroyHandle();
        }
    }
}
