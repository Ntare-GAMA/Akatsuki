using System.Collections.Generic;
using RacingGame.Enums;
using RacingGame.Models;

namespace RacingGame.Models
{
    /// <summary>
    /// Represents a race track with waypoints (Vector2D), difficulty, and laps.
    /// Demonstrates: struct usage, List of structs, linear algebra integration.
    /// </summary>
    public class Track
    {
        public string          Name        { get; }
        public double          LengthKm    { get; }
        public TrackDifficulty Difficulty  { get; }
        public int             DefaultLaps { get; }
        public string          Description { get; }

        /// <summary>Ordered waypoints forming the track layout (2-D coordinates).</summary>
        public List<Vector2D>  Waypoints   { get; }

        public Track(string name, double lengthKm, TrackDifficulty difficulty,
                     int laps, string description, List<Vector2D> waypoints)
        {
            Name        = name;
            LengthKm    = lengthKm;
            Difficulty  = difficulty;
            DefaultLaps = laps;
            Description = description;
            Waypoints   = waypoints;
        }

        /// <summary>Total track distance calculated from waypoints (Euclidean sum).</summary>
        public double WaypointDistance()
        {
            double total = 0;
            for (int i = 1; i < Waypoints.Count; i++)
                total += Waypoints[i - 1].DistanceTo(Waypoints[i]);
            return total;
        }

        /// <summary>Speed modifier based on difficulty.</summary>
        public double DifficultyMultiplier() => Difficulty switch
        {
            TrackDifficulty.Easy    => 1.10,
            TrackDifficulty.Medium  => 1.00,
            TrackDifficulty.Hard    => 0.85,
            TrackDifficulty.Extreme => 0.70,
            _                       => 1.00
        };

        /// <summary>Factory: all built-in tracks.</summary>
        public static List<Track> GetAllTracks() => new()
        {
            new Track("Kigali Circuit", 2.5, TrackDifficulty.Easy, 5,
                "A smooth city loop — perfect for beginners.",
                new() { new(0,0), new(2,0), new(2,2), new(0,2), new(0,0) }),

            new Track("Nyungwe Forest Run", 5.0, TrackDifficulty.Medium, 5,
                "Winding forest roads with tight corners.",
                new() { new(0,0), new(3,1), new(5,3), new(4,5), new(1,4), new(0,0) }),

            new Track("Volcano Peak Sprint", 8.0, TrackDifficulty.Hard, 5,
                "High altitude — thin air, sharp drops, no mercy.",
                new() { new(0,0), new(4,2), new(7,5), new(6,8), new(2,7), new(0,0) }),

            new Track("Hell's Gate Extreme", 12.0, TrackDifficulty.Extreme, 5,
                "One lap. One chance. Legends only.",
                new() { new(0,0), new(5,3), new(10,1), new(12,6), new(8,10), new(2,9), new(0,0) }),
        };

        public override string ToString() =>
            $"[{Difficulty}] {Name} | {LengthKm}km | {DefaultLaps} laps — {Description}";
    }
}
