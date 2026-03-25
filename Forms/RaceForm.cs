using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RacingGame.Core.Constants;
using RacingGame.Enums;
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
        private readonly System.Windows.Forms.Timer _turnTimer;

        // ── UI Controls ──────────────────────────────────────────────────
        private Label   _lblLap, _lblTimer, _lblTimeLeft, _lblWeather, _lblBoosts, _lblFuel, _lblDistance, _lblEvent;
        private ProgressBar _prgTimeLeft;
        private Panel   _progressPanel;
        private List<(Label name, ProgressBar bar, Label pct)> _racerRows = new();

        // ── State ────────────────────────────────────────────────────────
        private bool _countingDown = true;
        private int  _countdown    = 3;
        private System.Windows.Forms.Timer _countdownTimer;
        private Label _lblCountdown;
        private TurnAction? _queuedAction;

        public RaceForm(HumanRacer human, List<AIRacer> aiRacers, Track track)
        {
            _engine = new RaceEngine(human, aiRacers, track);

            Text            = $"🏎️  Racing — {track.Name}";
            Size            = new Size(880, 620);
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

            _turnTimer = new System.Windows.Forms.Timer { Interval = GameConstants.AutoTurnMs };
            _turnTimer.Tick += (_, _) => AutoTurn();

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
            _lblTimer   = MakeLabel("⏱  Elapsed: 00:00.00", 210, y, 230, 28, Color.LimeGreen, 12f, FontStyle.Bold);
            _lblTimeLeft = MakeLabel($"⌛ Left: {GameConstants.RaceTimeLimitSeconds:F0}s", 450, y, 170, 28, Color.Orange, 12f, FontStyle.Bold);
            _lblWeather = MakeLabel("☀️  Sunny", 640, y, 220, 28, Color.DeepSkyBlue, 12f, FontStyle.Regular);
            y += 38;

            _prgTimeLeft = new ProgressBar
            {
                Location = new Point(450, y - 4),
                Size = new Size(390, 14),
                Minimum = 0,
                Maximum = 100,
                Value = 100
            };
            Controls.Add(_prgTimeLeft);

            // Divider
            Controls.Add(new Panel { BackColor = Color.FromArgb(50,50,80), Location = new Point(20,y), Size = new Size(820,1) });
            y += 10;

            // Progress bars panel
            _progressPanel = new Panel
            {
                Location  = new Point(20, y),
                Size      = new Size(820, 40 * (1 + aiRacers.Count) + 10),
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
            Controls.Add(new Panel { BackColor = Color.FromArgb(50,50,80), Location = new Point(20,y), Size = new Size(820,1) });
            y += 10;

            // Boost + event row
            _lblBoosts = MakeLabel($"⚡ BOOSTS: {GameConstants.MaxBoostsPerLap}", 20, y, 220, 26, Color.Yellow, 11f, FontStyle.Bold);
            _lblFuel   = MakeLabel("⛽ Fuel: --", 250, y, 260, 26, Color.DeepSkyBlue, 11f, FontStyle.Bold);
            var btnSpeedUp = new Button
            {
                Text = "Speed Up",
                Location = new Point(520, y - 2),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 130, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            var btnMaintain = new Button
            {
                Text = "Maintain",
                Location = new Point(630, y - 2),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(90, 90, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            var btnPit = new Button
            {
                Text = "Pit Stop",
                Location = new Point(740, y - 2),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(140, 80, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSpeedUp.Click += (_, _) => QueueAction(TurnAction.SpeedUp, "⚡ Speed Up queued");
            btnMaintain.Click += (_, _) => QueueAction(TurnAction.MaintainSpeed, "🚗 Maintain queued");
            btnPit.Click += (_, _) =>
            {
                QueueAction(TurnAction.PitStop, "⛽ Pit Stop queued");
            };
            Controls.AddRange(new Control[] { btnSpeedUp, btnMaintain, btnPit });
            y += 32;

            _lblEvent = MakeLabel("", 20, y, 820, 26, Color.Violet, 10f, FontStyle.Italic);
            y += 18;

            _lblDistance = MakeLabel("🛣️ Distance: -- km", 20, y, 820, 26, Color.FromArgb(100, 170, 90), 10f, FontStyle.Italic);
            y += 32;

            // Controls hint
            var hint = MakeLabel("Space: accelerate  |  M: maintain speed  |  F: pit stop refuel", 20, y, 820, 28,
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
                Size      = new Size(880, 200),
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
                    UpdateHUD();
                    _turnTimer.Start();
                };
                goTimer.Start();
            }
        }

        private void UpdateHUD()
        {
            // Timer
            _lblTimer.Text = "⏱  Elapsed: " + _engine.SimElapsedFormatted;
            double timeLeft = Math.Max(0, GameConstants.RaceTimeLimitSeconds - _engine.SimElapsedSeconds);
            _lblTimeLeft.Text = $"⌛ Left: {timeLeft:F1}s";
            int timePct = (int)Math.Clamp(timeLeft / GameConstants.RaceTimeLimitSeconds * 100.0, 0, 100);
            _prgTimeLeft.Value = timePct;

            // Weather
            _lblWeather.Text = _engine.Weather.GetEmoji() + " " + _engine.Weather.Current;

            // Lap
            int lap = Math.Min(_engine.CurrentLap, _engine.Track.DefaultLaps);
            _lblLap.Text = $"LAP  {lap} / {_engine.Track.DefaultLaps}  |  TURN {_engine.TurnNumber}";

            // Boosts
            _lblBoosts.Text = $"⚡ BOOSTS: {_engine.Human.BoostsLeft}";
            _lblBoosts.ForeColor = _engine.Human.BoostsLeft > 10 ? Color.Yellow
                                 : _engine.Human.BoostsLeft > 5  ? Color.Orange
                                                                   : Color.Tomato;

            // Fuel + distance covered
            var car = _engine.Human.SelectedCar;
            double fuelPct = car.Fuel / car.MaxFuel * 100.0;
            _lblFuel.Text = $"⛽ Fuel: {car.Fuel:F1}  ({fuelPct:F0}%)";
            _lblFuel.ForeColor = fuelPct > 40 ? Color.DeepSkyBlue
                               : fuelPct > 20 ? Color.Orange
                                              : Color.Tomato;
            double kmCovered = car.Mileage / GameConstants.TrackLength * _engine.Track.LengthKm;
            _lblDistance.Text = _engine.Human.CanRefuelNow
                ? "⛽ REFUEL AVAILABLE — press  F  now!"
                : $"🛣️ Distance: {kmCovered:F2} km | Fuel Used: {car.FuelUsed:F1}";

            _lblDistance.ForeColor = _engine.Human.CanRefuelNow
                ? Color.LimeGreen
                : Color.FromArgb(100, 170, 90);
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
            _turnTimer.Stop();
            // Save result
            var result = new Models.RaceResult(
                _engine.Human.Name,
                _engine.Track.Name,
                _engine.SimElapsedSeconds,
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
                    QueueAction(TurnAction.SpeedUp, "⚡ Speed Up queued");
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.M)
                {
                    QueueAction(TurnAction.MaintainSpeed, "🚗 Maintain queued");
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.F)
                {
                    QueueAction(TurnAction.PitStop, "⛽ Pit Stop queued");
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

        private void ExecuteTurn(TurnAction action)
        {
            if (_countingDown || _engine.Status != RaceStatus.InProgress) return;
            _engine.PerformTurn(action);
            UpdateHUD();
        }

        private void AutoTurn()
        {
            var action = _queuedAction ?? TurnAction.MaintainSpeed;
            _queuedAction = null; // consume selected action for this turn
            ExecuteTurn(action);
        }

        private void QueueAction(TurnAction action, string message)
        {
            if (_countingDown || _engine.Status != RaceStatus.InProgress) return;
            _queuedAction = action;
            _lblEvent.Text = message;
        }
    }
}
