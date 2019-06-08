using System;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class ParticleSystem
    {
        public List<ParticleEmitter3D> ParticleEmitters3D;
        public List<ParticleEmitterText3D> TextParticleEmitters3D;

        public ParticleSystem()
        {
            this.ParticleEmitters3D = new List<ParticleEmitter3D>();
            this.TextParticleEmitters3D = new List<ParticleEmitterText3D>();
        }

        public void Update(UpdateEventArgs e)
        {
            // Update all emitters and remove the dying ones
            for (int i = this.ParticleEmitters3D.Count - 1; i >= 0; i--) {
                if (this.ParticleEmitters3D[i].Dead())
                    this.ParticleEmitters3D.RemoveAt(i);
                else
                    this.ParticleEmitters3D[i].Update(e);
            }
            for (int i = this.TextParticleEmitters3D.Count - 1; i >= 0; i--) {
                if (this.TextParticleEmitters3D[i].Dead())
                    this.TextParticleEmitters3D.RemoveAt(i);
                else
                    this.TextParticleEmitters3D[i].Update(e);
            }
        }

        public void Draw(UpdateEventArgs e)
        {
            foreach (ParticleEmitter3D particleEmitter in this.ParticleEmitters3D)
                particleEmitter.Draw(e);
            foreach (ParticleEmitterText3D particleEmitter in this.TextParticleEmitters3D)
                particleEmitter.Draw(e);
        }

        public void Add(ParticleEmitter3D particleEmitter)
        {
            this.ParticleEmitters3D.Add(particleEmitter);
        }
        public void Add(ParticleEmitterText3D particleEmitter)
        {
            this.TextParticleEmitters3D.Add(particleEmitter);
        }
    }
}
