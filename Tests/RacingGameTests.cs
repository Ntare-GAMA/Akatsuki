using NUnit.Framework;
using System.Collections.Generic;
using RacingGame.Models;
using RacingGame.Services;
using RacingGame.Exceptions;
using RacingGame.Enums;
using RacingGame.Core.Constants;

namespace RacingGame.Tests
{
    /// <summary>
    /// Full NUnit test suite — 30 tests.
    /// Uses the modern Assert.That constraint model throughout.
    /// Follows Arrange → Act → Assert methodology.
    /// </summary>
    [TestFixture]
    public class RacingGameTests
    {
        // ════════════════════════════════════════════════════
        // Vector2D  (Linear Algebra)
        // ════════════════════════════════════════════════════

        [Test]
        public void Vector2D_Addition_ReturnsCorrectResult()
        {
            var a = new Vector2D(1, 2);
            var b = new Vector2D(3, 4);

            var result = a + b;

            Assert.That(result.X, Is.EqualTo(4));
            Assert.That(result.Y, Is.EqualTo(6));
        }

        [Test]
        public void Vector2D_Magnitude_IsCorrect()
        {
            var v = new Vector2D(3, 4);

            Assert.That(v.Magnitude, Is.EqualTo(5.0).Within(0.001));
        }

        [Test]
        public void Vector2D_DotProduct_PerpendicularVectors_IsZero()
        {
            var a = new Vector2D(1, 0);
            var b = new Vector2D(0, 1);

            double dot = a.Dot(b);

            Assert.That(dot, Is.EqualTo(0.0));
        }

        [Test]
        public void Vector2D_CrossProduct_IsCorrect()
        {
            var a = new Vector2D(2, 0);
            var b = new Vector2D(0, 3);

            double cross = a.Cross(b);

            Assert.That(cross, Is.EqualTo(6.0));
        }

        [Test]
        public void Vector2D_Normalise_ReturnsUnitVector()
        {
            var v    = new Vector2D(3, 4);
            var unit = v.Normalise();

            Assert.That(unit.Magnitude, Is.EqualTo(1.0).Within(0.001));
        }

        [Test]
        public void Vector2D_DistanceTo_IsCorrect()
        {
            var a = new Vector2D(0, 0);
            var b = new Vector2D(3, 4);

            Assert.That(a.DistanceTo(b), Is.EqualTo(5.0).Within(0.001));
        }

        // ════════════════════════════════════════════════════
        // Car
        // ════════════════════════════════════════════════════

        [Test]
        public void Car_GetAllCars_ReturnsFourCars()
        {
            var cars = Car.GetAllCars();

            Assert.That(cars.Length, Is.EqualTo(4));
        }

        [Test]
        public void Car_SuperCar_HasHighestTopSpeed()
        {
            var cars = Car.GetAllCars();
            double maxSpeed = 0;
            foreach (var c in cars)
                if (c.Stats.TopSpeed > maxSpeed) maxSpeed = c.Stats.TopSpeed;

            Assert.That(maxSpeed, Is.EqualTo(1.6).Within(0.001));
        }

        // ════════════════════════════════════════════════════
        // Track
        // ════════════════════════════════════════════════════

        [Test]
        public void Track_GetAllTracks_ReturnsFourTracks()
        {
            var tracks = Track.GetAllTracks();

            Assert.That(tracks.Count, Is.EqualTo(4));
        }

        [Test]
        public void Track_EasyDifficultyMultiplier_IsAboveOne()
        {
            var track = new Track("T", 2.5, TrackDifficulty.Easy, 3, "test", new List<Vector2D>());

            Assert.That(track.DifficultyMultiplier(), Is.GreaterThan(1.0));
        }

        [Test]
        public void Track_ExtremeDifficultyMultiplier_IsBelowOne()
        {
            var track = new Track("T", 10, TrackDifficulty.Extreme, 1, "test", new List<Vector2D>());

            Assert.That(track.DifficultyMultiplier(), Is.LessThan(1.0));
        }

        [Test]
        public void Track_WaypointDistance_IsPositive()
        {
            var tracks = Track.GetAllTracks();

            foreach (var t in tracks)
                Assert.That(t.WaypointDistance(), Is.GreaterThan(0), $"{t.Name} waypoint distance should be > 0.");
        }

        // ════════════════════════════════════════════════════
        // HumanRacer
        // ════════════════════════════════════════════════════

        [Test]
        public void HumanRacer_RecordLapTime_AccumulatesTotal()
        {
            var car   = Car.GetAllCars()[0];
            var racer = new HumanRacer("Alice", 25, car, 3);

            racer.RecordLapTime(40.0);
            racer.RecordLapTime(38.0);

            Assert.That(racer.TotalRaceTime(), Is.EqualTo(78.0).Within(0.01));
        }

        [Test]
        public void HumanRacer_BoostsLeft_DecrementsOnBoost()
        {
            var car    = Car.GetAllCars()[1];
            var racer  = new HumanRacer("Bob", 22, car, 3);
            int before = racer.BoostsLeft;

            racer.ApplyBoost();

            Assert.That(racer.BoostsLeft, Is.EqualTo(before - 1));
        }

        [Test]
        public void HumanRacer_BoostsLeft_NeverGoesBelowZero()
        {
            var car   = Car.GetAllCars()[0];
            var racer = new HumanRacer("Carol", 30, car, 3);

            for (int i = 0; i < GameConstants.MaxBoostsPerLap + 10; i++)
                racer.ApplyBoost();

            Assert.That(racer.BoostsLeft, Is.EqualTo(0));
        }

        [Test]
        public void HumanRacer_Speed_IncreasesOnBoost()
        {
            var car    = Car.GetAllCars()[2];
            var racer  = new HumanRacer("Dave", 28, car, 3);
            double before = racer.Speed;

            racer.ApplyBoost();

            Assert.That(racer.Speed, Is.GreaterThan(before));
        }

        // ════════════════════════════════════════════════════
        // AIRacer
        // ════════════════════════════════════════════════════

        [Test]
        public void AIRacer_Advance_IncreasesProgress()
        {
            var car = Car.GetAllCars()[1];
            var ai  = new AIRacer("Bot", car, AIDifficulty.Amateur, 3);

            ai.Advance(1.0);

            Assert.That(ai.Progress, Is.GreaterThan(0));
        }

        [Test]
        public void AIRacer_Elite_CoversMoreGroundThanRookieOnAverage()
        {
            var car    = Car.GetAllCars()[1];
            var rookie = new AIRacer("Rookie", car, AIDifficulty.Rookie, 1);
            var elite  = new AIRacer("Elite",  car, AIDifficulty.Elite,  1);

            double rookieTotal = 0, eliteTotal = 0;
            for (int i = 0; i < 100; i++)
            {
                rookie.Advance(1.0);
                elite.Advance(1.0);
                rookieTotal += rookie.Progress;
                eliteTotal  += elite.Progress;
            }

            Assert.That(eliteTotal, Is.GreaterThan(rookieTotal));
        }

        // ════════════════════════════════════════════════════
        // Exceptions
        // ════════════════════════════════════════════════════

        [Test]
        public void InvalidRacerException_StoresRacerName()
        {
            var ex = new InvalidRacerException("TestRacer", "Some error");

            Assert.That(ex.RacerName, Is.EqualTo("TestRacer"));
        }

        [Test]
        public void RaceStateException_StoresMessage()
        {
            var ex = new RaceStateException("No racers.");

            Assert.That(ex.Message, Is.EqualTo("No racers."));
        }

        // ════════════════════════════════════════════════════
        // RaceResult
        // ════════════════════════════════════════════════════

        [Test]
        public void RaceResult_Serialize_Deserialize_RoundTrip()
        {
            var original = new RaceResult("Grace", "Kigali Circuit", 45.67, 3);
            string line  = original.Serialize();

            var restored = RaceResult.Deserialize(line);

            Assert.That(restored,                  Is.Not.Null);
            Assert.That(restored!.RacerName,       Is.EqualTo(original.RacerName));
            Assert.That(restored.TrackName,        Is.EqualTo(original.TrackName));
            Assert.That(restored.TimeSeconds,      Is.EqualTo(original.TimeSeconds).Within(0.01));
        }

        [Test]
        public void RaceResult_CompareTo_SortsFastestFirst()
        {
            var slow = new RaceResult("Slow", "T", 60.0, 3);
            var fast = new RaceResult("Fast", "T", 40.0, 3);

            // fast.CompareTo(slow) should be negative (fast comes first)
            Assert.That(fast.CompareTo(slow), Is.LessThan(0));
        }

        // ════════════════════════════════════════════════════
        // Generic Leaderboard
        // ════════════════════════════════════════════════════

        [Test]
        public void Leaderboard_Upsert_KeepsBestTime()
        {
            var board = new Leaderboard<RaceResult>();
            board.Upsert("Hank", new RaceResult("Hank", "T", 55.0, 3));
            board.Upsert("Hank", new RaceResult("Hank", "T", 38.0, 3));

            Assert.That(board.GetTop()!.TimeSeconds, Is.EqualTo(38.0).Within(0.01));
        }

        [Test]
        public void Leaderboard_GetTop_ReturnsNullWhenEmpty()
        {
            var board = new Leaderboard<RaceResult>();

            Assert.That(board.GetTop(), Is.Null);
        }

        [Test]
        public void Leaderboard_Count_TracksUniqueEntries()
        {
            var board = new Leaderboard<RaceResult>();
            board.Upsert("A", new RaceResult("A", "T", 30.0, 3));
            board.Upsert("B", new RaceResult("B", "T", 40.0, 3));

            Assert.That(board.Count, Is.EqualTo(2));
        }

        // ════════════════════════════════════════════════════
        // WeatherService
        // ════════════════════════════════════════════════════

        [Test]
        public void WeatherService_Multiplier_IsAlwaysPositive()
        {
            var ws = new WeatherService();

            Assert.That(ws.GetMultiplier(), Is.GreaterThan(0));
        }

        [Test]
        public void WeatherService_Emoji_IsNotEmpty()
        {
            var ws = new WeatherService();

            Assert.That(ws.GetEmoji().Trim(), Is.Not.Empty);
        }

        // ════════════════════════════════════════════════════
        // TimerService
        // ════════════════════════════════════════════════════

        [Test]
        public void TimerService_Elapsed_GreaterThanZeroAfterStart()
        {
            var timer = new TimerService();
            timer.Start();
            System.Threading.Thread.Sleep(50);
            double elapsed = timer.Stop();

            Assert.That(elapsed, Is.GreaterThan(0));
        }

        [Test]
        public void TimerService_Stop_ReturnsNonNegativeValue()
        {
            var timer = new TimerService();
            timer.Start();
            double t = timer.Stop();

            Assert.That(t, Is.GreaterThanOrEqualTo(0));
        }
    }
}
