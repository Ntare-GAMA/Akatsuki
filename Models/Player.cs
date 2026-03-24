using System.Collections.Generic;

namespace RacingGame.Models
{
    /// <summary>
    /// Abstract base for every participant in the game.
    /// Demonstrates: abstract class, access modifiers, encapsulation.
    /// </summary>
    public abstract class Player
    {
        public string Name        { get; protected set; }
        public int    Age         { get; protected set; }
        public Car    SelectedCar { get; protected set; }

        protected List<RaceResult> _history = new();

        protected Player(string name, int age, Car car)
        {
            Name        = name;
            Age         = age;
            SelectedCar = car;
        }

        /// <summary>Each subclass must describe itself.</summary>
        public abstract string GetPlayerInfo();

        /// <summary>Advance the racer by one game tick.</summary>
        public abstract void Advance(double weatherMultiplier);

        /// <summary>Record a completed lap time.</summary>
        public abstract void RecordLapTime(double seconds);

        /// <summary>Return this racer's best result.</summary>
        public abstract RaceResult GetBestResult();

        /// <summary>Return cumulative race time across all laps.</summary>
        public abstract double TotalRaceTime();

        /// <summary>Total races entered.</summary>
        public int TotalRaces => _history.Count;

        /// <summary>All past results (read-only view).</summary>
        public IReadOnlyList<RaceResult> History => _history.AsReadOnly();

        public override string ToString() => GetPlayerInfo();
    }
}
