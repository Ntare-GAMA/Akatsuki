namespace RacingGame.Core.Constants
{
    /// <summary>Centralised game constants. Change these to tune gameplay.</summary>
    public static class GameConstants
    {
        // Track
        public const int TrackLength        = 60;   // units (progress bar width)
        public const int DefaultLaps        = 5;

        // Racing
        public const int RaceLoopMs         = 120;  // ms per tick
        public const double HumanBoostPower = 2.5;  // progress per spacebar press
        public const int MaxBoostsPerLap    = 25;   // fuel per lap
        public const double AIBaseTick      = 0.6;  // base AI advance per tick

        // Fuel / cruising
        public const double MaxFuel                         = 100.0;
        public const double MaintainSpeedFuelPerTick      = 0.25; // per 120ms tick while cruise is on
        public const double PassiveFuelDrainPerTick       = 0.08; // per 120ms tick just from driving
        public const double MaintainSpeedEnableMinSpeed    = 0.25; // must already be moving
        public const double RefuelStationWindow            = 8.0;  // distance in track units (~1.5 s window)
        public const double OutOfFuelDecelerationPerTick   = 0.60; // stronger slowdown with empty tank
        public const double BrakeDecelerationPerTick       = 1.20; // player brake for pit/refuel control
        public const double RefuelStoppedSpeedThreshold    = 0.20; // allow refuel when basically stopped
        public const double RaceTimeLimitSeconds           = 180.0; // assignment: race can end on time out
        public const double TurnSeconds                    = 2.0;   // each action consumes fixed race time
        public const int AutoTurnMs                        = 900;   // UI auto-plays turns for all racers

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
