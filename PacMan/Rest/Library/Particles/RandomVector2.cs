using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class RandomVector2 : RandomVector3
    {
        public RandomVector2(float length)
            : this(length, 0) { }
        public RandomVector2(float avgLength, float lengthDev)
            : this(avgLength, lengthDev, GlobalRandom.Distribution.DoubleUniform) { }
        public RandomVector2(float avgLength, float lengthDev, GlobalRandom.Distribution d)
            : this(avgLength, lengthDev, d, 0, MathHelper.PiOver2, GlobalRandom.Distribution.Uniform) { }
        public RandomVector2(float avgLength, float lengthDev, GlobalRandom.Distribution lengthD, float avgAngle, float angleDev, GlobalRandom.Distribution angleD)
            : base(avgLength, lengthDev, lengthD, avgAngle, angleDev, angleD) { }

        public Vector2 Create2()
        {
            // Get the length
            float length = (float)GlobalRandom.Rand(lengthD, avgLength, lengthDev);

            // Get the angle
            float angle = (float)GlobalRandom.Rand(angleD, avgAngle, angleDev);

            // Get the vector
            return GameMath.Vector2FromRotation(angle, length);
        }
        public override Vector3 Create()
        {
            Vector2 v = this.Create2();
            return new Vector3(v);
        }

        /// <summary>
        /// Randomly create the zero vector
        /// </summary>
        public static RandomVector2 Zero2
        {
            get
            {
                return new RandomVector2(0);
            }
        }
    }
}
