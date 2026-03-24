# 🏎️ Time-Based Racing Game
### C# Console Simulation — ALU Project 3 (v2.0)

---

## 📋 Overview
A fully interactive, real-time console-based racing simulation built in C#.
The player accelerates their car by pressing **SPACEBAR** and races against AI opponents
over multiple laps, with dynamic weather, random events, a live HUD, and a persistent leaderboard.

---

## 🗂️ Project Structure

```
RacingGame/
│
├── Program.cs                        ← Entry point
│
├── Core/
│   ├── Constants/
│   │   └── GameConstants.cs          ← All tunable game values
│   └── Interfaces/
│       └── IContracts.cs             ← IRaceable, IDisplayable, IPersistable
│
├── Enums/
│   └── GameEnums.cs                  ← RaceStatus, TrackDifficulty, Weather,
│                                        CarTier, EventType, AIDifficulty
│
├── Exceptions/
│   └── RacingExceptions.cs           ← InvalidRacerException, RaceStateException,
│                                        TrackNotFoundException, LeaderboardIOException
│
├── Models/
│   ├── Vector2D.cs                   ← 2-D struct: dot/cross/magnitude (Linear Algebra)
│   ├── Car.cs                        ← Car + CarStats struct, factory method
│   ├── Track.cs                      ← Track with waypoints, difficulty, factory method
│   ├── Player.cs                     ← Abstract base class
│   ├── HumanRacer.cs                 ← Inherits Player, implements IRaceable
│   ├── AIRacer.cs                    ← Inherits Player, implements IRaceable
│   └── RaceResult.cs                 ← Immutable result, serialise/deserialise
│
├── Services/
│   ├── TimerService.cs               ← Precision race timer + countdown
│   ├── WeatherService.cs             ← Dynamic weather via Queue
│   ├── Leaderboard.cs                ← Generic Leaderboard<T> + RaceLeaderboard
│   ├── FileService.cs                ← Persist / load results to disk
│   └── RaceEngine.cs                 ← Real-time race loop (SPACEBAR input)
│
├── UI/
│   ├── DisplayService.cs             ← All console rendering (no logic)
│   └── MenuHandler.cs                ← Menu flow & game orchestration
│
└── Tests/
    └── RacingGameTests.cs            ← 30 NUnit tests (AAA pattern)
```

---

## ⚙️ Setup & Run

### Prerequisites
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or higher

### Run the Game
```bash
git clone <your-repo-url>
cd RacingGame
dotnet run
```

### Run Tests
```bash
dotnet test
```

### Code Quality
```bash
dotnet format
dotnet build 2>&1 | grep -i warning
```

---

## 🎮 How to Play

1. **Register** — enter your name and age
2. **Choose a car** — 4 tiers from Budget to SuperCar
3. **Choose a track** — Easy → Extreme difficulty
4. **Race!** — Press **SPACEBAR** rapidly to accelerate
5. **Manage boosts** — you have limited fuel per lap, use wisely
6. **Survive events** — flat tyres, nitro boosts, fog patches
7. **Weather changes** in real-time and affects everyone's speed
8. **Finish all laps** — your total time is saved to the leaderboard

---

## 🧠 C# Concepts Demonstrated

| Concept                    | Where Applied |
|----------------------------|---------------|
| Abstract class / inheritance | `Player` → `HumanRacer`, `AIRacer` |
| Interfaces                 | `IRaceable`, `IDisplayable`, `IPersistable` |
| Structs                    | `CarStats`, `Vector2D` |
| Enumerations               | 6 enums in `GameEnums.cs` |
| Generics                   | `Leaderboard<T>` |
| Custom Exceptions          | 4 exception types |
| List / Dictionary          | Leaderboard, race history |
| Queue                      | Weather forecast |
| HashSet                    | Active race events |
| Stack / events             | Triggered in-race events |
| Linear Algebra             | `Vector2D`: dot, cross, magnitude, normalise |
| File I/O                   | `FileService` (persist leaderboard) |
| Operator overloading       | `Vector2D` +, -, * |
| Real-time input loop       | SPACEBAR-driven `RaceEngine` |
| XML comments               | All public members |
| NUnit (AAA pattern)        | 30 tests in `RacingGameTests.cs` |
| Constants class            | `GameConstants.cs` |

---

## 🧪 Tests Summary (30 tests)

| Group             | Tests |
|-------------------|-------|
| Vector2D (Algebra)| 6     |
| Car               | 2     |
| Track             | 4     |
| HumanRacer        | 4     |
| AIRacer           | 2     |
| Exceptions        | 2     |
| RaceResult        | 2     |
| Leaderboard       | 3     |
| WeatherService    | 2     |
| TimerService      | 2     |
| **Total**         | **30**|

---

## 👤 Author
**Group #__ — ALU BSE**
GitHub: `<your-repo-link>`
