using System;

namespace RacingGame.Exceptions
{
    /// <summary>Thrown when racer registration fails validation.</summary>
    public class InvalidRacerException : Exception
    {
        public string RacerName { get; }
        public InvalidRacerException(string name, string msg) : base(msg) => RacerName = name;
        public override string ToString() => $"[InvalidRacer] '{RacerName}': {Message}";
    }

    /// <summary>Thrown when the race is started in an illegal state.</summary>
    public class RaceStateException : Exception
    {
        public RaceStateException(string msg) : base(msg) { }
    }

    /// <summary>Thrown when a track cannot be found or selected.</summary>
    public class TrackNotFoundException : Exception
    {
        public TrackNotFoundException(string msg) : base(msg) { }
    }

    /// <summary>Thrown when the leaderboard file is corrupt or unreadable.</summary>
    public class LeaderboardIOException : Exception
    {
        public LeaderboardIOException(string msg, Exception inner) : base(msg, inner) { }
    }
}
