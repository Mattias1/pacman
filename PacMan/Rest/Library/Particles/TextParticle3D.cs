using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class TextParticle3D : Particle<UVColorVertexData, FontGeometry>
    {
        string text;

        public TextParticle3D() { }

        public void SetText(string text)
        {
            this.text = text;
        }

        protected override void draw(Vector3 position, float scale, Color color)
        {
            this.geometry.Height = scale;
            this.geometry.Color = color;
            this.geometry.DrawMultiLineString(this.position, this.text, 0.5f, 0.5f);
        }
    }
}
