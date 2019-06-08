using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class Particle3D : Particle<UVColorVertexData, Sprite2DGeometry>
    {
        public Particle3D() { }

        protected override void draw(Vector3 position, float scale, Color color)
        {
            this.geometry.Color = color;
            this.geometry.DrawSprite(this.position, 0, scale);
        }
    }
}
