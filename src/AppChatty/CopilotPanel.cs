using System;
using System.Windows.Forms;

namespace AppChatty
{
    /// <summary>
    /// The Copilot-style side panel that appears on the right edge of the
    /// primary display.  It embeds an M365 Copilot agent via WebView2 and
    /// updates the loaded agent whenever the active foreground app changes.
    /// </summary>
    public partial class CopilotPanel : Form
    {
        private string _currentAgentUrl = string.Empty;

        public CopilotPanel()
        {
            InitializeComponent();
            PositionOnRightEdge();
            InitWebViewAsync();
        }

        // ── WebView2 initialisation ────────────────────────────────────────

        /// <summary>
        /// Ensures the WebView2 environment is ready, then navigates to the
        /// default M365 Copilot agent URL.
        /// </summary>
        private async void InitWebViewAsync()
        {
            try
            {
                await webView.EnsureCoreWebView2Async(null);
                NavigateTo(AgentResolver.Default.Url);
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"WebView2 initialisation failed: {ex.Message}";
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Updates the active-app banner and, if the agent URL has changed,
        /// navigates to the correct M365 Copilot agent for the given process
        /// name.  Thread-safe: can be called from any thread.
        /// </summary>
        public void UpdateAgent(string processName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateAgent), processName);
                return;
            }

            var agent = AgentResolver.Resolve(processName);

            lblStatus.Text = string.IsNullOrEmpty(processName)
                ? "Assisting: Unknown Application"
                : $"Assisting: {processName}";

            if (agent.Url != _currentAgentUrl)
                NavigateTo(agent.Url);
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private void NavigateTo(string url)
        {
            _currentAgentUrl = url;
            if (webView.CoreWebView2 != null)
                webView.CoreWebView2.Navigate(url);
        }

        /// <summary>
        /// Positions and sizes the form so it occupies the full height of the
        /// primary display's working area and is flush with the right edge.
        /// </summary>
        private void PositionOnRightEdge()
        {
            var workArea = Screen.PrimaryScreen.WorkingArea;
            Width  = 420;
            Height = workArea.Height;
            Left   = workArea.Right - Width;
            Top    = workArea.Top;
        }

        // ── Form overrides ─────────────────────────────────────────────────

        /// <summary>
        /// Intercept the user-initiated close (Alt+F4 / title-bar X) so that
        /// the form hides instead of being destroyed.  The window can be shown
        /// again via the tray icon.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        // ── Button handlers ────────────────────────────────────────────────

        private void btnClose_Click(object sender, EventArgs e) => Hide();

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (webView.CoreWebView2 != null)
                webView.CoreWebView2.Reload();
        }
    }
}
