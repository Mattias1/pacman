using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public abstract class Particle<VertexData, Geo>
        where VertexData : struct, IVertexData
        where Geo : Geometry<VertexData>
    {
        protected Vector3 position;       // in px
        protected Vector3 velocity;       // in px/s
        protected Vector3 acceleration;   // in px/s
        protected float lifeTime;         // in s
        protected float timeLeft;         // in s
        protected PercentageArray<float> alphaPercentages;
        protected PercentageArray<Color> colorPercentages;
        protected PercentageArray<float> scalePercentages;
        protected Geo geometry;

        public Particle() { }

        public virtual void Initialize(Vector3 position, Vector3 velocity, Vector3 acceleration, float lifeTime,
            PercentageArray<Color> colorPs, PercentageArray<float> scalePs, PercentageArray<float> alphaPs, Geo geo)
        {
            this.position = position;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.timeLeft = this.lifeTime = lifeTime;
            this.colorPercentages = colorPs;
            this.scalePercentages = scalePs;
            this.alphaPercentages = alphaPs;
            this.geometry = geo;
        }

        public virtual void Update(UpdateEventArgs e)
        {
            this.timeLeft -= e.ElapsedTimeInSf;
            this.velocity += this.acceleration * e.ElapsedTimeInSf;
            this.position += this.velocity * e.ElapsedTimeInSf;
        }

        public virtual void Draw(UpdateEventArgs e)
        {
            // Don't draw dead particles
            if (timeLeft <= 0)
                return;

            // Calculate the colour
            Color color = Color.White;
            float pct = 100f * (1f - this.timeLeft / this.lifeTime);
            if (this.colorPercentages.Length == 1) {
                color = this.colorPercentages[0].Value;
            }
            else {
                int i = this.colorPercentages.GetToIndex(pct);
                color = GameMath.Lerp(this.colorPercentages[i - 1].Value, this.colorPercentages[i].Value, colorPercentages.GetLerpFactor(i, pct));
            }

            // Calculate the scale
            float scale = 1f;
            if (this.scalePercentages.Length == 1) {
                scale = this.scalePercentages[0].Value;
            }
            else {
                int i = this.scalePercentages.GetToIndex(pct);
                scale = GameMath.Lerp(this.scalePercentages[i - 1].Value, this.scalePercentages[i].Value, scalePercentages.GetLerpFactor(i, pct));
            }

            // Calculate the alpha
            float alpha = 1f;
            if (this.alphaPercentages.Length == 1) {
                alpha = this.alphaPercentages[0].Value;
            }
            else {
                int i = this.alphaPercentages.GetToIndex(pct);
                alpha = GameMath.Lerp(this.alphaPercentages[i - 1].Value, this.alphaPercentages[i].Value, alphaPercentages.GetLerpFactor(i, pct));
            }

            // Draw the sprite
            color.A = (byte)(255 * alpha);
            this.draw(this.position, scale, color);
        }
        protected abstract void draw(Vector3 position, float scale, Color color);

        public bool IsDead()
        {
            return this.timeLeft <= 0;
        }
    }
}
