using System;
using System.Collections.Generic;
using RacingGame.Core.Constants;
using RacingGame.Core.Interfaces;
using RacingGame.Enums;

namespace RacingGame.Models
{
    /// <summary>
    /// Human-controlled racer. Progress advances when the player
    /// presses SPACEBAR — each press triggers a speed boost.
    /// Inherits from Player and implements IRaceable.
    /// </summary>
    public class HumanRacer : Player, IRaceable, IDisplayable
    {
        // ── IRaceable ───────────────────────────────────────────────────
        public double Progress    { get; private set; }
        public double Speed       { get; private set; }
        public bool   HasFinished { get; private set; }

        // ── Extra human-specific state ──────────────────────────────────
        public int  BoostsLeft    { get; private set; }
        public int  CurrentLap    { get; private set; }
        public int  TotalLaps     { get; private set; }
        public double RaceTime    { get; private set; }  // cumulative seconds

        // ── New gameplay state: cruise control + refuelling ─────────────
        public bool   MaintainSpeedEnabled { get; private set; }
        public double MaintainSpeedTarget  { get; private set; }
        public bool   CanRefuelNow         { get; private set; }

        private List<double> _lapTimes = new();

        private double _maintainTargetSpeed = 0;
        private bool   _refuelUsedThisLap   = false;

        private static readonly double[] _refuelStations =
        {
            0,
            GameConstants.TrackLength / 3.0,
            2 * GameConstants.TrackLength / 3.0
        };

        public HumanRacer(string name, int age, Car car, int totalLaps)
            : base(name, age, car)
        {
            TotalLaps  = totalLaps;
            BoostsLeft = GameConstants.MaxBoostsPerLap;
            CurrentLap = 1;

            MaintainSpeedEnabled = false;
            MaintainSpeedTarget  = 0;
            CanRefuelNow         = false;
        }

        /// <summary>Called every game tick by the RaceEngine.</summary>
        public override void Advance(double weatherMultiplier)
        {
            double speedCap = 8.0 * SelectedCar.Stats.TopSpeed;

            // ── Maintain speed / cruise control ─────────────────────────
            if (MaintainSpeedEnabled)
            {
                if (SelectedCar.Fuel <= 0)
                {
                    MaintainSpeedEnabled = false;
                    MaintainSpeedTarget  = 0;
                }
                else
                {
                    // Consume fuel over time while the player maintains speed.
                    SelectedCar.UseFuel(GameConstants.MaintainSpeedFuelPerTick);

                    if (SelectedCar.Fuel <= 0)
                    {
                        MaintainSpeedEnabled = false;
                        MaintainSpeedTarget  = 0;
                    }
                    else
                    {
                        _maintainTargetSpeed = Math.Min(speedCap, Math.Max(0, _maintainTargetSpeed));
                        MaintainSpeedTarget  = _maintainTargetSpeed;
                        Speed                 = MaintainSpeedTarget;
                    }
                }
            }

            // Natural deceleration each tick (friction / coasting)
            if (!MaintainSpeedEnabled)
            {
                Speed = Math.Max(0, Speed - 0.15);
            }

            // Calculate distance for this tick
            double distance = Speed * SelectedCar.Stats.TopSpeed * weatherMultiplier;
            Progress += distance;
            SelectedCar.AddMileage(distance);

            // Clamp to track end
            if (Progress >= GameConstants.TrackLength)
            {
                Progress = GameConstants.TrackLength;
                HasFinished = true;
                MaintainSpeedEnabled = false;
                MaintainSpeedTarget  = 0;
            }

            UpdateRefuelAvailability();
        }

        /// <summary>Player presses SPACEBAR → boost applied.</summary>
        public void ApplyBoost()
        {
            if (BoostsLeft <= 0 || HasFinished) return;

            Speed      += GameConstants.HumanBoostPower * SelectedCar.Stats.Acceleration;
            BoostsLeft -= 1;
            SelectedCar.UseFuel(2.5); // Example: each boost uses 2.5 units of fuel

            // Cap speed
            Speed = Math.Min(Speed, 8.0 * SelectedCar.Stats.TopSpeed);

            // If cruise control is active, keep the new speed as the target.
            if (MaintainSpeedEnabled)
            {
                _maintainTargetSpeed = Speed;
                MaintainSpeedTarget  = Speed;
            }
        }

        /// <summary>
        /// Toggle cruise control: keeps the current speed (consuming fuel over time).
        /// </summary>
        public bool ToggleMaintainSpeed()
        {
            if (HasFinished) return false;

            if (!MaintainSpeedEnabled)
            {
                if (SelectedCar.Fuel <= 0) return false;
                if (Speed < GameConstants.MaintainSpeedEnableMinSpeed) return false;

                MaintainSpeedEnabled = true;
                _maintainTargetSpeed = Speed;
                MaintainSpeedTarget  = Speed;
                return true;
            }

            MaintainSpeedEnabled = false;
            _maintainTargetSpeed = 0;
            MaintainSpeedTarget  = 0;
            return true;
        }

        /// <summary>
        /// Refuel to full when the player is within range of a pit-stop station.
        /// One refuel per lap.
        /// </summary>
        public bool TryRefuel()
        {
            if (HasFinished) return false;
            if (!CanRefuelNow) return false;
            if (SelectedCar.Fuel >= SelectedCar.MaxFuel - 0.001) return false;

            SelectedCar.RefuelToFull();
            _refuelUsedThisLap = true;
            CanRefuelNow = false;
            return true;
        }

        /// <summary>Record a lap time and reset for next lap.</summary>
        public override void RecordLapTime(double seconds)
        {
            _lapTimes.Add(seconds);
            RaceTime  += seconds;
            _history.Add(new RaceResult(Name, "N/A", seconds, CurrentLap));
            Progress   = 0;
            HasFinished = false;
            BoostsLeft = GameConstants.MaxBoostsPerLap;
            CurrentLap++;

            MaintainSpeedEnabled = false;
            MaintainSpeedTarget  = 0;
            _maintainTargetSpeed = 0;

            _refuelUsedThisLap = false;
            CanRefuelNow        = false;
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
            $"{SelectedCar.Symbol} {Name,-14} | Car: {SelectedCar.Name,-18} | Races: {TotalRaces}";

        public override string GetPlayerInfo() => ToDisplayString();

        private void UpdateRefuelAvailability()
        {
            if (HasFinished || _refuelUsedThisLap) { CanRefuelNow = false; return; }
            if (SelectedCar.Fuel >= SelectedCar.MaxFuel - 0.001) { CanRefuelNow = false; return; }

            foreach (var station in _refuelStations)
            {
                if (System.Math.Abs(Progress - station) <= GameConstants.RefuelStationWindow)
                {
                    CanRefuelNow = true;
                    return;
                }
            }

            CanRefuelNow = false;
        }
    }
}
