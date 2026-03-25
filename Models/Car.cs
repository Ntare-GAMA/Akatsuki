using System;
using System.Collections.Generic;
using RacingGame.Core.Constants;
using RacingGame.Enums;

namespace RacingGame.Models
{
    /// <summary>Immutable stats snapshot for a car. Demonstrates struct usage.</summary>
    public struct CarStats
    {
        public double TopSpeed;        // max speed multiplier (1.0 = baseline)
        public double Acceleration;    // how quickly it reaches top speed
        public double Handling;        // resistance to weather/events penalty
        public double Reliability;     // 0‒1 chance of surviving a random event

        public CarStats(double topSpeed, double accel, double handling, double reliability)
        {
            TopSpeed     = topSpeed;
            Acceleration = accel;
            Handling     = handling;
            Reliability  = reliability;
        }

        public override string ToString() =>
            $"Spd:{TopSpeed:F1} Acc:{Acceleration:F1} Hdl:{Handling:F1} Rel:{Reliability:F1}";
    }

    /// <summary>Represents a racing car that a player or AI can drive.</summary>
    public class Car
    {
        public string    Name    { get; }
        public CarTier   Tier    { get; }
        public CarStats  Stats   { get; }
        public string    Symbol  { get; }   // emoji shown on track

        public double MaxFuel { get; }
        public double FuelUseMultiplier { get; } // lower = more fuel efficient

        // Fuel and mileage
        public double Fuel { get; private set; }  // Start set in ctor
        public double Mileage { get; private set; } = 0.0; // Total distance covered

        public Car(string name, CarTier tier, CarStats stats, string symbol, double maxFuel, double fuelUseMultiplier)
        {
            Name   = name;
            Tier   = tier;
            Stats  = stats;
            Symbol = symbol;
            MaxFuel = maxFuel;
            FuelUseMultiplier = fuelUseMultiplier;
            Fuel    = maxFuel;
            Mileage = 0.0;
        }

        public double FuelUsed => MaxFuel - Fuel;

        // Call this when the car moves (advance)
        public void UseFuel(double amount)
        {
            Fuel = Math.Max(0, Fuel - amount);
        }

        // Call this when refuelling during a race.
        public void Refuel(double amount)
        {
            Fuel = Math.Min(MaxFuel, Fuel + amount);
        }

        // Fill tank to maximum capacity.
        public void RefuelToFull() => Fuel = MaxFuel;

        // Call this when the car covers distance
        public void AddMileage(double distance)
        {
            Mileage += distance;
        }

        /// <summary>Factory: returns all available cars the player can pick from.</summary>
        public static Car[] GetAllCars() => new[]
        {
            new Car("Kigali Cruiser",   CarTier.Budget,    new CarStats(0.8, 0.7, 0.9, 0.95), "🚗", 115, 0.80),
            new Car("Savanna Sprint",   CarTier.Standard,  new CarStats(1.0, 1.0, 1.0, 0.90), "🏎", 100, 1.00),
            new Car("Rwandan Rocket",   CarTier.Sport,     new CarStats(1.3, 1.2, 0.8, 0.85), "🚀", 90,  1.15),
            new Car("Volcano Viper",    CarTier.SuperCar,  new CarStats(1.6, 1.5, 0.7, 0.75), "🔥", 80,  1.30),
        };

        public override string ToString() => $"{Symbol} {Name} [{Tier}] | {Stats}";
    }
}
