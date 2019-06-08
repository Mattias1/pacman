using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class TextParticle2D : TextParticle3D
    {
        public TextParticle2D() { }

        public override void Initialize(Vector3 position, Vector3 velocity, Vector3 acceleration, float lifeTime, PercentageArray<Color> colorPs, PercentageArray<float> scalePs, PercentageArray<float> alphas, FontGeometry geo)
        {
            base.Initialize(position, velocity, acceleration, lifeTime, colorPs, scalePs, alphas, geo);

            this.position.Z = 0;
            this.velocity.Z = 0;
            this.acceleration.Z = 0;
        }
    }
}
