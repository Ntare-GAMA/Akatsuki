using System;
using System.Collections.Generic;
using RacingGame.Core.Constants;
using RacingGame.Enums;
using RacingGame.Models;

namespace RacingGame.Services
{
    /// <summary>
    /// Tickable race session driven by the WinForms timer.
    /// Each call to Tick() advances all racers by one step.
    /// The Form polls state properties to update the UI.
    /// </summary>
    public class RaceSession
    {
        // ── Participants ────────────────────────────────────────────────
        public HumanRacer       Human    { get; }
        public List<AIRacer>    AIRacers { get; }
        public Track            Track    { get; }

        // ── Services ────────────────────────────────────────────────────
        public  WeatherService  Weather  { get; }
        private TimerService    _timer   = new();
        private Random          _rng     = new();

        // ── Lap / race state ────────────────────────────────────────────
        public  int     CurrentLap      { get; private set; } = 1;
        public  bool    IsRaceComplete  { get; private set; }
        public  bool    IsLapComplete   { get; private set; }
        public  string  LastEvent       { get; private set; } = string.Empty;
        private int     _eventClearTick = 0;
        private int     _tickCount      = 0;

        // ── Results ─────────────────────────────────────────────────────
        public List<(string Name, double Time, bool IsHuman)> FinalResults { get; } = new();

        public RaceSession(HumanRacer human, List<AIRacer> aiRacers, Track track)
        {
            Human    = human;
            AIRacers = aiRacers;
            Track    = track;
            Weather  = new WeatherService();
        }

        /// <summary>Start the lap timer.</summary>
        public void StartTimer() => _timer.Start();

        /// <summary>
        /// Called once per WinForms timer tick (~120 ms).
        /// Returns true if something visually changed.
        /// </summary>
        public bool Tick()
        {
            if (IsRaceComplete) return false;

            _tickCount++;
            IsLapComplete = false;

            // Weather
            Weather.Tick();

            double mult = Weather.GetMultiplier() * Track.DifficultyMultiplier();

            // Advance human
            Human.Advance(mult);

            // Advance AI
            foreach (var ai in AIRacers)
                ai.Advance(mult);

            // Random event (4% per tick)
            if (_rng.Next(100) < 4)
                TriggerEvent();

            // Clear event message after ~2 seconds (≈16 ticks @ 120ms)
            if (_tickCount > _eventClearTick + 16)
                LastEvent = string.Empty;

            // Check lap complete
            if (Human.HasFinished)
            {
                double lapTime = _timer.Stop();
                Human.RecordLapTime(lapTime);

                foreach (var ai in AIRacers)
                {
                    double aiTime = lapTime + (_rng.NextDouble() * 10 - 5);
                    ai.RecordLapTime(Math.Max(1, aiTime));
                }

                if (CurrentLap >= Track.DefaultLaps)
                {
                    BuildFinalResults();
                    IsRaceComplete = true;
                }
                else
                {
                    CurrentLap++;
                    _timer.Start();
                    IsLapComplete = true;
                }
            }

            return true;
        }

        /// <summary>Player pressed SPACEBAR.</summary>
        public void Boost() => Human.ApplyBoost();

        public bool ToggleMaintainSpeed() => Human.ToggleMaintainSpeed();

        public bool TryRefuel() => Human.TryRefuel();

        public double ElapsedSeconds => _timer.Elapsed;

        public string ElapsedFormatted
        {
            get
            {
                var ts = TimeSpan.FromSeconds(_timer.Elapsed);
                return $"{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds / 10:D2}";
            }
        }

        // ── Progress percentage helpers ─────────────────────────────────
        public int HumanProgressPct =>
            (int)Math.Min(100, Human.Progress / GameConstants.TrackLength * 100);

        public int AIProgressPct(int index) =>
            (int)Math.Min(100, AIRacers[index].Progress / GameConstants.TrackLength * 100);

        // ── Human position ──────────────────────────────────────────────
        public int HumanPosition()
        {
            int pos = 1;
            foreach (var ai in AIRacers)
                if (ai.TotalRaceTime() > 0 && ai.TotalRaceTime() < Human.TotalRaceTime()) pos++;
            return pos;
        }

        private void TriggerEvent()
        {
            var events = new (EventType Type, string Msg)[]
            {
                (EventType.FlatTyre,   "🔴 Flat tyre! Slow down!"),
                (EventType.TailWind,   "💨 Tail wind! Free boost!"),
                (EventType.NitroBoost, "🟢 Nitro! Burning fast!"),
                (EventType.Obstacle,   "🪨 Obstacle! Braking hard!"),
                (EventType.FogPatch,   "🌫 Fog patch! Vision low."),
            };

            var ev = events[_rng.Next(events.Length)];
            LastEvent = ev.Msg;
            _eventClearTick = _tickCount;

            switch (ev.Type)
            {
                case EventType.TailWind:
                    Human.ApplyBoost(); Human.ApplyBoost(); break;
                case EventType.NitroBoost:
                    Human.ApplyBoost(); Human.ApplyBoost(); Human.ApplyBoost(); break;
            }
        }

        private void BuildFinalResults()
        {
            FinalResults.Add((Human.Name, Human.TotalRaceTime(), true));
            foreach (var ai in AIRacers)
                FinalResults.Add((ai.Name, ai.TotalRaceTime(), false));
            FinalResults.Sort((a, b) => a.Time.CompareTo(b.Time));
        }
    }
}
