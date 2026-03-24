namespace RacingGame.Core.Constants
{
    /// <summary>Centralised game constants. Change these to tune gameplay.</summary>
    public static class GameConstants
    {
        // Track
        public const int TrackLength        = 60;   // units (progress bar width)
        public const int DefaultLaps        = 3;

        // Racing
        public const int RaceLoopMs         = 120;  // ms per tick
        public const double HumanBoostPower = 2.5;  // progress per spacebar press
        public const int MaxBoostsPerLap    = 25;   // fuel per lap
        public const double AIBaseTick      = 0.6;  // base AI advance per tick

        // Fuel / cruising
        public const double MaxFuel                         = 100.0;
        public const double MaintainSpeedFuelPerTick      = 0.25; // per 120ms tick
        public const double MaintainSpeedEnableMinSpeed    = 0.25; // must already be moving
        public const double RefuelStationWindow            = 2.5;  // distance in track units

        // Weather multipliers
        public const double SunnyMult       = 1.10;
        public const double CloudyMult      = 1.00;
        public const double RainMult        = 0.80;
        public const double StormMult       = 0.55;
        public const double FogMult         = 0.70;

        // Leaderboard file
        public const string LeaderboardFile = "leaderboard.txt";

        // UI
        public const int ProgressBarWidth   = 40;
    }
}
