using System;
using System.Collections.Generic;
using RacingGame.Core.Constants;
using RacingGame.Core.Interfaces;
using RacingGame.Enums;

namespace RacingGame.Models
{
    /// <summary>
    /// AI-controlled opponent. Advances automatically each tick.
    /// Speed is influenced by AIDifficulty, Car stats, and randomness.
    /// Inherits from Player and implements IRaceable.
    /// </summary>
    public class AIRacer : Player, IRaceable, IDisplayable
    {
        // ── IRaceable ───────────────────────────────────────────────────
        public double Progress    { get; private set; }
        public double Speed       { get; private set; }
        public bool   HasFinished { get; private set; }

        public AIDifficulty Difficulty { get; }
        public int  CurrentLap        { get; private set; }
        public int  TotalLaps         { get; private set; }
        public double RaceTime        { get; private set; }

        private readonly Random _rng;
        private readonly double _diffMult;
        private List<double> _lapTimes = new();

        public AIRacer(string name, Car car, AIDifficulty difficulty, int totalLaps)
            : base(name, 0, car)
        {
            Difficulty = difficulty;
            TotalLaps  = totalLaps;
            CurrentLap = 1;
            _rng       = new Random(Guid.NewGuid().GetHashCode());

            _diffMult = difficulty switch
            {
                AIDifficulty.Rookie  => 0.70,
                AIDifficulty.Amateur => 0.90,
                AIDifficulty.Pro     => 1.10,
                AIDifficulty.Elite   => 1.30,
                _                    => 1.00
            };
        }

        /// <summary>Called every game tick — AI decides its own speed.</summary>
        public override void Advance(double weatherMultiplier)
        {
            // Random variance each tick: -0.3 to +0.3
            double variance = (_rng.NextDouble() * 0.6) - 0.3;

            double tick = GameConstants.AIBaseTick
                          * _diffMult
                          * SelectedCar.Stats.TopSpeed
                          * weatherMultiplier
                          + variance;

            Progress += Math.Max(0, tick);

            if (Progress >= GameConstants.TrackLength)
            {
                Progress    = GameConstants.TrackLength;
                HasFinished = true;
            }
        }

        public override void RecordLapTime(double seconds)
        {
            _lapTimes.Add(seconds);
            RaceTime   += seconds;
            Progress    = 0;
            HasFinished = false;
            CurrentLap++;
        }

        public override RaceResult GetBestResult()
        {
            double best = double.MaxValue;
            foreach (var t in _lapTimes) if (t < best) best = t;
            return new RaceResult(Name, "N/A", best == double.MaxValue ? 0 : best, TotalLaps);
        }

        public override double TotalRaceTime() => RaceTime;

        // ── IDisplayable ────────────────────────────────────────────────
        public string ToDisplayString() =>
            $"🤖 {Name,-14} | {Difficulty,-8} AI | Car: {SelectedCar.Name}";

        public override string GetPlayerInfo() => ToDisplayString();
    }
}
