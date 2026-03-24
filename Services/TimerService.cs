using System;
using System.Threading;

namespace RacingGame.Services
{
    /// <summary>Precision timer for race timing and countdown display.</summary>
    public class TimerService
    {
        private DateTime _start;
        private DateTime _end;
        private bool     _running;

        public bool IsRunning => _running;

        /// <summary>Animated countdown displayed in the console.</summary>
        public void Countdown(int seconds)
        {
            Console.WriteLine();
            for (int i = seconds; i > 0; i--)
            {
                Console.ForegroundColor = i <= 1 ? ConsoleColor.Red
                                        : i == 2 ? ConsoleColor.Yellow
                                                 : ConsoleColor.Green;
                Console.WriteLine($"\n              {i}...");
                Console.ResetColor();
                Thread.Sleep(900);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n          🚦 GO GO GO!\n");
            Console.ResetColor();
            Thread.Sleep(200);
        }

        public void   Start()   { _start = DateTime.Now; _running = true; }
        public double Stop()    { _end = DateTime.Now; _running = false; return Elapsed; }
        public void   Reset()   { _running = false; }

        public double Elapsed
        {
            get
            {
                var end = _running ? DateTime.Now : _end;
                return Math.Round((end - _start).TotalSeconds, 2);
            }
        }

        public string ElapsedFormatted
        {
            get
            {
                var ts = TimeSpan.FromSeconds(Elapsed);
                return $"{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds / 10:D2}";
            }
        }
    }
}
