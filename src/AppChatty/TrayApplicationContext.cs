using System;
using System.Drawing;
using System.Windows.Forms;

namespace AppChatty
{
    /// <summary>
    /// Custom <see cref="ApplicationContext"/> that manages the system-tray
    /// icon and the Copilot side panel.  The application runs headlessly until
    /// the user activates the tray icon or panel.
    /// </summary>
    public sealed class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon          _trayIcon;
        private readonly CopilotPanel        _panel;
        private readonly ActiveWindowWatcher _watcher;

        public TrayApplicationContext()
        {
            _panel   = new CopilotPanel();
            _watcher = new ActiveWindowWatcher(intervalMs: 2000);

            _trayIcon = new NotifyIcon
            {
                Icon             = LoadTrayIcon(),
                Text             = "AppChatty – Click to toggle the Copilot panel",
                Visible          = true,
                ContextMenuStrip = BuildContextMenu(),
            };

            // Single click or double-click toggles the panel.
            _trayIcon.MouseClick  += OnTrayMouseClick;
            _trayIcon.DoubleClick += (_, __) => TogglePanel();

            // Forward active-app changes to the panel.
            _watcher.ActiveAppChanged += (_, appName) => _panel.UpdateAgent(appName);
            _watcher.Start();

            // Show the panel on first launch so the user sees it immediately.
            _panel.Show();
        }

        // ── Tray helpers ───────────────────────────────────────────────────

        private static Icon LoadTrayIcon()
        {
            try
            {
                // Try to load the bundled icon; fall back to the system default.
                string iconPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Resources", "icon.ico");

                if (System.IO.File.Exists(iconPath))
                    return new Icon(iconPath, 16, 16);
            }
            catch { /* ignored */ }

            return SystemIcons.Application;
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add(new ToolStripMenuItem("Open Panel",
                null, (_, __) => { _panel.Show(); _panel.Activate(); }));

            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add(new ToolStripMenuItem("Quit AppChatty",
                null, (_, __) => ExitApplication()));

            return menu;
        }

        private void OnTrayMouseClick(object sender, MouseEventArgs e)
        {
            // Left-click toggles the panel; right-click is handled by the
            // context menu automatically.
            if (e.Button == MouseButtons.Left)
                TogglePanel();
        }

        private void TogglePanel()
        {
            if (_panel.Visible)
            {
                _panel.Hide();
            }
            else
            {
                _panel.Show();
                _panel.Activate();
            }
        }

        // ── Shutdown ───────────────────────────────────────────────────────

        private void ExitApplication()
        {
            _watcher.Stop();
            _trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _watcher.Dispose();
                _trayIcon.Dispose();
                _panel.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
