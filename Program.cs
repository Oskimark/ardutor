using System;
using System.Windows.Forms;
using ArduinoHelper.UI;

namespace ArduinoHelper
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            using (var trayContext = new TrayAppContext())
            {
                Application.Run(trayContext);
            }
        }
    }
}
