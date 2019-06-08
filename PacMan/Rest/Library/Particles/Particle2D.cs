using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class Particle2D : Particle3D
    {
        public Particle2D() { }

        public override void Initialize(Vector3 position, Vector3 velocity, Vector3 acceleration, float lifeTime, PercentageArray<Color> colorPs, PercentageArray<float> scalePs, PercentageArray<float> alphaPs, Sprite2DGeometry geo)
        {
            base.Initialize(position, velocity, acceleration, lifeTime, colorPs, scalePs, alphaPs, geo);

            this.position.Z = 0;
            this.velocity.Z = 0;
            this.acceleration.Z = 0;
        }
    }
}
