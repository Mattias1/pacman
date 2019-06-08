using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public static class GameMath
    {
        // Float stuff
        public static float Clamp(float min, float max, float value)
        {
            if (value <= min)
                return min;
            if (value >= max)
                return max;
            return value;
        }

        public static float Lerp(float from, float to, float t)
        {
            return from + (to - from) * GameMath.Clamp(0, 1, t);
        }

        // Vector stuff
        public static Vector2 Vector2FromRotation(float angle, float radius = 1)
        {
            return radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
        public static Vector3 Vector3FromRotation(float angle, float angle2, float radius = 1)
        {
            return radius * new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), (float)Math.Sin(angle2));
        }

        public static float Angle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X); // In ]-pi,pi]
        }

        /// <summary>
        /// Round the values in the vector to integers
        /// </summary>
        /// <param name="mathematical">When true, rounded to the closest integer (0.5 ruonded up), when false, rounded down.</param>
        public static Vector2 Round(Vector2 vector, bool mathematical = true)
        {
            float add = mathematical ? 0.5f : 0f;
            return new Vector2((int)(vector.X + add), (int)(vector.Y + add));
        }

        public static bool OppositeDirections(Vector2 v, Vector2 w)
        {
            return v != Vector2.Zero && w != Vector2.Zero
                && ((v.X == w.X && v.Y > 0 != w.Y > 0) || (v.X > 0 != w.X > 0 && v.Y == w.Y));
        }

        // Color stuff
        public static Color Clamp(float min, float max, Color color, bool alpha = false)
        {
            return new Color(
                (byte)Clamp(min, max, color.R),
                (byte)Clamp(min, max, color.G),
                (byte)Clamp(min, max, color.B),
                alpha ? (byte)Clamp(min, max, color.A) : color.A
            );
        }

        public static Color Lerp(Color from, Color to, float t, bool alpha = false)
        {
            return new Color(
                (byte)Lerp(from.R, to.R, t),
                (byte)Lerp(from.G, to.G, t),
                (byte)Lerp(from.B, to.B, t),
                alpha ? (byte)Lerp(from.A, to.A, t) : from.A
            );
        }

        // Collision stuff
        /// <summary>
        /// Check if a point is inside a triangle
        /// </summary>
        /// <param name="point">The point to check on</param>
        /// <param name="triangleTop">The orientation point of the triangle</param>
        /// <param name="dir1">A point relative to the triangleTop</param>
        /// <param name="dir2">The other point relative to the triangle top</param>
        /// <returns>True when inside the triantle, false when outside</returns>
        public static bool PointInTriangle(Vector2 point, Vector2 triangleTop, Vector2 dir0, Vector2 dir1)
        {
            // Compute dot products
            float dot00 = Vector2.Dot(dir0, dir0);
            float dot01 = Vector2.Dot(dir0, dir1);
            float dot0p = Vector2.Dot(dir0, point - triangleTop);
            float dot11 = Vector2.Dot(dir1, dir1);
            float dot1p = Vector2.Dot(dir1, point - triangleTop);

            // Compute barycentric coordinates
            float div = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot0p - dot01 * dot1p) * div;
            float v = (dot00 * dot1p - dot01 * dot0p) * div;

            // Check if point is in triangle
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }
    }
}
