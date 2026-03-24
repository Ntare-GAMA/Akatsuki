using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RacingGame.Models;
using RacingGame.Services;
using RacingGame.Core.Constants;

namespace RacingGame.Forms
{
    /// <summary>All-time leaderboard screen.</summary>
    public class LeaderboardForm : Form
    {
        public LeaderboardForm()
        {
            Text            = "🏆  All-Time Leaderboard";
            Size            = new Size(620, 520);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = Color.FromArgb(10, 10, 25);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Segoe UI", 10f);

            BuildUI();
        }

        private void BuildUI()
        {
            // Title
            Controls.Add(new Label
            {
                Text      = "🏆  ALL-TIME LEADERBOARD",
                ForeColor = Color.Gold,
                Font      = new Font("Segoe UI", 20f, FontStyle.Bold),
                Location  = new Point(20, 20),
                Size      = new Size(580, 44),
                TextAlign = ContentAlignment.MiddleCenter
            });

            // DataGridView
            var grid = new DataGridView
            {
                Location          = new Point(20, 80),
                Size              = new Size(580, 360),
                BackgroundColor   = Color.FromArgb(15, 15, 35),
                BorderStyle       = BorderStyle.None,
                GridColor         = Color.FromArgb(40, 40, 60),
                RowHeadersVisible = false,
                AllowUserToAddRows     = false,
                AllowUserToDeleteRows  = false,
                ReadOnly               = true,
                SelectionMode          = DataGridViewSelectionMode.FullRowSelect,
                Font                   = new Font("Consolas", 10f),
                AutoSizeColumnsMode    = DataGridViewAutoSizeColumnsMode.Fill,
                DefaultCellStyle       = { BackColor = Color.FromArgb(20,20,40), ForeColor = Color.White, SelectionBackColor = Color.FromArgb(0,80,40) },
                ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(30,30,60), ForeColor = Color.Gold, Font = new Font("Segoe UI", 10f, FontStyle.Bold) }
            };

            grid.Columns.Add("pos",    "Pos");
            grid.Columns.Add("name",   "Racer");
            grid.Columns.Add("track",  "Track");
            grid.Columns.Add("time",   "Best Time");
            grid.Columns.Add("laps",   "Laps");
            grid.Columns.Add("date",   "Date");

            grid.Columns["pos"]!.FillWeight   = 30;
            grid.Columns["name"]!.FillWeight  = 100;
            grid.Columns["track"]!.FillWeight = 120;
            grid.Columns["time"]!.FillWeight  = 60;
            grid.Columns["laps"]!.FillWeight  = 35;
            grid.Columns["date"]!.FillWeight  = 80;

            // Load data
            var lb      = new RaceLeaderboard(GameConstants.LeaderboardFile);
            var results = lb.GetAll();
            string[] medals = { "🥇", "🥈", "🥉" };

            for (int i = 0; i < results.Count; i++)
            {
                var r     = results[i];
                string pos = i < 3 ? medals[i] : $"#{i + 1}";
                grid.Rows.Add(pos, r.RacerName, r.TrackName, $"{r.TimeSeconds:F2}s", r.Laps, r.RacedOn.ToString("dd MMM yyyy"));

                // Gold row for top result
                if (i == 0)
                    grid.Rows[i].DefaultCellStyle.ForeColor = Color.Gold;
            }

            if (results.Count == 0)
            {
                grid.Rows.Add("—", "No records yet", "—", "—", "—", "—");
            }

            Controls.Add(grid);

            // Close button
            var btnClose = new Button
            {
                Text      = "Close",
                Location  = new Point(240, 452),
                Size      = new Size(140, 38),
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnClose.Click += (s, e) => Close();
            Controls.Add(btnClose);
        }
    }
}
