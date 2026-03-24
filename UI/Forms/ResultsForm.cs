using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RacingGame.Services;

namespace RacingGame.UI.Forms
{
    /// <summary>Displays the race podium results after the race ends.</summary>
    public class ResultsForm : Form
    {
        private readonly List<(string Name, double Time, bool IsHuman)> _results;
        private readonly RaceLeaderboard _leaderboard;

        public ResultsForm(
            List<(string Name, double Time, bool IsHuman)> results,
            RaceLeaderboard leaderboard)
        {
            _results     = results;
            _leaderboard = leaderboard;
            InitUI();
        }

        private void InitUI()
        {
            Theme.Apply(this);
            Text            = "Race Results";
            Size            = new Size(480, 500);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            // ── Header ───────────────────────────────────────────────────
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Theme.Surface };
            var lblH   = new Label
            {
                Text      = "🏁  RACE RESULTS",
                Dock      = DockStyle.Fill,
                Font      = Theme.FontTitle,
                ForeColor = Theme.AccentGold,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            pnlTop.Controls.Add(lblH);

            var div = new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Theme.Accent };

            // ── Results panel ─────────────────────────────────────────────
            var pnlRes = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Theme.Background,
                Padding   = new Padding(40, 20, 40, 10),
                AutoScroll= true,
            };

            string[] medals = { "🥇", "🥈", "🥉" };

            for (int i = 0; i < _results.Count; i++)
            {
                var (name, time, isHuman) = _results[i];
                string medal = i < medals.Length ? medals[i] : $"#{i + 1}";

                var row = new Panel
                {
                    Size      = new Size(380, 52),
                    Location  = new Point(0, i * 60),
                    BackColor = isHuman ? Color.FromArgb(30, 0, 120, 50) : Theme.Card,
                    Padding   = new Padding(12, 0, 12, 0),
                };

                if (isHuman)
                {
                    var border = new Panel
                    {
                        Size      = new Size(row.Width, row.Height),
                        Location  = new Point(0, 0),
                        BackColor = Color.Transparent,
                    };
                    row.Controls.Add(border);
                }

                var lblM = new Label
                {
                    Text      = medal,
                    Font      = new Font("Segoe UI", 20),
                    ForeColor = i == 0 ? Theme.AccentGold : Theme.TextPrimary,
                    AutoSize  = false,
                    Size      = new Size(50, 52),
                    Location  = new Point(8, 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                };

                var lblN = new Label
                {
                    Text      = name,
                    Font      = Theme.FontSubTitle,
                    ForeColor = isHuman ? Theme.AccentGreen : Theme.TextPrimary,
                    AutoSize  = false,
                    Size      = new Size(220, 52),
                    Location  = new Point(62, 0),
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                };

                var lblT = new Label
                {
                    Text      = $"{time:F2}s",
                    Font      = Theme.FontMono,
                    ForeColor = Theme.AccentGold,
                    AutoSize  = false,
                    Size      = new Size(100, 52),
                    Location  = new Point(280, 0),
                    TextAlign = ContentAlignment.MiddleRight,
                    BackColor = Color.Transparent,
                };

                row.Controls.AddRange(new Control[] { lblM, lblN, lblT });
                pnlRes.Controls.Add(row);
            }

            // ── Buttons ──────────────────────────────────────────────────
            var pnlBot = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Theme.Surface, Padding = new Padding(20, 10, 20, 10) };

            var btnLead = Theme.MakeButton("🏆  Leaderboard", 180, 42);
            var btnMenu = Theme.MakeSecondaryButton("🏠  Main Menu",  160, 42);
            btnLead.Dock = DockStyle.Right;
            btnMenu.Dock = DockStyle.Left;

            btnLead.Click += (_, _) =>
            {
                var frm = new LeaderboardForm(_leaderboard);
                frm.ShowDialog(this);
            };
            btnMenu.Click += (_, _) => Close();

            pnlBot.Controls.Add(btnLead);
            pnlBot.Controls.Add(btnMenu);

            Controls.Add(pnlRes);
            Controls.Add(div);
            Controls.Add(pnlTop);
            Controls.Add(pnlBot);
        }
    }
}
