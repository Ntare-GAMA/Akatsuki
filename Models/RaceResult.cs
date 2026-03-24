using System;

namespace RacingGame.Models
{
    /// <summary>Immutable record of one completed race result.</summary>
    public class RaceResult : IComparable<RaceResult>
    {
        public string RacerName  { get; }
        public string TrackName  { get; }
        public double TimeSeconds { get; }
        public int    Laps       { get; }
        public int    Position   { get; set; }   // set after all racers finish
        public DateTime RacedOn  { get; }

        public RaceResult(string racerName, string trackName, double time, int laps)
        {
            RacerName   = racerName;
            TrackName   = trackName;
            TimeSeconds = time;
            Laps        = laps;
            RacedOn     = DateTime.Now;
        }

        /// <summary>Serialize to a single line for file storage.</summary>
        public string Serialize() =>
            $"{RacerName}|{TrackName}|{TimeSeconds:F2}|{Laps}|{RacedOn:yyyy-MM-dd HH:mm}";

        /// <summary>Deserialize from a saved line.</summary>
        public static RaceResult? Deserialize(string line)
        {
            try
            {
                var p = line.Split('|');
                return new RaceResult(p[0], p[1], double.Parse(p[2]), int.Parse(p[3]));
            }
            catch { return null; }
        }

        // Sort ascending by time (fastest first)
        public int CompareTo(RaceResult? other) =>
            other == null ? 1 : TimeSeconds.CompareTo(other.TimeSeconds);

        public override string ToString() =>
            $"{RacerName,-16} {TrackName,-22} {TimeSeconds,8:F2}s  {RacedOn:dd MMM yyyy}";
    }
}
