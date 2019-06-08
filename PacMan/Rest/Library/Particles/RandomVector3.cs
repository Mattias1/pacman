using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class RandomVector3
    {
        protected float avgLength;
        protected float lengthDev;
        protected GlobalRandom.Distribution lengthD;
        protected float avgAngle;
        protected float angleDev;
        protected GlobalRandom.Distribution angleD;

        public float AverageLength
        {
            get { return this.avgLength; }
        }

        public RandomVector3(float length)
            : this(length, 0) { }
        public RandomVector3(float avgLength, float lengthDev)
            : this(avgLength, lengthDev, GlobalRandom.Distribution.DoubleUniform) { }
        public RandomVector3(float avgLength, float lengthDev, GlobalRandom.Distribution d)
            : this(avgLength, lengthDev, d, 0, MathHelper.PiOver2, GlobalRandom.Distribution.Uniform) { }
        public RandomVector3(float avgLength, float lengthDev, GlobalRandom.Distribution lengthD, float avgAngle, float angleDev, GlobalRandom.Distribution angleD)
        {
            this.avgLength = avgLength;
            this.lengthDev = lengthDev;
            this.lengthD = lengthD;
            this.avgAngle = avgAngle;
            this.angleDev = angleDev;
            this.angleD = angleD;
        }

        public virtual Vector3 Create()
        {
            // Get the length
            float length = (float)GlobalRandom.Rand(lengthD, avgLength, lengthDev);

            // Get the angle
            float angle = (float)GlobalRandom.Rand(angleD, avgAngle, angleDev);
            float angle2 = (float)GlobalRandom.Rand(angleD, avgAngle, angleDev);

            // Get the vector
            return GameMath.Vector3FromRotation(angle, angle2, length);
        }

        /// <summary>
        /// Randomly create the zero vector
        /// </summary>
        public static RandomVector3 Zero
        {
            get
            {
                return new RandomVector3(0);
            }
        }
    }
}
