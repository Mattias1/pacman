using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class ParticleEmitterText2D : ParticleEmitterText3D
    {
        public ParticleEmitterText2D(string text, Vector2 position, int emitterLifeTime, FontGeometry geo)
            : base(text, new Vector3(position), emitterLifeTime, geo)
        {
            this.Velocity = RandomVector2.Zero;
            this.Acceleration = RandomVector2.Zero;
        }
    }
}
