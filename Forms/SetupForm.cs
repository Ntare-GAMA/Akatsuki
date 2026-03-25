using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RacingGame.Enums;
using RacingGame.Exceptions;
using RacingGame.Models;

namespace RacingGame.Forms
{
    /// <summary>Setup screen: player details, car selection, track selection.</summary>
    public class SetupForm : Form
    {
        private TextBox   _txtName   = new();
        private TextBox   _txtAge    = new();
        private ListBox   _lstCars   = new();
        private ListBox   _lstTracks = new();
        private Label     _lblStatus = new();

        private Car[]          _cars;
        private List<Track>    _tracks;

        public SetupForm()
        {
            Text            = "Race Setup";
            Size            = new Size(560, 620);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = Color.FromArgb(15, 15, 30);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            Font            = new Font("Segoe UI", 10f);

            _cars   = Car.GetAllCars();
            _tracks = Track.GetAllTracks();

            BuildUI();
        }

        private void BuildUI()
        {
            int x = 30, y = 20;

            // ── Title ────────────────────────────────────────────────────
            AddLabel("⚙️  RACE SETUP", x, y, 500, 36, Color.Gold, 18f, FontStyle.Bold);
            AddDivider(x, y + 44, 500);
            y += 60;

            // ── Player Info ──────────────────────────────────────────────
            AddLabel("YOUR NAME", x, y, 200, 22, Color.Silver, 9f, FontStyle.Bold);
            AddLabel("YOUR AGE", x + 270, y, 200, 22, Color.Silver, 9f, FontStyle.Bold);
            y += 24;

            _txtName = MakeTextBox(x, y, 240);
            _txtAge  = MakeTextBox(x + 270, y, 100);
            Controls.AddRange(new Control[] { _txtName, _txtAge });
            y += 50;

            // ── Car Selection ────────────────────────────────────────────
            AddLabel("🚗  SELECT YOUR CAR", x, y, 500, 22, Color.Gold, 10f, FontStyle.Bold);
            y += 26;

            _lstCars = new ListBox
            {
                Location      = new Point(x, y),
                Size          = new Size(500, 130),
                BackColor     = Color.FromArgb(25, 25, 45),
                ForeColor     = Color.White,
                Font          = new Font("Consolas", 9.5f),
                BorderStyle   = BorderStyle.FixedSingle,
                IntegralHeight = false
            };
            foreach (var c in _cars)
                _lstCars.Items.Add($"{c.Symbol}  {c.Name,-17} [{c.Tier,-9}] Spd:{c.Stats.TopSpeed:F1} FuelCap:{c.MaxFuel,4:F0} Use:{c.FuelUseMultiplier:F2}x");
            _lstCars.SelectedIndex = 0;
            Controls.Add(_lstCars);
            y += 140;

            // ── Track Selection ───────────────────────────────────────────
            AddLabel("🏁  SELECT TRACK", x, y, 500, 22, Color.Gold, 10f, FontStyle.Bold);
            y += 26;

            _lstTracks = new ListBox
            {
                Location      = new Point(x, y),
                Size          = new Size(500, 115),
                BackColor     = Color.FromArgb(25, 25, 45),
                ForeColor     = Color.White,
                Font          = new Font("Consolas", 9.5f),
                BorderStyle   = BorderStyle.FixedSingle,
                IntegralHeight = false
            };
            foreach (var t in _tracks)
                _lstTracks.Items.Add($"[{t.Difficulty,-8}]  {t.Name,-26}  {t.LengthKm}km  {t.DefaultLaps} laps");
            _lstTracks.SelectedIndex = 0;
            Controls.Add(_lstTracks);
            y += 125;

            // ── Status label ─────────────────────────────────────────────
            _lblStatus = new Label
            {
                Location  = new Point(x, y),
                Size      = new Size(500, 24),
                ForeColor = Color.Tomato,
                Font      = new Font("Segoe UI", 9f, FontStyle.Italic)
            };
            Controls.Add(_lblStatus);
            y += 28;

            // ── Start Button ─────────────────────────────────────────────
            var btnStart = new Button
            {
                Text      = "🏁   START RACE",
                Location  = new Point(x, y),
                Size      = new Size(500, 48),
                BackColor = Color.FromArgb(0, 150, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnStart.Click += OnStartClicked;
            Controls.Add(btnStart);
        }

        private void OnStartClicked(object? sender, EventArgs e)
        {
            try
            {
                string name = _txtName.Text.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidRacerException("", "Please enter your racer name.");

                if (!int.TryParse(_txtAge.Text.Trim(), out int age) || age < 16 || age > 80)
                    throw new InvalidRacerException(name, "Age must be a number between 16 and 80.");

                if (_lstCars.SelectedIndex < 0)
                    throw new InvalidRacerException(name, "Please select a car.");

                if (_lstTracks.SelectedIndex < 0)
                    throw new InvalidRacerException(name, "Please select a track.");

                var car   = _cars[_lstCars.SelectedIndex];
                var track = _tracks[_lstTracks.SelectedIndex];

                var human = new HumanRacer(name, age, car, track.DefaultLaps);

                // Build AI opponents
                var aiNames = new[] { "Speedo-9", "NightOwl", "TurboX" };
                var diffs   = new[] { AIDifficulty.Rookie, AIDifficulty.Pro, AIDifficulty.Elite };
                var aiCars  = Car.GetAllCars();
                var rng     = new Random();
                var aiList  = new System.Collections.Generic.List<AIRacer>();
                for (int i = 0; i < 3; i++)
                    aiList.Add(new AIRacer(aiNames[i], aiCars[rng.Next(aiCars.Length)], diffs[i], track.DefaultLaps));

                var raceForm = new RaceForm(human, aiList, track);
                raceForm.ShowDialog(this);
                Close();
            }
            catch (InvalidRacerException ex)
            {
                _lblStatus.Text = "❌ " + ex.Message;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────
        private void AddLabel(string text, int x, int y, int w, int h,
                              Color color, float size, FontStyle style)
        {
            Controls.Add(new Label
            {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(w, h),
                ForeColor = color,
                Font      = new Font("Segoe UI", size, style),
                AutoSize  = false
            });
        }

        private void AddDivider(int x, int y, int w)
        {
            Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(60, 60, 80),
                Location  = new Point(x, y),
                Size      = new Size(w, 1)
            });
        }

        private TextBox MakeTextBox(int x, int y, int w)
        {
            return new TextBox
            {
                Location  = new Point(x, y),
                Size      = new Size(w, 30),
                BackColor = Color.FromArgb(25, 25, 45),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 11f),
                BorderStyle = BorderStyle.FixedSingle
            };
        }
    }
}
