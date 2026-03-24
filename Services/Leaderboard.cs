using System;
using System.Collections.Generic;
using RacingGame.Core.Constants;
using RacingGame.Core.Interfaces;
using RacingGame.Exceptions;
using RacingGame.Models;

namespace RacingGame.Services
{
    /// <summary>
    /// Generic leaderboard — can rank any IComparable.
    /// Internally uses a sorted List and a Dictionary for O(1) lookup.
    /// Implements IPersistable via FileService.
    /// </summary>
    public class Leaderboard<T> where T : IComparable<T>
    {
        private List<T>              _sorted   = new();
        private Dictionary<string, T> _byName  = new();

        public int Count => _sorted.Count;

        /// <summary>Insert or update an entry (keeps only best/lowest).</summary>
        public void Upsert(string key, T value)
        {
            if (_byName.TryGetValue(key, out T? existing))
            {
                if (value.CompareTo(existing) < 0)
                {
                    _sorted.Remove(existing);
                    _byName[key] = value;
                    InsertSorted(value);
                }
            }
            else
            {
                _byName[key] = value;
                InsertSorted(value);
            }
        }

        private void InsertSorted(T val)
        {
            int i = _sorted.BinarySearch(val);
            _sorted.Insert(i < 0 ? ~i : i, val);
        }

        public T? GetTop()    => _sorted.Count > 0 ? _sorted[0] : default;
        public List<T> GetAll() => new(_sorted);
    }

    /// <summary>
    /// Concrete leaderboard for RaceResults.
    /// Wraps the generic Leaderboard and adds file persistence.
    /// </summary>
    public class RaceLeaderboard : IPersistable
    {
        private Leaderboard<RaceResult> _board = new();
        private FileService             _file;

        public RaceLeaderboard(string filePath)
        {
            _file = new FileService(filePath);

            // Load persisted results on startup
            try   { foreach (var r in _file.LoadAll()) _board.Upsert(r.RacerName, r); }
            catch (LeaderboardIOException) { /* start fresh if file corrupt */ }
        }

        public void Record(RaceResult result)
        {
            _board.Upsert(result.RacerName, result);
            try   { _file.AppendResult(result); }
            catch (LeaderboardIOException) { /* non-fatal */ }
        }

        public RaceResult? GetTop() => _board.GetTop();
        public List<RaceResult> GetAll() => _board.GetAll();

        public void Save(string path) { /* handled per-record */ }

        public void Load(string path)
        {
            var svc = new FileService(path);
            foreach (var r in svc.LoadAll()) _board.Upsert(r.RacerName, r);
        }

        public void Display()
        {
            var all = _board.GetAll();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  ╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                   🏆  ALL-TIME LEADERBOARD               ║");
            Console.WriteLine("  ╠══════════════════════════════════════════════════════════╣");
            Console.ResetColor();

            if (all.Count == 0)
            {
                Console.WriteLine("  ║              No results recorded yet.                    ║");
            }
            else
            {
                for (int i = 0; i < Math.Min(all.Count, 10); i++)
                {
                    string medal = i == 0 ? "🥇" : i == 1 ? "🥈" : i == 2 ? "🥉" : $"#{i+1} ";
                    Console.ForegroundColor = i == 0 ? ConsoleColor.Yellow : ConsoleColor.White;
                    Console.WriteLine($"  ║  {medal}  {all[i]}  ║");
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }
    }
}
