using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    class ScoreText : ParticleEmitterText3D
    {
        public ScoreText(int score, Vector2 position, Graphics g, float awesomeFactor, Color color)
            : this("+" + score.ToString(), position, g, awesomeFactor, color) { }
        public ScoreText(string text, Vector2 position, Graphics g, float awesomeFactor, Color color)
            : base(text, new Vector3(position.X, 0, position.Y), 1, g.FontGeometry3D)
        {
            this.SecondsBetweenSpawning = awesomeFactor + 9; // Die before spawning again
            this.AverageLifetime = awesomeFactor + 0.5f;     // Die before spawning again
            this.Velocity = RandomVector3.Zero;
            this.Acceleration = RandomVector3.Zero;
            this.NumberOfParticlesToSpawn = 1;
            this.Color = color;
            this.SetScales(.7f * awesomeFactor, 1.4f * awesomeFactor);
            this.SetPercentageAlphas(0f, .8f, 80f, .8f, 100f, 0f);
            Vector3 closer = g.Camera.Eye - g.Camera.Focus;
            closer.NormalizeFast();
            this.Position += 3 * closer;
        }
    }
}
