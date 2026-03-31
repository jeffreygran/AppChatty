using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace AppChatty
{
    partial class CopilotPanel
    {
        private IContainer components = null;

        // Controls
        private Panel     pnlHeader;
        private Label     lblTitle;
        private Button    btnRefresh;
        private Button    btnClose;
        private Button    btnCollapse;
        private Panel     pnlBanner;
        private Label     lblStatus;
        private WebView2  webView;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            if (disposing)
            {
                _statusFontRegular?.Dispose();
                _statusFontBold?.Dispose();
                _slideTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();

            // ── Instantiate controls ───────────────────────────────────────
            pnlHeader   = new Panel();
            lblTitle    = new Label();
            btnRefresh  = new Button();
            btnClose    = new Button();
            btnCollapse = new Button();
            pnlBanner   = new Panel();
            lblStatus   = new Label();
            webView     = new WebView2();

            SuspendLayout();
            pnlHeader.SuspendLayout();
            pnlBanner.SuspendLayout();

            // ── pnlHeader (brand header, docked top) ──────────────────────
            pnlHeader.BackColor = Color.FromArgb(0, 120, 212); // Fluent brand blue
            pnlHeader.Dock      = DockStyle.Top;
            pnlHeader.Height    = 52;
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, btnRefresh, btnClose, btnCollapse });
            pnlHeader.MouseDown += pnlHeader_MouseDown;
            pnlHeader.MouseMove += pnlHeader_MouseMove;
            pnlHeader.MouseUp   += pnlHeader_MouseUp;

            // ── lblTitle ───────────────────────────────────────────────────
            lblTitle.Text       = "AppChatty";
            lblTitle.ForeColor  = Color.White;
            lblTitle.Font       = new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.Location   = new Point(12, 14);
            lblTitle.AutoSize   = true;
            lblTitle.Cursor     = Cursors.SizeAll;
            lblTitle.MouseDown += pnlHeader_MouseDown;
            lblTitle.MouseMove += pnlHeader_MouseMove;
            lblTitle.MouseUp   += pnlHeader_MouseUp;

            // ── btnCollapse (◄ / ►) – rightmost button, always visible ────
            btnCollapse.Text      = "◄";
            btnCollapse.ForeColor = Color.White;
            btnCollapse.BackColor = Color.Transparent;
            btnCollapse.FlatStyle = FlatStyle.Flat;
            btnCollapse.FlatAppearance.BorderSize         = 0;
            btnCollapse.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(40, 255, 255, 255);
            btnCollapse.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(70, 255, 255, 255);
            btnCollapse.Font     = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            btnCollapse.Size     = new Size(36, 36);
            btnCollapse.Location = new Point(376, 8);
            btnCollapse.Cursor   = Cursors.Hand;
            btnCollapse.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnCollapse.Click   += btnCollapse_Click;

            // ── btnClose ───────────────────────────────────────────────────
            btnClose.Text      = "✕";
            btnClose.ForeColor = Color.White;
            btnClose.BackColor = Color.Transparent;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize        = 0;
            btnClose.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(40, 255, 255, 255);
            btnClose.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(70, 255, 255, 255);
            btnClose.Font     = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            btnClose.Size     = new Size(36, 36);
            btnClose.Location = new Point(336, 8);
            btnClose.Cursor   = Cursors.Hand;
            btnClose.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click   += btnClose_Click;

            // ── btnRefresh ─────────────────────────────────────────────────
            btnRefresh.Text      = "↺";
            btnRefresh.ForeColor = Color.White;
            btnRefresh.BackColor = Color.Transparent;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize        = 0;
            btnRefresh.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(40, 255, 255, 255);
            btnRefresh.FlatAppearance.MouseDownBackColor =
                Color.FromArgb(70, 255, 255, 255);
            btnRefresh.Font     = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
            btnRefresh.Size     = new Size(36, 36);
            btnRefresh.Location = new Point(296, 8);
            btnRefresh.Cursor   = Cursors.Hand;
            btnRefresh.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click   += btnRefresh_Click;

            // ── pnlBanner (active-app banner, docked below header) ─────────
            pnlBanner.BackColor = Color.FromArgb(199, 224, 244); // light Fluent blue
            pnlBanner.Dock      = DockStyle.Top;
            pnlBanner.Height    = 32;
            pnlBanner.Controls.Add(lblStatus);

            // ── lblStatus ──────────────────────────────────────────────────
            lblStatus.Text      = "Assisting: Detecting…";
            lblStatus.ForeColor = Color.FromArgb(0, 74, 117);
            lblStatus.Font      = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblStatus.Dock      = DockStyle.Fill;
            lblStatus.Padding   = new Padding(10, 8, 0, 0);

            // ── webView ────────────────────────────────────────────────────
            webView.Dock = DockStyle.Fill;

            // ── CopilotPanel (Form) ────────────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            FormBorderStyle     = FormBorderStyle.None;
            Name                = "CopilotPanel";
            Text                = "AppChatty";
            TopMost             = true;
            ShowInTaskbar       = false;

            // Add controls in reverse dock order so that:
            //   pnlHeader  → top of form  (last added = highest z-order = outermost Top dock)
            //   pnlBanner  → below header
            //   webView    → fills remainder (first added, lowest z-order)
            Controls.Add(webView);
            Controls.Add(pnlBanner);
            Controls.Add(pnlHeader);

            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlBanner.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
