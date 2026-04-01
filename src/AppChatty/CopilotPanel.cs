using System;
using System.Drawing;
using System.Windows.Forms;

namespace AppChatty
{
    /// <summary>
    /// The Copilot-style side panel that appears on the right edge of the
    /// primary display.  It embeds an M365 Copilot agent via WebView2 and
    /// updates the loaded agent whenever the active foreground app changes.
    /// The panel can be collapsed (slides right, leaving a narrow tab) or
    /// expanded (slides left to full width), and can be dragged vertically.
    /// </summary>
    public partial class CopilotPanel : Form
    {
        // ── Agent state ────────────────────────────────────────────────────
        private string _currentAgentUrl = string.Empty;

        // ── Collapse / expand ──────────────────────────────────────────────
        private bool   _isCollapsed       = false;
        private int    _slideTargetLeft;
        private const int ExpandedWidth    = 420;
        private const int CollapsedTabWidth = 44; // px of panel visible when collapsed

        private System.Windows.Forms.Timer _slideTimer;

        // ── Drag (repositioning) ──────────────────────────────────────────
        private bool  _isDragging        = false;
        private Point _dragStartMouse;
        private int   _dragStartFormTop;
        private int   _dragStartFormLeft;

        // ── Banner colour scheme ───────────────────────────────────────────
        private static readonly Color BannerColorDefault  = Color.FromArgb(199, 224, 244);
        private static readonly Color BannerColorMatched  = Color.FromArgb(198, 239, 206);
        private static readonly Color StatusColorDefault  = Color.FromArgb(0,  74, 117);
        private static readonly Color StatusColorMatched  = Color.FromArgb(0,  97,   0);

        // Pre-created fonts reused across UpdateAgent calls (disposed with the form)
        private Font _statusFontRegular;
        private Font _statusFontBold;

        public CopilotPanel()
        {
            InitializeComponent();
            InitSlideTimer();
            _statusFontRegular = new Font(lblStatus.Font, FontStyle.Regular);
            _statusFontBold    = new Font(lblStatus.Font, FontStyle.Bold);
            PositionOnRightEdge();
            InitWebViewAsync();
        }

        // ── Slide animation ────────────────────────────────────────────────

        private void InitSlideTimer()
        {
            _slideTimer = new System.Windows.Forms.Timer { Interval = 12 };
            _slideTimer.Tick += OnSlideTick;
        }

        private void OnSlideTick(object sender, EventArgs e)
        {
            int diff = _slideTargetLeft - Left;
            int step = Math.Max(6, Math.Abs(diff) / 4);

            if (Math.Abs(diff) <= step)
            {
                Left = _slideTargetLeft;
                _slideTimer.Stop();
            }
            else
            {
                Left += diff > 0 ? step : -step;
            }
        }

        private void CollapsePanel()
        {
            _isCollapsed       = true;
            btnMinimize.Visible = false;
            btnClose.Visible   = false;
            btnRefresh.Visible = false;
            lblTitle.Visible   = false;
            pnlBanner.Visible  = false;
            webView.Visible    = false;
            btnCollapse.Text   = "►";
            // Move collapse button into the visible collapsed tab area
            btnCollapse.Location = new Point(4, 8);

            var workArea       = Screen.PrimaryScreen.WorkingArea;
            _slideTargetLeft   = workArea.Right - CollapsedTabWidth;
            _slideTimer.Start();
        }

        private void RestorePanel()
        {
            _isCollapsed       = false;
            btnMinimize.Visible = true;
            btnClose.Visible   = true;
            btnRefresh.Visible = true;
            lblTitle.Visible   = true;
            pnlBanner.Visible  = true;
            webView.Visible    = true;
            btnCollapse.Text   = "◄";
            // Restore collapse button to its original position
            btnCollapse.Location = new Point(376, 8);

            var workArea       = Screen.PrimaryScreen.WorkingArea;
            _slideTargetLeft   = workArea.Right - ExpandedWidth;
            _slideTimer.Start();
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

            var  agent          = AgentResolver.Resolve(processName);

            if (!agent.IsDefault)
            {
                // Highlight banner in green and show the matched agent label
                pnlBanner.BackColor = BannerColorMatched;
                lblStatus.ForeColor = StatusColorMatched;
                lblStatus.Font      = _statusFontBold;
                lblStatus.Text      = $"✓ This agent supports: {agent.Label}";
            }
            else
            {
                pnlBanner.BackColor = BannerColorDefault;
                lblStatus.ForeColor = StatusColorDefault;
                lblStatus.Font      = _statusFontRegular;
                lblStatus.Text      = string.IsNullOrEmpty(processName)
                    ? "Assisting: Unknown Application"
                    : $"Assisting: {processName}";
            }

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
            Width  = ExpandedWidth;
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

        private void btnMinimize_Click(object sender, EventArgs e) => Hide();

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (webView.CoreWebView2 != null)
                webView.CoreWebView2.Reload();
        }

        private void btnCollapse_Click(object sender, EventArgs e)
        {
            if (_isCollapsed)
                RestorePanel();
            else
                CollapsePanel();
        }

        // ── Drag handlers (repositioning) ──────────────────────────────────

        private void pnlHeader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging        = true;
                _dragStartMouse    = Control.MousePosition;
                _dragStartFormTop  = Top;
                _dragStartFormLeft = Left;
            }
        }

        private void pnlHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            int deltaX = Control.MousePosition.X - _dragStartMouse.X;
            int deltaY = Control.MousePosition.Y - _dragStartMouse.Y;
            var workArea = Screen.PrimaryScreen.WorkingArea;

            // Clamp so the panel remains within the working area
            int newTop = _dragStartFormTop + deltaY;
            newTop = Math.Max(workArea.Top, Math.Min(workArea.Bottom - Height, newTop));
            Top = newTop;

            int newLeft = _dragStartFormLeft + deltaX;
            int effectiveWidth = _isCollapsed ? CollapsedTabWidth : Width;
            newLeft = Math.Max(workArea.Left + CollapsedTabWidth - effectiveWidth,
                              Math.Min(workArea.Right - CollapsedTabWidth, newLeft));
            Left = newLeft;
        }

        private void pnlHeader_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }
    }
}
