using System;
using System.Drawing;
using System.Windows.Forms;

namespace RacingGame.Forms
{
    /// <summary>Main menu — entry point of the WinForms UI.</summary>
    public class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            Text            = "🏎️  Time-Based Racing Game";
            Size            = new Size(520, 580);
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = Color.FromArgb(15, 15, 30);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            Font            = new Font("Segoe UI", 10f);

            BuildUI();
        }

        private void BuildUI()
        {
            // ── Title ────────────────────────────────────────────────────
            var title = new Label
            {
                Text      = "🏎️  RACING GAME",
                ForeColor = Color.Gold,
                Font      = new Font("Segoe UI", 26f, FontStyle.Bold),
                AutoSize  = false,
                Size      = new Size(480, 60),
                Location  = new Point(20, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var subtitle = new Label
            {
                Text      = "Console Simulation  v2.0  —  ALU Project 3",
                ForeColor = Color.Silver,
                Font      = new Font("Segoe UI", 10f, FontStyle.Italic),
                AutoSize  = false,
                Size      = new Size(480, 28),
                Location  = new Point(20, 95),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // ── Divider ──────────────────────────────────────────────────
            var divider = new Panel
            {
                BackColor = Color.Gold,
                Size      = new Size(440, 2),
                Location  = new Point(40, 135)
            };

            // ── Buttons ──────────────────────────────────────────────────
            int btnY = 160, gap = 65;

            var btnRace  = MakeButton("🏁   NEW RACE",         Color.FromArgb(0,150,80),  new Point(100, btnY));
            var btnBoard = MakeButton("🏆   LEADERBOARD",      Color.FromArgb(180,120,0), new Point(100, btnY + gap));
            var btnHelp  = MakeButton("📋   HOW TO PLAY",      Color.FromArgb(0,100,160), new Point(100, btnY + gap*2));
            var btnClear = MakeButton("🗑️    CLEAR LEADERBOARD",Color.FromArgb(140,40,40), new Point(100, btnY + gap*3));
            var btnExit  = MakeButton("🚪   EXIT",             Color.FromArgb(60,60,60),  new Point(100, btnY + gap*4));

            btnRace.Click  += (s, e) => OpenSetup();
            btnBoard.Click += (s, e) => OpenLeaderboard();
            btnHelp.Click  += (s, e) => ShowHelp();
            btnClear.Click += (s, e) => ClearLeaderboard();
            btnExit.Click  += (s, e) => Application.Exit();

            // ── Footer ───────────────────────────────────────────────────
            var footer = new Label
            {
                Text      = "Press SPACEBAR during a race to accelerate!",
                ForeColor = Color.DimGray,
                Font      = new Font("Segoe UI", 9f, FontStyle.Italic),
                AutoSize  = false,
                Size      = new Size(480, 24),
                Location  = new Point(20, 510),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Controls.AddRange(new Control[]
            {
                title, subtitle, divider,
                btnRace, btnBoard, btnHelp, btnClear, btnExit,
                footer
            });
        }

        private Button MakeButton(string text, Color color, Point loc)
        {
            return new Button
            {
                Text      = text,
                Size      = new Size(320, 48),
                Location  = loc,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(12, 0, 0, 0)
            };
        }

        private void OpenSetup()
        {
            var setup = new SetupForm();
            setup.ShowDialog(this);
        }

        private void OpenLeaderboard()
        {
            var lb = new LeaderboardForm();
            lb.ShowDialog(this);
        }

        private void ShowHelp()
        {
            MessageBox.Show(
                "HOW TO PLAY\n\n" +
                "1. Register your racer (name + age)\n" +
                "2. Pick a car — each has different stats\n" +
                "3. Pick a track — Easy to Extreme\n" +
                "4. During the race, press SPACEBAR to accelerate!\n" +
                "5. You have limited boosts per lap — use wisely\n" +
                "6. Weather changes mid-race and affects speed\n" +
                "7. Random events can help or hurt you\n" +
                "8. Finish all laps faster than the AI to win!\n\n" +
                "Your time is saved to the all-time leaderboard.",
                "How to Play",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void ClearLeaderboard()
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all leaderboard records?",
                "Clear Leaderboard",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                var svc = new Services.FileService(Core.Constants.GameConstants.LeaderboardFile);
                svc.Clear();
                MessageBox.Show("Leaderboard cleared!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
