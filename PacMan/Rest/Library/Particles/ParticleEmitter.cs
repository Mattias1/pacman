using System;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public abstract class ParticleEmitter<VertexData, Geo, ParticleT>
        where VertexData : struct, IVertexData
        where Geo : Geometry<VertexData>
        where ParticleT : Particle<VertexData, Geo>, new()
    {
        // Particle emitter settings
        /// <summary>
        /// The amount of seconds between the spawning of new particles
        /// </summary>
        public float SecondsBetweenSpawning = 0.005f;
        /// <summary>
        /// The number of particles to spawn when spawning.
        /// </summary>
        public int NumberOfParticlesToSpawn = 1;
        /// <summary>
        /// The start position of the particles
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// The random distribution used
        /// </summary>
        public GlobalRandom.Distribution Distribution = GlobalRandom.Distribution.DoubleUniform;
        /// <summary>
        /// Average life time of the particles in miliseconds
        /// </summary>
        public PercentageArray<float> AverageLifetimes = PercentageArray<float>.Singleton(1000);
        /// <summary>
        /// Depending on the distribution.
        /// Normal: standard deviation, (Double)Uniform Uniform: [avg - 2 * dev, avg + 2 * dev]
        /// </summary>
        public float LifeTimeDeviation = 0;
        /// <summary>
        /// The velocity vector (pixels per second)
        /// </summary>
        public RandomVector3 Velocity = RandomVector3.Zero;
        /// <summary>
        /// The acceleration vector (pixels per square second)
        /// </summary>
        public RandomVector3 Acceleration = RandomVector3.Zero;
        /// <summary>
        /// Direction in [minDirection, maxDirection]
        /// </summary>
        public float MinDirection = 0;
        /// <summary>
        /// Direction in [minDirection, maxDirection]
        /// </summary>
        public float MaxDirection = MathHelper.TwoPi;
        /// <summary>
        /// The sizes the particles should lerp between
        /// </summary>
        public PercentageArray<float> ScalePercentages = PercentageArray<float>.Singleton(1f);
        /// <summary>
        /// The colours of the particle (with lineair interpolation between the percentages)
        /// </summary>
        public PercentageArray<Color> ColorPercentages = PercentageArray<Color>.Singleton(Color.White);
        /// <summary>
        /// The alpha values (in [0,1]) of the particle (with lineair interpolation between the percentages)
        /// </summary>
        public PercentageArray<float> AlphaPercentages = PercentageArray<float>.Singleton(0.3f);
        /// <summary>
        /// Whether or not the scale should get smaller when the lifetime does.
        /// </summary>
        public bool ScaleDiesToo = false;
        /// <summary>
        /// Whether or not the new alpha values given to new particles should get smaller when the lifetime does.
        /// </summary>
        public bool AlphaDiesToo = false;
        /// <summary>
        /// The amount of particles to generate right when the particle system starts
        /// </summary>
        public int PreGenerate = 0;
        /// <summary>
        /// The geometry used for the particle
        /// </summary>
        public Geo Geometry;

        // Percentage array setters
        /// <summary>
        /// The lifetime of the particle (in s)
        /// </summary>
        public float AverageLifetime
        {
            set { this.AverageLifetimes = PercentageArray<float>.Singleton(value); }
        }
        /// <summary>
        /// Set the lifetimes (and interpolate the percentages linearely) (in s)
        /// </summary>
        /// <param name="lifetimes"></param>
        public void SetAverageLifetimes(params float[] lifetimes)
        {
            this.AverageLifetimes = PercentageArray<float>.FromArray(lifetimes);
        }
        /// <summary>
        /// Set the percentages and lifetimes (in the form: pct, time, pct, time, ...) (in s)
        /// </summary>
        /// <param name="percentageLifetimes"></param>
        public void SetPercentageAverageLifetimes(params object[] percentageLifetimes)
        {
            this.AverageLifetimes = PercentageArray<float>.FromParams(percentageLifetimes);
        }

        /// <summary>
        /// The colour of the particle
        /// </summary>
        public Color Color
        {
            set { this.ColorPercentages = PercentageArray<Color>.Singleton(value); }
        }
        /// <summary>
        /// Set the colours (and interpolate the percentages linearely)
        /// </summary>
        /// <param name="colors"></param>
        public void SetColors(params Color[] colors)
        {
            this.ColorPercentages = PercentageArray<Color>.FromArray(colors);
        }
        /// <summary>
        /// Set the percentages and colours (in the form: pct, color, pct, color, ...)
        /// </summary>
        /// <param name="percentageColors"></param>
        public void SetPercentageColors(params object[] percentageColors)
        {
            this.ColorPercentages = PercentageArray<Color>.FromParams(percentageColors);
        }

        /// <summary>
        /// The scale of the particle
        /// </summary>
        public float Scale
        {
            set { this.ScalePercentages = PercentageArray<float>.Singleton(value); }
        }
        /// <summary>
        /// Set the scales (and interpolate the percentages linearely)
        /// </summary>
        /// <param name="scales"></param>
        public void SetScales(params float[] scales)
        {
            this.ScalePercentages = PercentageArray<float>.FromArray(scales);
        }
        /// <summary>
        /// Set the percentages and scales (in the form: pct, scale, pct, scale, ...)
        /// </summary>
        /// <param name="percentageScales"></param>
        public void SetPercentageScales(params object[] percentageScales)
        {
            this.ScalePercentages = PercentageArray<float>.FromParams(percentageScales);
        }

        /// <summary>
        /// The alpha value of the color (in [0,1])
        /// </summary>
        public float Alpha
        {
            set { this.AlphaPercentages = PercentageArray<float>.Singleton(value); }
        }
        /// <summary>
        /// Set the alpha values (in [0,1]) (and interpolate the percentages linearely)
        /// </summary>
        /// <param name="scales"></param>
        public void SetAlphas(params float[] scales)
        {
            this.AlphaPercentages = PercentageArray<float>.FromArray(scales);
        }
        /// <summary>
        /// Set the percentages and alpha values (in [0,1]) (in the form: pct, alpha, pct, alpha, ...)
        /// </summary>
        /// <param name="percentageAlphaValues"></param>
        public void SetPercentageAlphas(params object[] percentageAlphaValues)
        {
            this.AlphaPercentages = PercentageArray<float>.FromParams(percentageAlphaValues);
        }

        // Private members
        protected LinkedList<ParticleT> particles;
        protected float timeLeftToSpawn = 0; // In seconds
        protected float emitterLifeTime;     // In seconds
        protected float emitterTimeLeft;     // In seconds
        protected bool firstTime = true;

        // Methods
        public ParticleEmitter(Vector3 position, int emitterLifeTime, Geo geo)
        {
            this.Position = position;
            this.particles = new LinkedList<ParticleT>();
            this.emitterTimeLeft = this.emitterLifeTime = emitterLifeTime;
            this.Geometry = geo;
        }

        public virtual void Update(UpdateEventArgs e)
        {
            if (this.firstTime) {
                // Pregenerate particles if nescessary
                for (int i = 0; i < this.PreGenerate; i++) {
                    ParticleT p = this.addParticle(0);
                    int maxUpdates = (int)(this.AverageLifetimes[0].Value / this.Velocity.AverageLength);
                    int amountOfUpdates = GlobalRandom.Next(0, maxUpdates);
                    for (int j = 0; j < amountOfUpdates; j++)
                        p.Update(e);
                }
                this.firstTime = false;
            }

            // Substract lifetime and check if there is nothing left of it.
            if (this.manageLifetime(e))
                this.Stop();

            // Add new particles if nescessary
            if (!this.Stopped()) {
                this.timeLeftToSpawn -= e.ElapsedTimeInSf;
                while (this.timeLeftToSpawn < 0) {
                    for (int i = 0; i < this.NumberOfParticlesToSpawn; i++)
                        this.addParticle(e.ElapsedTimeInSf);
                    this.timeLeftToSpawn += this.SecondsBetweenSpawning;
                }
            }

            // Update all the live particles and remove those that are dead.
            LinkedListNode<ParticleT> n = this.particles.First;
            while (n != null) {
                if (n.Value.IsDead()) {
                    if (n == this.particles.First) {
                        this.particles.RemoveFirst();
                        n = this.particles.First;
                        continue;
                    }
                    else {
                        n = n.Previous;
                        this.particles.Remove(n.Next);
                        n = n.Next;
                        continue;
                    }
                }
                n.Value.Update(e);
                n = n.Next;
            }
        }

        public virtual void Draw(UpdateEventArgs e)
        {
            foreach (ParticleT p in this.particles)
                p.Draw(e);
        }

        protected virtual ParticleT createParticle(float elepsedS)
        {
            float percentage = this.calculateEmitterPercentage();

            // Calculate the lifetime
            float lifetime = 1f;
            if (this.AverageLifetimes.Length == 1) {
                lifetime = this.AverageLifetimes[0].Value;
            }
            else {
                int i = this.AverageLifetimes.GetToIndex(percentage);
                lifetime = GameMath.Lerp(this.AverageLifetimes[i - 1].Value, this.AverageLifetimes[i].Value, AverageLifetimes.GetLerpFactor(i, percentage));
            }
            lifetime = (float)GlobalRandom.Rand(this.Distribution, lifetime, this.LifeTimeDeviation);

            // Adjust the scales
            PercentageArray<float> scales = this.ScalePercentages.Clone();
            if (ScaleDiesToo) {
                float factor = lifetime / this.AverageLifetimes[0].Value;
                for (int i = 0; i < scales.Length; i++)
                    scales[i].Value = scales[i].Value * factor;
            }

            // Adjust the alphas
            PercentageArray<float> alphas = this.AlphaPercentages.Clone();
            if (AlphaDiesToo) {
                float factor = lifetime / this.AverageLifetimes[0].Value;
                for (int i = 0; i < alphas.Length; i++)
                    alphas[i].Value = alphas[i].Value * factor;
            }

            // Create the particle
            ParticleT particle = this.createParticle(elepsedS, lifetime, scales, alphas);
            return particle;
        }
        //protected abstract ParticleT createParticle(float elepsedS, float lifetime, PercentageArray<float> scales, byte alpha);
        protected virtual ParticleT createParticle(float elepsedS, float lifetime, PercentageArray<float> scales, PercentageArray<float> alphas)
        {
            Vector3 velocity = this.Velocity.Create();
            ParticleT particle = new ParticleT();
            particle.Initialize(
                this.Position + velocity * elepsedS * (GlobalRandom.Next(4) + 1f) / 4f,
                velocity,
                this.Acceleration.Create(),
                lifetime,
                this.ColorPercentages,
                scales,
                alphas,
                this.Geometry
            );
            return particle;
        }
        protected virtual ParticleT addParticle(float elepsedS)
        {
            ParticleT particle = this.createParticle(elepsedS);
            this.particles.AddLast(particle);
            return particle;
        }

        /// <summary>
        /// The factor of the time it has lived so far. Kinda scary when you think about it huh.
        /// </summary>
        /// <returns></returns>
        protected virtual float calculateEmitterFactor()
        {
            return 1f - this.emitterTimeLeft / (float)this.emitterLifeTime;
        }
        protected float calculateEmitterPercentage()
        {
            return 100f * this.calculateEmitterFactor();
        }

        /// <summary>
        /// The lifetime part of the update loop
        /// </summary>
        /// <param name="e"></param>
        /// <returns>True when lifetime is over, false when alive</returns>
        protected virtual bool manageLifetime(UpdateEventArgs e)
        {
            return (this.emitterTimeLeft -= e.ElapsedTimeInSf) < 0;
        }

        // Methods concerning ending the emitter
        public virtual void Stop()
        {
            this.SecondsBetweenSpawning = 0;
        }

        public bool Stopped()
        {
            return this.SecondsBetweenSpawning == 0;
        }

        public void KillAllParticles()
        {
            this.particles.Clear();
        }

        public bool Dead()
        {
            return this.Stopped() && this.particles.Count == 0;
        }
    }
}
