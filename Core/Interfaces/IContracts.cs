using RacingGame.Models;

namespace RacingGame.Core.Interfaces
{
    /// <summary>Contract that every racer type must fulfil.</summary>
    public interface IRaceable
    {
        string Name       { get; }
        double Progress   { get; }   // 0–TrackLength
        double Speed      { get; }
        bool   HasFinished { get; }

        void Advance(double weatherMultiplier);
        void RecordLapTime(double seconds);
        RaceResult GetBestResult();
    }

    /// <summary>Contract for anything that can be printed to the console.</summary>
    public interface IDisplayable
    {
        string ToDisplayString();
    }

    /// <summary>Contract for a persistable leaderboard.</summary>
    public interface IPersistable
    {
        void Save(string path);
        void Load(string path);
    }
}
