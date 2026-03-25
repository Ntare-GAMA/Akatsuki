using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RacingGame.Enums;
using RacingGame.Exceptions;
using RacingGame.Models;
using RacingGame.Services;

namespace RacingGame.UI.Forms
{
    /// <summary>
    /// Multi-step wizard: register → pick car → pick track → launch race.
    /// </summary>
    public class NewRaceForm : Form
    {
        private readonly RaceLeaderboard _leaderboard;
        private readonly Panel[]         _steps;
        private int                      _currentStep = 0;

        // ── Selections ──────────────────────────────────────────────────
        private string _name    = "";
        private int    _age     = 0;
        private Car?   _car;
        private Track? _track;

        // ── Controls (referenced across methods) ────────────────────────
        private TextBox  _txtName  = new();
        private TextBox  _txtAge   = new();
        private ListBox  _lstCars  = new();
        private ListBox  _lstTracks= new();
        private Label    _lblStep  = new();
        private Button   _btnNext  = new();
        private Button   _btnBack  = new();

        public NewRaceForm(RaceLeaderboard leaderboard)
        {
            _leaderboard = leaderboard;
            _steps = new Panel[3];
            InitUI();
        }

        private void InitUI()
        {
            Theme.Apply(this);
            Text            = "New Race — Setup";
            Size            = new Size(520, 540);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            // ── Header ───────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 70,
                BackColor = Theme.Surface,
            };
            _lblStep = new Label
            {
                Dock      = DockStyle.Fill,
                Font      = Theme.FontSubTitle,
                ForeColor = Theme.AccentGold,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            pnlHeader.Controls.Add(_lblStep);

            var div = new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Theme.Accent };

            // ── Content area ─────────────────────────────────────────────
            var pnlContent = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Theme.Background,
                Padding   = new Padding(30, 20, 30, 10),
            };

            _steps[0] = BuildStep1();
            _steps[1] = BuildStep2();
            _steps[2] = BuildStep3();

            foreach (var s in _steps)
            {
                s.Dock = DockStyle.Fill;
                pnlContent.Controls.Add(s);
            }

            // ── Footer buttons ────────────────────────────────────────────
            var pnlFoot = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 64,
                BackColor = Theme.Surface,
                Padding   = new Padding(20, 10, 20, 10),
            };

            _btnBack = Theme.MakeSecondaryButton("◀  Back", 110, 42);
            _btnNext = Theme.MakeButton("Next  ▶", 130, 42);

            _btnBack.Dock = DockStyle.Left;
            _btnNext.Dock = DockStyle.Right;

            pnlFoot.Controls.Add(_btnBack);
            pnlFoot.Controls.Add(_btnNext);

            Controls.Add(pnlContent);
            Controls.Add(div);
            Controls.Add(pnlHeader);
            Controls.Add(pnlFoot);

            _btnNext.Click += OnNext;
            _btnBack.Click += OnBack;

            ShowStep(0);
        }

        // ── Step builders ────────────────────────────────────────────────

        private Panel BuildStep1()
        {
            var p = new Panel { BackColor = Color.Transparent };

            var lblN = Theme.MakeLabel("Racer Name", Theme.FontSubTitle, Theme.TextPrimary);
            lblN.Location = new Point(0, 10);
            _txtName = new TextBox
            {
                Location  = new Point(0, 40),
                Size      = new Size(420, 34),
                BackColor = Theme.Card,
                ForeColor = Theme.TextPrimary,
                Font      = Theme.FontMono,
                BorderStyle = BorderStyle.FixedSingle,
            };

            var lblA = Theme.MakeLabel("Age", Theme.FontSubTitle, Theme.TextPrimary);
            lblA.Location = new Point(0, 95);
            _txtAge = new TextBox
            {
                Location  = new Point(0, 125),
                Size      = new Size(120, 34),
                BackColor = Theme.Card,
                ForeColor = Theme.TextPrimary,
                Font      = Theme.FontMono,
                BorderStyle = BorderStyle.FixedSingle,
            };

            var note = Theme.MakeLabel("Age must be between 16 and 80.", Theme.FontSmall, Theme.TextSecondary);
            note.Location = new Point(0, 170);

            p.Controls.AddRange(new Control[] { lblN, _txtName, lblA, _txtAge, note });
            return p;
        }

        private Panel BuildStep2()
        {
            var p = new Panel { BackColor = Color.Transparent };

            var lbl = Theme.MakeLabel("Choose Your Car", Theme.FontSubTitle, Theme.TextPrimary);
            lbl.Location = new Point(0, 10);

            _lstCars = new ListBox
            {
                Location        = new Point(0, 45),
                Size            = new Size(440, 300),
                BackColor       = Theme.Card,
                ForeColor       = Theme.TextPrimary,
                Font            = Theme.FontMono,
                BorderStyle     = BorderStyle.FixedSingle,
                ItemHeight      = 28,
            };

            foreach (var c in Car.GetAllCars())
                _lstCars.Items.Add($"{c.Symbol}  {c.Name,-20}  [{c.Tier}]   Spd:{c.Stats.TopSpeed:F1}  Acc:{c.Stats.Acceleration:F1}");

            _lstCars.SelectedIndex = 0;

            p.Controls.AddRange(new Control[] { lbl, _lstCars });
            return p;
        }

        private Panel BuildStep3()
        {
            var p = new Panel { BackColor = Color.Transparent };

            var lbl = Theme.MakeLabel("Choose a Track", Theme.FontSubTitle, Theme.TextPrimary);
            lbl.Location = new Point(0, 10);

            _lstTracks = new ListBox
            {
                Location    = new Point(0, 45),
                Size        = new Size(440, 280),
                BackColor   = Theme.Card,
                ForeColor   = Theme.TextPrimary,
                Font        = Theme.FontMonoSm,
                BorderStyle = BorderStyle.FixedSingle,
                ItemHeight  = 44,
            };

            foreach (var t in Track.GetAllTracks())
                _lstTracks.Items.Add(
                    $"[{t.Difficulty,-8}]  {t.Name}\n" +
                    $"           {t.LengthKm}km  ·  {t.DefaultLaps} laps  ·  {t.Description}");

            _lstTracks.SelectedIndex = 0;
            _lstTracks.DrawMode = DrawMode.OwnerDrawFixed;
            _lstTracks.DrawItem += (s, e) =>
            {
                e.DrawBackground();
                bool sel = (e.State & DrawItemState.Selected) != 0;
                var bg = sel ? Theme.Accent : Theme.Card;
                var fg = Theme.TextPrimary;
                e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
                if (e.Index >= 0)
                    e.Graphics.DrawString(_lstTracks.Items[e.Index].ToString(),
                        Theme.FontMonoSm, new SolidBrush(fg),
                        new RectangleF(e.Bounds.X + 4, e.Bounds.Y + 4, e.Bounds.Width - 8, e.Bounds.Height - 4));
                e.DrawFocusRectangle();
            };

            p.Controls.AddRange(new Control[] { lbl, _lstTracks });
            return p;
        }

        // ── Navigation ───────────────────────────────────────────────────

        private void ShowStep(int i)
        {
            _currentStep = i;
            for (int j = 0; j < _steps.Length; j++)
                _steps[j].Visible = (j == i);

            _btnBack.Enabled = (i > 0);

            _lblStep.Text = i switch
            {
                0 => "Step 1 of 3  —  Register Racer",
                1 => "Step 2 of 3  —  Choose Your Car",
                2 => "Step 3 of 3  —  Choose Track",
                _ => ""
            };

            _btnNext.Text = i == 2 ? "🏁  START RACE" : "Next  ▶";
        }

        private void OnNext(object? s, EventArgs e)
        {
            if (_currentStep == 0)
            {
                if (!ValidateStep1()) return;
                ShowStep(1);
            }
            else if (_currentStep == 1)
            {
                if (_lstCars.SelectedIndex < 0) { ShowError("Please select a car."); return; }
                _car = Car.GetAllCars()[_lstCars.SelectedIndex];
                ShowStep(2);
            }
            else if (_currentStep == 2)
            {
                if (_lstTracks.SelectedIndex < 0) { ShowError("Please select a track."); return; }
                _track = Track.GetAllTracks()[_lstTracks.SelectedIndex];
                LaunchRace();
            }
        }

        private void OnBack(object? s, EventArgs e)
        {
            if (_currentStep > 0) ShowStep(_currentStep - 1);
        }

        private bool ValidateStep1()
        {
            _name = _txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(_name)) { ShowError("Name cannot be empty."); return false; }
            if (!int.TryParse(_txtAge.Text.Trim(), out _age) || _age < 16 || _age > 80)
            { ShowError("Age must be a number between 16 and 80."); return false; }
            return true;
        }

        private void LaunchRace()
        {
            try
            {
                var car     = _car!;
                var track   = _track!;
                var human   = new HumanRacer(_name, _age, car, track.DefaultLaps);
                var rng     = new Random();
                var aiCars  = Car.GetAllCars();
                var diffs   = new[] { AIDifficulty.Rookie, AIDifficulty.Amateur, AIDifficulty.Pro };
                var names   = new[] { "Speedo-9", "NightOwl", "Turbo-X" };

                var aiList  = new List<AIRacer>();
                for (int i = 0; i < 3; i++)
                    aiList.Add(new AIRacer(names[i], aiCars[rng.Next(aiCars.Length)], diffs[i], track.DefaultLaps));

                var raceForm = new global::RacingGame.Forms.RaceForm(human, aiList, track);
                raceForm.ShowDialog(this);
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Could not start race:\n{ex.Message}");
            }
        }

        private void ShowError(string msg) =>
            MessageBox.Show(msg, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
