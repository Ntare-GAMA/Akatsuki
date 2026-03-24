using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RacingGame.Forms
{
    /// <summary>Race results screen shown after every race.</summary>
    public class ResultsForm : Form
    {
        public ResultsForm(List<(string Name, double Time, bool IsHuman)> results, string trackName)
        {
            Text            = "🏁  Race Results";
            Size            = new Size(480, 420);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = Color.FromArgb(10, 10, 25);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Segoe UI", 10f);

            BuildUI(results, trackName);
        }

        private void BuildUI(List<(string Name, double Time, bool IsHuman)> results, string trackName)
        {
            int y = 20;

            // Title
            Controls.Add(new Label
            {
                Text      = "🏁  RACE RESULTS",
                ForeColor = Color.Gold,
                Font      = new Font("Segoe UI", 20f, FontStyle.Bold),
                Location  = new Point(20, y),
                Size      = new Size(440, 44),
                TextAlign = ContentAlignment.MiddleCenter
            });
            y += 48;

            Controls.Add(new Label
            {
                Text      = "Track: " + trackName,
                ForeColor = Color.Silver,
                Font      = new Font("Segoe UI", 10f, FontStyle.Italic),
                Location  = new Point(20, y),
                Size      = new Size(440, 22),
                TextAlign = ContentAlignment.MiddleCenter
            });
            y += 30;

            // Divider
            Controls.Add(new Panel { BackColor = Color.Gold, Location = new Point(30, y), Size = new Size(420, 2) });
            y += 12;

            // Result rows
            string[] medals = { "🥇", "🥈", "🥉", "#4 ", "#5 " };
            for (int i = 0; i < results.Count; i++)
            {
                var (name, time, isHuman) = results[i];
                Color fg = isHuman ? Color.LimeGreen : (i == 0 ? Color.Gold : Color.Silver);
                string medal = i < medals.Length ? medals[i] : $"#{i+1} ";

                var row = new Panel
                {
                    Location  = new Point(30, y),
                    Size      = new Size(420, 42),
                    BackColor = isHuman ? Color.FromArgb(0, 40, 20) : Color.FromArgb(20, 20, 35)
                };

                row.Controls.Add(new Label
                {
                    Text      = $"{medal}  {name}",
                    ForeColor = fg,
                    Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                    Location  = new Point(10, 10),
                    Size      = new Size(260, 24),
                    AutoSize  = false
                });

                row.Controls.Add(new Label
                {
                    Text      = $"{time:F2}s",
                    ForeColor = fg,
                    Font      = new Font("Consolas", 12f, FontStyle.Bold),
                    Location  = new Point(300, 10),
                    Size      = new Size(110, 24),
                    TextAlign = ContentAlignment.MiddleRight,
                    AutoSize  = false
                });

                Controls.Add(row);
                y += 48;
            }

            y += 10;

            // Buttons
            var btnBoard = new Button
            {
                Text      = "🏆  View Leaderboard",
                Location  = new Point(30, y),
                Size      = new Size(200, 42),
                BackColor = Color.FromArgb(160, 120, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnBoard.Click += (s, e) =>
            {
                new LeaderboardForm().ShowDialog(this);
            };

            var btnMenu = new Button
            {
                Text      = "🏠  Main Menu",
                Location  = new Point(245, y),
                Size      = new Size(205, 42),
                BackColor = Color.FromArgb(0, 100, 160),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnMenu.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { btnBoard, btnMenu });
            ClientSize = new Size(480, y + 60);
        }
    }
}
