namespace RacingGame.Enums
{
    /// <summary>Current state of the race.</summary>
    public enum RaceStatus { NotStarted, Countdown, InProgress, Finished, Cancelled }

    /// <summary>Track difficulty levels.</summary>
    public enum TrackDifficulty { Easy, Medium, Hard, Extreme }

    /// <summary>Live weather conditions affecting the race.</summary>
    public enum WeatherCondition { Sunny, Cloudy, Rain, Storm, Fog }

    /// <summary>Car performance tier.</summary>
    public enum CarTier { Budget, Standard, Sport, SuperCar }

    /// <summary>Type of in-race event triggered.</summary>
    public enum EventType { None, FlatTyre, TailWind, Obstacle, PitStop, NitroBoost, FogPatch }

    /// <summary>AI difficulty level.</summary>
    public enum AIDifficulty { Rookie, Amateur, Pro, Elite }
}
