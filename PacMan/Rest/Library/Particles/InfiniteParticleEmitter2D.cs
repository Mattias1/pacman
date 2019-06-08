using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public abstract class InfiniteParticleEmitter2D : ParticleEmitter2D
    {
        public InfiniteParticleEmitter2D(Vector2 position, Sprite2DGeometry geo)
            : base(position, 1, geo) { }

        protected override float calculateEmitterFactor()
        {
            return 0f;
        }

        protected override bool manageLifetime(UpdateEventArgs e)
        {
            return false;
        }
    }
}
