using System;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class ParticleEmitter3D : ParticleEmitter<UVColorVertexData, Sprite2DGeometry, Particle3D>
    {
        // Methods
        public ParticleEmitter3D(Vector3 position, int emitterLifeTime, Sprite2DGeometry geo)
            : base(position, emitterLifeTime, geo) { }
    }
}
