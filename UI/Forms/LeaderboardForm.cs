using System;
using System.Drawing;
using System.Windows.Forms;
using RacingGame.Services;

namespace RacingGame.UI.Forms
{
    /// <summary>Displays all-time saved race results in a styled grid.</summary>
    public class LeaderboardForm : Form
    {
        private readonly RaceLeaderboard _leaderboard;

        public LeaderboardForm(RaceLeaderboard leaderboard)
        {
            _leaderboard = leaderboard;
            InitUI();
        }

        private void InitUI()
        {
            Theme.Apply(this);
            Text            = "All-Time Leaderboard";
            Size            = new Size(680, 540);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            // ── Header ───────────────────────────────────────────────────
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Theme.Surface };
            var lblH   = new Label
            {
                Text      = "🏆  ALL-TIME LEADERBOARD",
                Dock      = DockStyle.Fill,
                Font      = Theme.FontTitle,
                ForeColor = Theme.AccentGold,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            pnlTop.Controls.Add(lblH);
            var div = new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Theme.Accent };

            // ── DataGridView ─────────────────────────────────────────────
            var grid = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                BackgroundColor       = Theme.Background,
                GridColor             = Theme.Border,
                BorderStyle           = BorderStyle.None,
                RowHeadersVisible     = false,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                Font                  = Theme.FontMonoSm,
                ForeColor             = Theme.TextPrimary,
                DefaultCellStyle      = { BackColor = Theme.Card, ForeColor = Theme.TextPrimary, SelectionBackColor = Theme.Accent, SelectionForeColor = Color.White },
                ColumnHeadersDefaultCellStyle = { BackColor = Theme.Surface, ForeColor = Theme.AccentGold, Font = Theme.FontSubTitle, SelectionBackColor = Theme.Surface },
                EnableHeadersVisualStyles = false,
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(22, 22, 38) },
                RowTemplate           = { Height = 36 },
            };

            grid.Columns.Add("rank",   "#");
            grid.Columns.Add("name",   "Racer");
            grid.Columns.Add("track",  "Track");
            grid.Columns.Add("time",   "Time (s)");
            grid.Columns.Add("laps",   "Laps");
            grid.Columns.Add("date",   "Date");

            grid.Columns["rank"]!.FillWeight  = 6;
            grid.Columns["name"]!.FillWeight  = 18;
            grid.Columns["track"]!.FillWeight = 24;
            grid.Columns["time"]!.FillWeight  = 14;
            grid.Columns["laps"]!.FillWeight  = 8;
            grid.Columns["date"]!.FillWeight  = 20;

            var all = _leaderboard.GetAll();
            string[] medals = { "🥇", "🥈", "🥉" };

            for (int i = 0; i < all.Count; i++)
            {
                var r = all[i];
                string rank = i < medals.Length ? medals[i] : $"#{i + 1}";
                grid.Rows.Add(rank, r.RacerName, r.TrackName,
                    r.TimeSeconds.ToString("F2"), r.Laps,
                    r.RacedOn.ToString("dd MMM yyyy"));
            }

            if (all.Count == 0)
                grid.Rows.Add("—", "No records yet", "—", "—", "—", "—");

            // ── Footer ───────────────────────────────────────────────────
            var pnlBot = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = Theme.Surface, Padding = new Padding(20, 7, 20, 7) };
            var btnClose = Theme.MakeButton("Close", 120, 38);
            btnClose.Dock  = DockStyle.Right;
            btnClose.Click += (_, _) => Close();
            pnlBot.Controls.Add(btnClose);

            Controls.Add(grid);
            Controls.Add(div);
            Controls.Add(pnlTop);
            Controls.Add(pnlBot);
        }
    }
}
