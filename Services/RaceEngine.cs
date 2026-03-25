using System;
using System.Collections.Generic;
using RacingGame.Core.Constants;
using RacingGame.Enums;
using RacingGame.Models;

namespace RacingGame.Services
{
    /// <summary>
    /// Pure-logic race engine — no Console calls.
    /// WinForms calls Tick() on every timer interval.
    /// </summary>
    public class RaceEngine
    {
        // ── State ────────────────────────────────────────────────────────
        public  HumanRacer       Human        { get; }
        public  List<AIRacer>    AIRacers     { get; }
        public  Track            Track        { get; }
        public  WeatherService   Weather      { get; }
        public  TimerService     Timer        { get; }
        public  RaceStatus       Status       { get; private set; }
        public  int              CurrentLap   { get; private set; } = 1;
        public  string           LastEvent    { get; private set; } = "";
        public  string           EndReason    { get; private set; } = "";
        public  int              TurnNumber   { get; private set; }
        public  double           SimElapsedSeconds { get; private set; }

        private readonly Random _rng = new();

        // ── Events (raised for the Form to react to) ─────────────────────
        public event Action<int, double>?                         LapCompleted;   // lap#, lapTime
        public event Action<List<(string,double,bool)>>?          RaceFinished;
        public event Action<string>?                              EventTriggered;

        private double _lapStartElapsed = 0;

        public RaceEngine(HumanRacer human, List<AIRacer> aiRacers, Track track)
        {
            Human    = human;
            AIRacers = aiRacers;
            Track    = track;
            Weather  = new WeatherService();
            Timer    = new TimerService();
            Status   = RaceStatus.NotStarted;
        }

        /// <summary>Start the race timer.</summary>
        public void Start()
        {
            Status = RaceStatus.InProgress;
            Timer.Start();
            _lapStartElapsed = 0;
            TurnNumber = 0;
            SimElapsedSeconds = 0;
        }

        /// <summary>
        /// Called every timer tick from WinForms.
        /// Returns true if the race is still running.
        /// </summary>
        public bool Tick()
        {
            return PerformTurn(TurnAction.MaintainSpeed);
        }

        /// <summary>
        /// Turn-based simulation step: one player action advances one race turn.
        /// </summary>
        public bool PerformTurn(TurnAction action)
        {
            if (Status != RaceStatus.InProgress) return false;

            TurnNumber++;
            SimElapsedSeconds += GameConstants.TurnSeconds;

            switch (action)
            {
                case TurnAction.SpeedUp:
                    Human.ApplyBoost();
                    break;
                case TurnAction.MaintainSpeed:
                    if (!Human.MaintainSpeedEnabled)
                        Human.ToggleMaintainSpeed();
                    break;
                case TurnAction.PitStop:
                    if (!Human.TryRefuel())
                        LastEvent = "⛽ Pit stop failed.";
                    else
                        LastEvent = "⛽ Pit stop complete.";
                    break;
            }

            Weather.Tick();
            double mult = Weather.GetMultiplier() * Track.DifficultyMultiplier();

            // Advance all
            Human.Advance(mult);
            foreach (var ai in AIRacers) ai.Advance(mult);

            if (SimElapsedSeconds >= GameConstants.RaceTimeLimitSeconds)
            {
                FinishRace(RaceStatus.Cancelled, "⏰ Time ran out!");
                return false;
            }

            if (Human.SelectedCar.Fuel <= 0)
            {
                FinishRace(RaceStatus.Cancelled, "⛽ You ran out of fuel!");
                return false;
            }

            // Random event (4% chance per tick)
            if (_rng.Next(100) < 4)
            {
                LastEvent = TriggerEvent();
                EventTriggered?.Invoke(LastEvent);
            }

            // Check if human finished this lap
            if (Human.HasFinished)
            {
                double lapTime = SimElapsedSeconds - _lapStartElapsed;
                _lapStartElapsed = SimElapsedSeconds;

                Human.RecordLapTime(lapTime);
                foreach (var ai in AIRacers)
                    ai.RecordLapTime(lapTime + (_rng.NextDouble() * 8 - 4));

                LapCompleted?.Invoke(CurrentLap, lapTime);
                CurrentLap++;

                if (CurrentLap > Track.DefaultLaps)
                {
                    FinishRace(RaceStatus.Finished, "🏁 Race complete!");
                    return false;
                }
            }

            return true;
        }

        /// <summary>Human player boost — called on SPACEBAR.</summary>
        public void ApplyBoost() => Human.ApplyBoost();

        /// <summary>Human player cruise control — called on M.</summary>
        public bool ToggleMaintainSpeed() => Human.ToggleMaintainSpeed();

        /// <summary>Human player refuel — called on F.</summary>
        public bool TryRefuel() => Human.TryRefuel();

        /// <summary>Human player brake — called on S.</summary>
        public void ApplyBrake() => Human.ApplyBrake();

        public string SimElapsedFormatted
        {
            get
            {
                var ts = TimeSpan.FromSeconds(SimElapsedSeconds);
                return $"{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds / 10:D2}";
            }
        }

        private string TriggerEvent()
        {
            string[] events = {
                "⚡ NITRO BOOST! Extra speed!",
                "🔴 FLAT TYRE! Losing speed!",
                "💨 TAIL WIND! Free push!",
                "🪨 OBSTACLE! Braking hard!",
                "🌫️  FOG PATCH! Visibility down!"
            };
            string ev = events[_rng.Next(events.Length)];

            if (ev.Contains("NITRO") || ev.Contains("TAIL"))
            {
                Human.ApplyBoost();
                Human.ApplyBoost();
            }

            return ev;
        }

        private List<(string Name, double Time, bool IsHuman)> BuildResults()
        {
            var list = new List<(string, double, bool)>();
            list.Add((Human.Name, Human.TotalRaceTime(), true));
            foreach (var ai in AIRacers)
                list.Add((ai.Name, ai.TotalRaceTime(), false));
            list.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            return list;
        }

        private void FinishRace(RaceStatus status, string reason)
        {
            Status = status;
            EndReason = reason;
            Timer.Stop();
            LastEvent = reason;
            EventTriggered?.Invoke(reason);
            var results = BuildResults();
            RaceFinished?.Invoke(results);
        }
    }
}
