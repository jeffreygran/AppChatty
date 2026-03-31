using System;
using System.Threading;
using System.Windows.Forms;

namespace AppChatty
{
    /// <summary>
    /// Application entry point.
    /// Enforces single-instance behaviour via a named Mutex, then starts the
    /// tray-based ApplicationContext so the app lives in the notification area.
    /// </summary>
    internal static class Program
    {
        private static Mutex _mutex;

        [STAThread]
        private static void Main()
        {
            // Prevent a second instance from opening a second tray icon.
            _mutex = new Mutex(true, "AppChatty_SingleInstance_Mutex", out bool createdNew);
            if (!createdNew)
            {
                MessageBox.Show(
                    "AppChatty is already running. Look for it in the system tray.",
                    "AppChatty",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new TrayApplicationContext());
            }
            finally
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
        }
    }
}
