using System;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public abstract class ParticleEmitter2D : ParticleEmitter<UVColorVertexData, Sprite2DGeometry, Particle2D>
    {
        public ParticleEmitter2D(Vector2 position, int emitterLifeTime, Sprite2DGeometry geo)
            : base(new Vector3(position), emitterLifeTime, geo)
        {
            this.Velocity = RandomVector2.Zero;
        }

        // I seriously doubt if this class works (it should either override PEmitter3D, or have its own creator right?)
    }
}
