using System;

namespace RacingGame.Models
{
    /// <summary>
    /// Immutable 2-D vector. Used for track waypoints and positional maths.
    /// Demonstrates: struct, operator overloading, dot/cross product, magnitude.
    /// </summary>
    public struct Vector2D
    {
        public double X { get; }
        public double Y { get; }

        public Vector2D(double x, double y) { X = x; Y = y; }

        // ── Basic arithmetic ────────────────────────────────────────────
        public static Vector2D operator +(Vector2D a, Vector2D b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector2D operator -(Vector2D a, Vector2D b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector2D operator *(Vector2D v, double s)   => new(v.X * s, v.Y * s);

        // ── Magnitude (length of the vector) ───────────────────────────
        public double Magnitude => Math.Sqrt(X * X + Y * Y);

        // ── Unit vector (normalised) ────────────────────────────────────
        public Vector2D Normalise()
        {
            double m = Magnitude;
            if (m == 0) return new Vector2D(0, 0);
            return new Vector2D(X / m, Y / m);
        }

        // ── Dot product: measures alignment between two vectors ─────────
        public double Dot(Vector2D other) => X * other.X + Y * other.Y;

        // ── 2-D cross product (scalar z-component) ──────────────────────
        public double Cross(Vector2D other) => X * other.Y - Y * other.X;

        // ── Distance to another point ───────────────────────────────────
        public double DistanceTo(Vector2D other) => (this - other).Magnitude;

        public override string ToString() => $"({X:F2}, {Y:F2})";
    }
}
