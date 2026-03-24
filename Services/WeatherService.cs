using System;
using RacingGame.Core.Constants;
using RacingGame.Enums;

namespace RacingGame.Services
{
    /// <summary>
    /// Manages dynamic weather that changes during a race.
    /// Weather is stored in a Queue — conditions are dequeued as they expire.
    /// </summary>
    public class WeatherService
    {
        private readonly Random _rng = new();
        private System.Collections.Generic.Queue<WeatherCondition> _forecast;

        public WeatherCondition Current { get; private set; }
        public int TicksUntilChange     { get; private set; }
        private int _changeCooldown = 25;   // ticks between weather changes

        public WeatherService()
        {
            _forecast = new();
            Current   = WeatherCondition.Sunny;
            BuildForecast();
        }

        private void BuildForecast()
        {
            WeatherCondition[] pool =
            {
                WeatherCondition.Sunny, WeatherCondition.Sunny,
                WeatherCondition.Cloudy,
                WeatherCondition.Rain,
                WeatherCondition.Storm,
                WeatherCondition.Fog
            };

            for (int i = 0; i < 8; i++)
                _forecast.Enqueue(pool[_rng.Next(pool.Length)]);
        }

        /// <summary>Call once per game tick to potentially update weather.</summary>
        public bool Tick()
        {
            TicksUntilChange--;
            if (TicksUntilChange > 0) return false;

            // Dequeue next condition
            if (_forecast.Count == 0) BuildForecast();
            var previous = Current;
            Current = _forecast.Dequeue();
            TicksUntilChange = _changeCooldown + _rng.Next(-5, 10);
            return Current != previous;   // true = changed
        }

        /// <summary>Returns the speed multiplier for the current weather.</summary>
        public double GetMultiplier() => Current switch
        {
            WeatherCondition.Sunny  => GameConstants.SunnyMult,
            WeatherCondition.Cloudy => GameConstants.CloudyMult,
            WeatherCondition.Rain   => GameConstants.RainMult,
            WeatherCondition.Storm  => GameConstants.StormMult,
            WeatherCondition.Fog    => GameConstants.FogMult,
            _                       => 1.0
        };

        public string GetEmoji() => Current switch
        {
            WeatherCondition.Sunny  => "☀️ ",
            WeatherCondition.Cloudy => "⛅ ",
            WeatherCondition.Rain   => "🌧️ ",
            WeatherCondition.Storm  => "⛈️ ",
            WeatherCondition.Fog    => "🌫️ ",
            _                       => "  "
        };
    }
}
