using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;

namespace AppChatty
{
    /// <summary>
    /// Polls the Win32 foreground window at a configurable interval and raises
    /// <see cref="ActiveAppChanged"/> whenever the active application changes.
    /// </summary>
    /// <remarks>
    /// The event is raised on a thread-pool thread (Timer thread).  Consumers
    /// that update UI must marshal the call back to the UI thread.
    /// </remarks>
    public sealed class ActiveWindowWatcher : IDisposable
    {
        // ── Win32 imports ──────────────────────────────────────────────────

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = false)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // ── Events ─────────────────────────────────────────────────────────

        /// <summary>
        /// Raised when the foreground application changes.
        /// The event argument is the new process name (e.g. "EXCEL").
        /// </summary>
        public event EventHandler<string> ActiveAppChanged;

        // ── State ──────────────────────────────────────────────────────────

        private readonly Timer _timer;
        private string _lastProcessName = string.Empty;
        private bool _disposed;

        // ── Constructor ────────────────────────────────────────────────────

        /// <param name="intervalMs">
        /// Polling interval in milliseconds (default: 2000 ms).
        /// </param>
        public ActiveWindowWatcher(double intervalMs = 2000)
        {
            _timer = new Timer(intervalMs) { AutoReset = true };
            _timer.Elapsed += OnTimerElapsed;
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>Starts polling.</summary>
        public void Start() => _timer.Start();

        /// <summary>Stops polling without disposing the watcher.</summary>
        public void Stop() => _timer.Stop();

        // ── Implementation ─────────────────────────────────────────────────

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            string name = GetForegroundProcessName();
            if (name == _lastProcessName) return;

            _lastProcessName = name;
            ActiveAppChanged?.Invoke(this, name);
        }

        /// <summary>
        /// Returns the process name of the current foreground window owner,
        /// or an empty string when it cannot be determined.
        /// </summary>
        private static string GetForegroundProcessName()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return string.Empty;

                GetWindowThreadProcessId(hwnd, out uint pid);
                if (pid == 0) return string.Empty;

                using (var process = Process.GetProcessById((int)pid))
                    return process.ProcessName;
            }
            catch
            {
                return string.Empty;
            }
        }

        // ── IDisposable ────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _timer.Dispose();
        }
    }
}
