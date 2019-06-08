using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class ParticleEmitterText3D : ParticleEmitter<UVColorVertexData, FontGeometry, TextParticle3D>
    {
        string text;

        public ParticleEmitterText3D(string text, Vector3 position, int emitterLifeTime, FontGeometry geo)
            : base(position, emitterLifeTime, geo)
        {
            this.text = text;
        }

        protected override TextParticle3D createParticle(float elepsedS, float lifetime, PercentageArray<float> scales, PercentageArray<float> alphas)
        {
            TextParticle3D particle = base.createParticle(elepsedS, lifetime, scales, alphas);
            particle.SetText(this.text);
            return particle;
        }
    }
}
