using System.Drawing;
using System.Windows.Forms;

namespace RacingGame.UI
{
    /// <summary>Centralised colour and font constants for the dark racing theme.</summary>
    public static class Theme
    {
        // ── Colours ─────────────────────────────────────────────────────
        public static readonly Color Background   = Color.FromArgb(15, 15, 25);
        public static readonly Color Surface      = Color.FromArgb(25, 25, 40);
        public static readonly Color Card         = Color.FromArgb(30, 30, 50);
        public static readonly Color Accent       = Color.FromArgb(220, 40, 40);      // racing red
        public static readonly Color AccentGold   = Color.FromArgb(255, 200, 0);      // gold
        public static readonly Color AccentGreen  = Color.FromArgb(0, 220, 100);      // player
        public static readonly Color AccentBlue   = Color.FromArgb(0, 160, 255);      // AI
        public static readonly Color TextPrimary  = Color.FromArgb(240, 240, 255);
        public static readonly Color TextSecondary= Color.FromArgb(140, 140, 170);
        public static readonly Color Border       = Color.FromArgb(50, 50, 80);
        public static readonly Color TrackBg      = Color.FromArgb(20, 20, 35);

        // ── Fonts ────────────────────────────────────────────────────────
        public static readonly Font FontTitle     = new Font("Segoe UI", 22, FontStyle.Bold);
        public static readonly Font FontSubTitle  = new Font("Segoe UI", 13, FontStyle.Bold);
        public static readonly Font FontBody      = new Font("Segoe UI", 10);
        public static readonly Font FontSmall     = new Font("Segoe UI", 8);
        public static readonly Font FontMono      = new Font("Consolas",  11, FontStyle.Bold);
        public static readonly Font FontMonoSm    = new Font("Consolas",  9);

        // ── Apply dark theme to any form ─────────────────────────────────
        public static void Apply(Form form)
        {
            form.BackColor   = Background;
            form.ForeColor   = TextPrimary;
            form.Font        = FontBody;
        }

        // ── Styled button factory ────────────────────────────────────────
        public static Button MakeButton(string text, int w = 220, int h = 46)
        {
            var btn = new Button
            {
                Text      = text,
                Size      = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = Color.White,
                Font      = FontSubTitle,
                Cursor    = Cursors.Hand,
            };
            btn.FlatAppearance.BorderSize  = 0;
            btn.FlatAppearance.MouseOverBackColor  = Color.FromArgb(255, 60, 60);
            btn.FlatAppearance.MouseDownBackColor  = Color.FromArgb(180, 20, 20);
            return btn;
        }

        public static Button MakeSecondaryButton(string text, int w = 220, int h = 40)
        {
            var btn = MakeButton(text, w, h);
            btn.BackColor = Card;
            btn.ForeColor = TextPrimary;
            btn.FlatAppearance.BorderSize  = 1;
            btn.FlatAppearance.BorderColor = Border;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 45, 70);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 35);
            return btn;
        }

        public static Label MakeLabel(string text, Font? font = null, Color? color = null)
        {
            return new Label
            {
                Text      = text,
                ForeColor = color ?? TextPrimary,
                Font      = font ?? FontBody,
                AutoSize  = true,
                BackColor = Color.Transparent,
            };
        }
    }
}
