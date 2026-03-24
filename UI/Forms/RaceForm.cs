using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RacingGame.Models;
using RacingGame.Core.Constants;
using RacingGame.Services;

namespace RacingGame.UI.Forms
{
    /// <summary>
    /// Live race window.
    /// A WinForms Timer ticks every 120 ms, calls RaceSession.Tick(),
    /// then repaints the custom race track panel.
    /// SPACEBAR is captured via KeyDown to call session.Boost().
    /// </summary>
    public class RaceForm : Form
    {
        private readonly RaceSession      _session;
        private readonly RaceLeaderboard  _leaderboard;
        private readonly System.Windows.Forms.Timer _ticker;

        // ── Header labels ────────────────────────────────────────────────
        private Label _lblLap, _lblTime, _lblWeather, _lblTrack, _lblPos;

        // ── Race track panel (custom-painted) ────────────────────────────
        private Panel _pnlTrack;

        // ── Player info bar ──────────────────────────────────────────────
        private Label _lblBoosts, _lblEvent, _lblInstruction, _lblFuel, _lblMileage;

        // ── Countdown state ──────────────────────────────────────────────
        private int    _countdown   = 3;
        private bool   _started     = false;
        private System.Windows.Forms.Timer _countdownTimer;
        private Label  _lblCountdown;

        // Small local HUD messages (e.g., refuel feedback)
        private int    _uiTickCount = 0;
        private string _localHudMessage = string.Empty;
        private int    _localHudMessageClearAtTick = 0;

        // ── Racer row data ───────────────────────────────────────────────
        private List<(string Name, bool IsHuman)> _racerMeta = new();

        public RaceForm(RaceSession session, RaceLeaderboard leaderboard)
        {
            _session     = session;
            _leaderboard = leaderboard;

            // Build racer meta for painting
            _racerMeta.Add((session.Human.Name, true));
            foreach (var ai in session.AIRacers)
                _racerMeta.Add((ai.Name, false));

            _ticker = new System.Windows.Forms.Timer { Interval = 120 };
            _ticker.Tick += OnTick;

            _countdownTimer = new System.Windows.Forms.Timer { Interval = 950 };
            _countdownTimer.Tick += OnCountdown;

            InitUI();
            StartCountdown();
        }

        // ════════════════════════════════════════════════════════════════
        // UI Construction
        // ════════════════════════════════════════════════════════════════

        private void InitUI()
        {
            Theme.Apply(this);
            Text            = $"Racing — {_session.Track.Name}";
            Size            = new Size(700, 580);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            KeyPreview      = true;   // ← lets this Form receive KeyDown before child controls

            // ── Top info bar ─────────────────────────────────────────────
            var pnlTop = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 64,
                BackColor = Theme.Surface,
                Padding   = new Padding(12, 8, 12, 8),
            };

            _lblTrack   = MkLbl("", Theme.FontSubTitle, Theme.AccentGold);
            _lblLap     = MkLbl("", Theme.FontBody,     Theme.TextPrimary);
            _lblTime    = MkLbl("", Theme.FontMono,     Theme.AccentGreen);
            _lblWeather = MkLbl("", Theme.FontBody,     Theme.AccentBlue);
            _lblPos     = MkLbl("", Theme.FontBody,     Theme.AccentGold);

            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 5,
                RowCount    = 1,
                BackColor   = Color.Transparent,
            };
            float col = 100f / 5;
            for (int i = 0; i < 5; i++)
                tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, col));

            tbl.Controls.Add(Center(_lblTrack),   0, 0);
            tbl.Controls.Add(Center(_lblLap),     1, 0);
            tbl.Controls.Add(Center(_lblTime),    2, 0);
            tbl.Controls.Add(Center(_lblWeather), 3, 0);
            tbl.Controls.Add(Center(_lblPos),     4, 0);

            pnlTop.Controls.Add(tbl);

            // ── Divider ──────────────────────────────────────────────────
            var div = new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Theme.Accent };

            // ── Custom-painted race track ────────────────────────────────
            _pnlTrack = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Theme.TrackBg,
            };
            _pnlTrack.Paint += OnPaintTrack;

            // ── Bottom bar ───────────────────────────────────────────────
            var pnlBot = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 100,
                BackColor = Theme.Surface,
                Padding   = new Padding(20, 8, 20, 8),
            };

            _lblBoosts = MkLbl("⚡ Boosts: --", Theme.FontSubTitle, Theme.AccentGold);
            _lblBoosts.Location = new Point(20, 10);

            // Add fuel and mileage labels
            _lblFuel = MkLbl("⛽ Fuel: --", Theme.FontSubTitle, Theme.AccentBlue);
            _lblFuel.Location = new Point(220, 10);
            _lblMileage = MkLbl("🛣️ Mileage: --", Theme.FontSubTitle, Theme.AccentGreen);
            _lblMileage.Location = new Point(420, 10);

            _lblInstruction = MkLbl("Space: accelerate  |  M: maintain speed  |  F: refuel", Theme.FontBody, Theme.TextSecondary);
            _lblInstruction.Location = new Point(20, 44);

            _lblEvent = MkLbl("", Theme.FontBody, Theme.AccentGold);
            _lblEvent.Location  = new Point(20, 66);
            _lblEvent.AutoSize  = false;
            _lblEvent.Size      = new Size(640, 24);

            pnlBot.Controls.AddRange(new Control[] { _lblBoosts, _lblFuel, _lblMileage, _lblInstruction, _lblEvent });

            // ── Countdown overlay ─────────────────────────────────────────
            _lblCountdown = new Label
            {
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 72, FontStyle.Bold),
                ForeColor = Theme.AccentGold,
                BackColor = Color.FromArgb(200, 15, 15, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                Text      = "3",
            };

            Controls.Add(_pnlTrack);
            Controls.Add(div);
            Controls.Add(pnlTop);
            Controls.Add(pnlBot);
            Controls.Add(_lblCountdown); // on top of everything

            KeyDown += OnKeyDown;
            FormClosing += (_, e) => { _ticker.Stop(); _countdownTimer.Stop(); };
        }

        // ════════════════════════════════════════════════════════════════
        // Countdown
        // ════════════════════════════════════════════════════════════════

        private void StartCountdown()
        {
            _lblCountdown.Text = "3";
            _countdownTimer.Start();
        }

        private void OnCountdown(object? s, EventArgs e)
        {
            _countdown--;
            if (_countdown > 0)
            {
                _lblCountdown.Text = _countdown.ToString();
            }
            else if (_countdown == 0)
            {
                _lblCountdown.Text = "GO! 🏁";
            }
            else
            {
                _countdownTimer.Stop();
                _lblCountdown.Visible = false;
                _started = true;
                _session.StartTimer();
                _ticker.Start();
            }
        }

        // ════════════════════════════════════════════════════════════════
        // Race tick
        // ════════════════════════════════════════════════════════════════

        private void OnTick(object? s, EventArgs e)
        {
            _uiTickCount++;
            _session.Tick();

            // Update header labels
            _lblTrack.Text   = _session.Track.Name;
            _lblLap.Text     = $"LAP {_session.CurrentLap} / {_session.Track.DefaultLaps}";
            _lblTime.Text    = _session.ElapsedFormatted;
            _lblWeather.Text = $"{_session.Weather.GetEmoji()} {_session.Weather.Current}";
            _lblPos.Text     = $"P{_session.HumanPosition()} / {1 + _session.AIRacers.Count}";


            // Update boost counter
            _lblBoosts.Text  = $"⚡ Boosts left: {_session.Human.BoostsLeft}";
            _lblBoosts.ForeColor = _session.Human.BoostsLeft > 5
                ? Theme.AccentGreen : Theme.Accent;

            // Update fuel + distance
            var car = _session.Human.SelectedCar;
            double fuel = car.Fuel;
            double fuelUsed = car.FuelUsed;
            double kmCovered = car.Mileage / GameConstants.TrackLength * _session.Track.LengthKm;
            _lblFuel.Text = $"⛽ Fuel: {fuel:F1} (Used: {fuelUsed:F1})";
            _lblMileage.Text = $"🛣️ Distance: {kmCovered:F2} km";

            // Update control hint
            if (_session.Human.MaintainSpeedEnabled)
                _lblInstruction.Text = "Cruise ON — press M to stop  |  F: refuel";
            else
                _lblInstruction.Text = "Space: accelerate  |  M: maintain speed  |  F: refuel";

            if (_session.Human.CanRefuelNow)
                _lblInstruction.Text += "  (refuel available)";

            // Event message
            if (!string.IsNullOrEmpty(_session.LastEvent))
            {
                _lblEvent.Text = _session.LastEvent;
            }
            else if (_uiTickCount <= _localHudMessageClearAtTick)
            {
                _lblEvent.Text = _localHudMessage;
            }
            else
            {
                _lblEvent.Text = "";
                _localHudMessage = string.Empty;
            }

            // Repaint the track
            _pnlTrack.Invalidate();

            // Race complete
            if (_session.IsRaceComplete)
            {
                _ticker.Stop();
                ShowResults();
            }
        }

        // ════════════════════════════════════════════════════════════════
        // Custom track painting
        // ════════════════════════════════════════════════════════════════

        private void OnPaintTrack(object? s, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Theme.TrackBg);

            int   rowH   = _pnlTrack.Height / (_racerMeta.Count + 1);
            int   barW   = _pnlTrack.Width - 180;
            int   barX   = 150;
            int   barH   = Math.Min(28, rowH - 14);

            for (int i = 0; i < _racerMeta.Count; i++)
            {
                var (name, isHuman) = _racerMeta[i];
                int pct = isHuman
                    ? _session.HumanProgressPct
                    : _session.AIProgressPct(i - 1);

                int y = (i + 1) * rowH - barH / 2;

                // ── Name label ─────────────────────────────────────────
                var nameBrush = new SolidBrush(isHuman ? Theme.AccentGreen : Theme.AccentBlue);
                var nameFont  = isHuman ? Theme.FontSubTitle : Theme.FontBody;
                g.DrawString(
                    (isHuman ? "🎮 " : "🤖 ") + name,
                    nameFont, nameBrush,
                    new RectangleF(4, y, barX - 8, barH + 4));

                // ── Track bar background ───────────────────────────────
                var bgRect = new Rectangle(barX, y, barW, barH);
                g.FillRectangle(new SolidBrush(Theme.Card), bgRect);
                g.DrawRectangle(new Pen(Theme.Border, 1), bgRect);

                // ── Progress fill ──────────────────────────────────────
                int filled = (int)(barW * pct / 100.0);
                if (filled > 0)
                {
                    var fillColor = isHuman ? Theme.AccentGreen : Theme.AccentBlue;
                    var grad = new LinearGradientBrush(
                        new Rectangle(barX, y, Math.Max(1, filled), barH),
                        fillColor,
                        Color.FromArgb(120, fillColor),
                        LinearGradientMode.Horizontal);
                    g.FillRectangle(grad, new Rectangle(barX, y, filled, barH));
                }

                // ── Car emoji at current position ──────────────────────
                int carX = barX + (int)(barW * pct / 100.0) - 10;
                carX = Math.Clamp(carX, barX, barX + barW - 20);
                var symbol = isHuman
                    ? _session.Human.SelectedCar.Symbol
                    : _session.AIRacers[i - 1].SelectedCar.Symbol;
                g.DrawString(symbol, Theme.FontBody, Brushes.White,
                    new PointF(carX, y - 2));

                // ── % text ─────────────────────────────────────────────
                g.DrawString($"{pct}%", Theme.FontSmall,
                    new SolidBrush(Theme.TextSecondary),
                    new PointF(barX + barW + 6, y + 6));
            }

            // ── Finish line ────────────────────────────────────────────
            int finX = barX + barW;
            g.DrawLine(new Pen(Theme.AccentGold, 2), finX, 10, finX, _pnlTrack.Height - 10);
            g.DrawString("🏁", Theme.FontBody, Brushes.White, finX - 8, 4);
        }

        // ════════════════════════════════════════════════════════════════
        // Input
        // ════════════════════════════════════════════════════════════════

        private void OnKeyDown(object? s, KeyEventArgs e)
        {
            if (_started && !_session.IsRaceComplete)
            {
                if (e.KeyCode == Keys.Space)
                {
                    _session.Boost();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.M)
                {
                    bool ok = _session.ToggleMaintainSpeed();
                    _localHudMessage = ok
                        ? (_session.Human.MaintainSpeedEnabled ? "Cruise ON" : "Cruise OFF")
                        : "Cruise unavailable (need speed + fuel)";
                    _localHudMessageClearAtTick = _uiTickCount + 12;
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.F)
                {
                    bool ok = _session.TryRefuel();
                    _localHudMessage = ok ? "⛽ Fuel refilled" : "⛽ Can't refuel here";
                    _localHudMessageClearAtTick = _uiTickCount + 12;
                    e.SuppressKeyPress = true;
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        // Results
        // ════════════════════════════════════════════════════════════════

        private void ShowResults()
        {
            // Save to leaderboard
            var result = new Models.RaceResult(
                _session.Human.Name,
                _session.Track.Name,
                _session.Human.TotalRaceTime(),
                _session.Track.DefaultLaps);
            _leaderboard.Record(result);

            var resForm = new ResultsForm(_session.FinalResults, _leaderboard);
            resForm.ShowDialog(this);
            Close();
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static Label MkLbl(string t, Font f, Color c) =>
            new Label { Text = t, Font = f, ForeColor = c, AutoSize = true, BackColor = Color.Transparent };

        private static Panel Center(Control c)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            c.Anchor = AnchorStyles.None;
            p.Controls.Add(c);
            p.Layout += (_, _) =>
            {
                c.Left = (p.Width  - c.Width)  / 2;
                c.Top  = (p.Height - c.Height) / 2;
            };
            return p;
        }
    }
}
