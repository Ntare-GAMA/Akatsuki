using System;
using System.Collections.Generic;
using System.IO;
using RacingGame.Exceptions;
using RacingGame.Models;

namespace RacingGame.Services
{
    /// <summary>
    /// Persists leaderboard results to a plain-text file.
    /// Each line = one serialised RaceResult.
    /// </summary>
    public class FileService
    {
        private readonly string _filePath;

        public FileService(string filePath) => _filePath = filePath;

        /// <summary>Append a single result to the leaderboard file.</summary>
        public void AppendResult(RaceResult result)
        {
            try
            {
                File.AppendAllText(_filePath, result.Serialize() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                throw new LeaderboardIOException("Failed to save result.", ex);
            }
        }

        /// <summary>Load all results from the file. Returns empty list if file missing.</summary>
        public List<RaceResult> LoadAll()
        {
            var list = new List<RaceResult>();
            if (!File.Exists(_filePath)) return list;

            try
            {
                foreach (var line in File.ReadAllLines(_filePath))
                {
                    var r = RaceResult.Deserialize(line);
                    if (r != null) list.Add(r);
                }
            }
            catch (Exception ex)
            {
                throw new LeaderboardIOException("Failed to read leaderboard file.", ex);
            }

            return list;
        }

        /// <summary>Clear the leaderboard file.</summary>
        public void Clear()
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
        }
    }
}
