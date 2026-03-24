using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RacingGame.Core.Constants;
using RacingGame.Models;
using RacingGame.Services;

namespace RacingGame.Forms
{
    /// <summary>
    /// Live race screen.
    /// Uses a WinForms Timer to drive the race engine each tick.
    /// Player presses SPACEBAR to boost.
    /// </summary>
    public class RaceForm : Form
    {
        // ── Engine ───────────────────────────────────────────────────────
        private readonly RaceEngine  _engine;
        private readonly System.Windows.Forms.Timer _gameTimer;

        // ── UI Controls ──────────────────────────────────────────────────
        private Label   _lblLap, _lblTimer, _lblWeather, _lblBoosts, _lblFuel, _lblDistance, _lblEvent;
        private Panel   _progressPanel;
        private List<(Label name, ProgressBar bar, Label pct)> _racerRows = new();

        // ── State ────────────────────────────────────────────────────────
        private bool _countingDown = true;
        private int  _countdown    = 3;
        private System.Windows.Forms.Timer _countdownTimer;
        private Label _lblCountdown;

        public RaceForm(HumanRacer human, List<AIRacer> aiRacers, Track track)
        {
            _engine = new RaceEngine(human, aiRacers, track);

            Text            = $"🏎️  Racing — {track.Name}";
            Size            = new Size(700, 520);
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = Color.FromArgb(10, 10, 25);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            KeyPreview      = true;
            Font            = new Font("Segoe UI", 10f);

            // Wire up engine events
            _engine.LapCompleted  += OnLapComplete;
            _engine.RaceFinished  += OnRaceFinished;
            _engine.EventTriggered += msg => _lblEvent.Text = "⚡ " + msg;

            _gameTimer = new System.Windows.Forms.Timer { Interval = GameConstants.RaceLoopMs };
            _gameTimer.Tick += GameLoop;

            _countdownTimer = new System.Windows.Forms.Timer { Interval = 900 };
            _countdownTimer.Tick += CountdownTick;

            BuildUI(human, aiRacers, track);
            StartCountdown();
        }

        // ── UI Builder ───────────────────────────────────────────────────
        private void BuildUI(HumanRacer human, List<AIRacer> aiRacers, Track track)
        {
            int y = 18;

            // Header row
            _lblLap = MakeLabel($"LAP  1 / {track.DefaultLaps}", 20, y, 180, 28, Color.Gold, 13f, FontStyle.Bold);
            _lblTimer   = MakeLabel("⏱  00:00.00", 210, y, 200, 28, Color.LimeGreen, 13f, FontStyle.Bold);
            _lblWeather = MakeLabel("☀️  Sunny", 420, y, 240, 28, Color.DeepSkyBlue, 12f, FontStyle.Regular);
            y += 38;

            // Divider
            Controls.Add(new Panel { BackColor = Color.FromArgb(50,50,80), Location = new Point(20,y), Size = new Size(648,1) });
            y += 10;

            // Progress bars panel
            _progressPanel = new Panel
            {
                Location  = new Point(20, y),
                Size      = new Size(648, 40 * (1 + aiRacers.Count) + 10),
                BackColor = Color.Transparent
            };
            Controls.Add(_progressPanel);

            // Human row
            AddRacerRow(human.Name, human.SelectedCar.Symbol + " YOU", Color.LimeGreen, 0);

            // AI rows
            for (int i = 0; i < aiRacers.Count; i++)
                AddRacerRow(aiRacers[i].Name, "🤖 " + aiRacers[i].Name, Color.Silver, i + 1);

            y += _progressPanel.Height + 12;

            // Divider
            Controls.Add(new Panel { BackColor = Color.FromArgb(50,50,80), Location = new Point(20,y), Size = new Size(648,1) });
            y += 10;

            // Boost + event row
            _lblBoosts = MakeLabel($"⚡ BOOSTS: {GameConstants.MaxBoostsPerLap}", 20, y, 300, 26, Color.Yellow, 11f, FontStyle.Bold);
            _lblFuel   = MakeLabel("⛽ Fuel: --", 330, y, 318, 26, Color.DeepSkyBlue, 11f, FontStyle.Bold);
            y += 32;

            _lblEvent = MakeLabel("", 20, y, 648, 26, Color.Violet, 10f, FontStyle.Italic);
            y += 18;

            _lblDistance = MakeLabel("🛣️ Distance: -- km", 20, y, 648, 26, Color.FromArgb(100, 170, 90), 10f, FontStyle.Italic);
            y += 32;

            // Controls hint
            var hint = MakeLabel("Space: accelerate  |  M: maintain speed  |  F: refuel", 20, y, 648, 28,
                                 Color.FromArgb(100,100,130), 10f, FontStyle.Italic);
            hint.TextAlign = ContentAlignment.MiddleCenter;
            y += 38;

            // Countdown overlay
            _lblCountdown = new Label
            {
                Text      = "3",
                ForeColor = Color.Gold,
                Font      = new Font("Segoe UI", 72f, FontStyle.Bold),
                AutoSize  = false,
                Size      = new Size(700, 200),
                Location  = new Point(0, 130),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            Controls.Add(_lblCountdown);
            _lblCountdown.BringToFront();

            Controls.AddRange(new Control[] { _lblLap, _lblTimer, _lblWeather, _lblBoosts, _lblFuel, _lblDistance, _lblEvent, hint });
        }

        private void AddRacerRow(string key, string displayName, Color barColor, int rowIndex)
        {
            int y = rowIndex * 40 + 5;

            var lblName = new Label
            {
                Text      = displayName,
                Location  = new Point(0, y + 8),
                Size      = new Size(140, 22),
                ForeColor = barColor,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                AutoSize  = false
            };

            var bar = new ProgressBar
            {
                Location    = new Point(145, y + 6),
                Size        = new Size(430, 22),
                Minimum     = 0,
                Maximum     = 100,
                Value       = 0,
                Style       = ProgressBarStyle.Continuous,
                ForeColor   = barColor
            };

            var lblPct = new Label
            {
                Text      = "0%",
                Location  = new Point(582, y + 8),
                Size      = new Size(55, 22),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f),
                AutoSize  = false
            };

            _progressPanel.Controls.AddRange(new Control[] { lblName, bar, lblPct });
            _racerRows.Add((lblName, bar, lblPct));
        }

        // ── Countdown ────────────────────────────────────────────────────
        private void StartCountdown()
        {
            _lblCountdown.Text = _countdown.ToString();
            _countdownTimer.Start();
        }

        private void CountdownTick(object? sender, EventArgs e)
        {
            _countdown--;
            if (_countdown > 0)
            {
                _lblCountdown.Text      = _countdown.ToString();
                _lblCountdown.ForeColor = _countdown == 1 ? Color.Tomato : Color.Gold;
            }
            else
            {
                _countdownTimer.Stop();
                _lblCountdown.Text      = "GO!";
                _lblCountdown.ForeColor = Color.LimeGreen;

                var goTimer = new System.Windows.Forms.Timer { Interval = 700 };
                goTimer.Tick += (s, ev) =>
                {
                    goTimer.Stop();
                    _lblCountdown.Visible = false;
                    _countingDown = false;
                    _engine.Start();
                    _gameTimer.Start();
                };
                goTimer.Start();
            }
        }

        // ── Game loop ────────────────────────────────────────────────────
        private void GameLoop(object? sender, EventArgs e)
        {
            if (_countingDown) return;

            _engine.Tick();
            UpdateHUD();
        }

        private void UpdateHUD()
        {
            // Timer
            _lblTimer.Text = "⏱  " + _engine.Timer.ElapsedFormatted;

            // Weather
            _lblWeather.Text = _engine.Weather.GetEmoji() + " " + _engine.Weather.Current;

            // Lap
            int lap = Math.Min(_engine.CurrentLap, _engine.Track.DefaultLaps);
            _lblLap.Text = $"LAP  {lap} / {_engine.Track.DefaultLaps}";

            // Boosts
            _lblBoosts.Text = $"⚡ BOOSTS: {_engine.Human.BoostsLeft}";
            _lblBoosts.ForeColor = _engine.Human.BoostsLeft > 10 ? Color.Yellow
                                 : _engine.Human.BoostsLeft > 5  ? Color.Orange
                                                                   : Color.Tomato;

            // Fuel + distance covered
            var car = _engine.Human.SelectedCar;
            _lblFuel.Text = $"⛽ Fuel: {car.Fuel:F1} (Used: {car.FuelUsed:F1})";
            double kmCovered = car.Mileage / GameConstants.TrackLength * _engine.Track.LengthKm;
            _lblDistance.Text = $"🛣️ Distance: {kmCovered:F2} km";

            // Human progress bar (row 0)
            UpdateRow(0, _engine.Human.Progress);

            // AI progress bars
            for (int i = 0; i < _engine.AIRacers.Count; i++)
                UpdateRow(i + 1, _engine.AIRacers[i].Progress);
        }

        private void UpdateRow(int index, double progress)
        {
            if (index >= _racerRows.Count) return;
            int pct = (int)(progress / GameConstants.TrackLength * 100);
            pct = Math.Clamp(pct, 0, 100);
            _racerRows[index].bar.Value = pct;
            _racerRows[index].pct.Text  = pct + "%";
        }

        // ── Engine events ─────────────────────────────────────────────────
        private void OnLapComplete(int lap, double lapTime)
        {
            _lblEvent.ForeColor = Color.LimeGreen;
            _lblEvent.Text      = $"✅  LAP {lap} COMPLETE!   Time: {lapTime:F2}s";
        }

        private void OnRaceFinished(List<(string Name, double Time, bool IsHuman)> results)
        {
            _gameTimer.Stop();

            // Save result
            var result = new Models.RaceResult(
                _engine.Human.Name,
                _engine.Track.Name,
                _engine.Human.TotalRaceTime(),
                _engine.Track.DefaultLaps);

            var lb = new Services.RaceLeaderboard(Core.Constants.GameConstants.LeaderboardFile);
            lb.Record(result);

            // Open results screen
            var resForm = new ResultsForm(results, _engine.Track.Name);
            resForm.ShowDialog(this);
            Close();
        }

        // ── Key input ────────────────────────────────────────────────────
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!_countingDown)
            {
                if (e.KeyCode == Keys.Space)
                {
                    _engine.ApplyBoost();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.M)
                {
                    bool ok = _engine.ToggleMaintainSpeed();
                    _lblEvent.Text = ok
                        ? (_engine.Human.MaintainSpeedEnabled ? "Cruise ON" : "Cruise OFF")
                        : "Cruise unavailable (need speed + fuel)";
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.F)
                {
                    bool ok = _engine.TryRefuel();
                    _lblEvent.Text = ok ? "⛽ Fuel refilled" : "⛽ Can't refuel here";
                    e.SuppressKeyPress = true;
                }
            }

            base.OnKeyDown(e);
        }

        // ── Helpers ──────────────────────────────────────────────────────
        private Label MakeLabel(string text, int x, int y, int w, int h,
                                Color color, float size, FontStyle style)
        {
            var lbl = new Label
            {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(w, h),
                ForeColor = color,
                Font      = new Font("Segoe UI", size, style),
                AutoSize  = false
            };
            Controls.Add(lbl);
            return lbl;
        }
    }
}
