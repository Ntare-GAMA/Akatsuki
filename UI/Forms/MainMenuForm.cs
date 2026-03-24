using System;
using System.Drawing;
using System.Windows.Forms;
using RacingGame.Services;
using RacingGame.Core.Constants;

namespace RacingGame.UI.Forms
{
    /// <summary>Main menu window — entry point of the WinForms UI.</summary>
    public class MainMenuForm : Form
    {
        private RaceLeaderboard _leaderboard;

        public MainMenuForm()
        {
            _leaderboard = new RaceLeaderboard(GameConstants.LeaderboardFile);
            InitUI();
        }

        private void InitUI()
        {
            Theme.Apply(this);
            Text            = "Time-Based Racing Game";
            Size            = new Size(520, 620);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;

            // ── Title banner ────────────────────────────────────────────
            var pnlTop = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 160,
                BackColor = Theme.Surface,
            };

            var lblTitle = new Label
            {
                Text      = "🏎️  RACING GAME",
                Font      = Theme.FontTitle,
                ForeColor = Theme.AccentGold,
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Fill,
            };
            pnlTop.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text      = "Time-Based Console Simulation  v2.0",
                Font      = Theme.FontBody,
                ForeColor = Theme.TextSecondary,
                AutoSize  = false,
                TextAlign = ContentAlignment.BottomCenter,
                Dock      = DockStyle.Bottom,
                Height    = 30,
            };
            pnlTop.Controls.Add(lblSub);

            // ── Divider ─────────────────────────────────────────────────
            var div = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 3,
                BackColor = Theme.Accent,
            };

            // ── Menu buttons ─────────────────────────────────────────────
            var pnlMenu = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                Padding       = new Padding(130, 40, 0, 0),
            };

            var btnNew  = Theme.MakeButton("🆕   New Race",        240, 52);
            var btnLead = Theme.MakeButton("🏆   Leaderboard",     240, 52);
            var btnHow  = Theme.MakeSecondaryButton("📋   How to Play",  240, 46);
            var btnClear= Theme.MakeSecondaryButton("🗑    Clear Records",240, 46);
            var btnExit = Theme.MakeSecondaryButton("🚪   Exit",         240, 46);

            // spacing
            void AddSpaced(Control c, int top = 12)
            {
                c.Margin = new Padding(0, top, 0, 0);
                pnlMenu.Controls.Add(c);
            }

            AddSpaced(btnNew,   0);
            AddSpaced(btnLead, 14);
            AddSpaced(btnHow,  14);
            AddSpaced(btnClear, 8);
            AddSpaced(btnExit,  8);

            // ── Footer ───────────────────────────────────────────────────
            var pnlFoot = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 34,
                BackColor = Theme.Surface,
            };
            var lblFoot = new Label
            {
                Text      = "ALU BSE  ·  C# Project 3",
                ForeColor = Theme.TextSecondary,
                Font      = Theme.FontSmall,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            pnlFoot.Controls.Add(lblFoot);

            Controls.Add(pnlMenu);
            Controls.Add(div);
            Controls.Add(pnlTop);
            Controls.Add(pnlFoot);

            // ── Events ───────────────────────────────────────────────────
            btnNew.Click  += (_, _) =>
            {
                var frm = new NewRaceForm(_leaderboard);
                frm.ShowDialog(this);
            };

            btnLead.Click += (_, _) =>
            {
                var frm = new LeaderboardForm(_leaderboard);
                frm.ShowDialog(this);
            };

            btnHow.Click  += (_, _) => ShowHowToPlay();
            btnClear.Click+= (_, _) => ClearLeaderboard();
            btnExit.Click += (_, _) => Application.Exit();
        }

        private void ShowHowToPlay()
        {
            MessageBox.Show(
                "🏎️  HOW TO PLAY\n\n" +
                "1.  Register your racer (name & age)\n" +
                "2.  Choose a car (Budget → SuperCar)\n" +
                "3.  Pick a track (Easy → Extreme)\n" +
                "4.  Press  SPACEBAR  rapidly to accelerate!\n" +
                "5.  You have limited boosts per lap — use wisely\n" +
                "6.  Weather changes mid-race and affects speed\n" +
                "7.  Random events can help or hurt you\n" +
                "8.  Finish all laps as fast as possible\n\n" +
                "Your total time is saved to the leaderboard.",
                "How to Play",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ClearLeaderboard()
        {
            var res = MessageBox.Show(
                "Are you sure you want to delete all leaderboard records?",
                "Clear Leaderboard",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (res == DialogResult.Yes)
            {
                var svc = new FileService(GameConstants.LeaderboardFile);
                svc.Clear();
                MessageBox.Show("Leaderboard cleared.", "Done",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
