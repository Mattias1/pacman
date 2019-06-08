using System;

namespace PacMan
{
    /// <summary>
    /// Static class to act as global random number generator
    /// Incudes methods for random integers and doubles.
    /// </summary>
    public static class GlobalRandom
    {
        private static Random random = new Random();

        public enum Distribution { Uniform, DoubleUniform, Normal };

        /// <summary>
        /// The instance of Random used by GlobalRandom
        /// </summary>
        public static Random Random { get { return random; } }

        /// <summary>
        /// Returns a random boolean
        /// </summary>
        /// <returns></returns>
        public static bool Boolean() {
            return Next(2) == 0;
        }
        /// <summary>
        /// Returns random integer in interger [0, upper bound[
        /// </summary>
        /// <param name="max">The exclusive upper bound</param>
        public static int Next(int max) {
            return random.Next(max);
        }
        /// <summary>
        /// Returns random integer in the interval [lower bound, upper bound[
        /// </summary>
        /// <param name="min">The inclusive lower bound</param>
        /// <param name="max">The exclusive upper bound</param>
        /// <returns></returns>
        public static int Next(int min, int max) {
            return random.Next(min, max);
        }
        /// <summary>
        /// Returns random double between 0 and 1 with uniform distribution.
        /// </summary>
        /// <returns></returns>
        public static double NextDouble() {
            return random.NextDouble();
        }
        /// <summary>
        /// Returns a random double between min and max with uniform distribution.
        /// </summary>
        /// <param name="min">The lower bound</param>
        /// <param name="max">The upper bound</param>
        /// <returns></returns>
        public static double NextDouble(double min, double max) {
            return NextDouble() * (max - min) + min;
        }
        /// <summary>
        /// Returns random double between 0 and 1 with double uniform distribution.
        /// </summary>
        /// <returns></returns>
        public static double DoubleUniformDouble() {
            return (NextDouble() + NextDouble()) * 0.5;
        }
        /// <summary>
        /// Returns a random double between min and max with double uniform distribution.
        /// </summary>
        /// <param name="min">The lower bound</param>
        /// <param name="max">The upper bound</param>
        /// <returns></returns>
        public static double DoubleUniformDouble(double min, double max) {
            return DoubleUniformDouble() * (max - min) + min;
        }
        /// <summary>
        /// Generates a random double using the standard normal distribution;
        /// </summary>
        /// <returns></returns>
        public static double NormalDouble() {
            // Box-Muller
            double u1 = NextDouble();
            double u2 = NextDouble();
            return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
        }
        /// <summary>
        /// Generates a random double using the normal distribution with the given mean and deviation.
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="deviation"></param>
        /// <returns></returns>
        public static double NormalDouble(double mean, double deviation) {
            return mean + deviation * NormalDouble();
        }

        /// <summary>
        /// Return a random double between 0 and 1 with a certain distribtion
        /// (with the normal distribution it returns a standard normal distributed double)
        /// </summary>
        /// <param name="distribution"></param>
        /// <returns></returns>
        public static double Rand(Distribution distribution) {
            switch (distribution) {
            case Distribution.DoubleUniform:
                return DoubleUniformDouble();
            case Distribution.Normal:
                return NormalDouble();
            default:
                return NextDouble();
            }
        }
        /// <summary>
        /// Return a random double depending on a distribution.
        /// Normal: a double with an average and a deviation
        /// (Double)Uniform: a double in [avg - 2 * dev, avg + 2 * dev]
        /// </summary>
        /// <param name="distribution"></param>
        /// <param name="avg"></param>
        /// <param name="dev"></param>
        /// <returns></returns>
        public static double Rand(Distribution distribution, double avg, double dev) {
            if (dev == 0)
                return avg;
            switch (distribution) {
            case Distribution.DoubleUniform:
                return DoubleUniformDouble(avg - 2 * dev, avg + 2 * dev);
            case Distribution.Normal:
                return NormalDouble(avg, dev);
            default:
                return NextDouble(avg - 2 * dev, avg + 2 * dev);
            }
        }
    }
}