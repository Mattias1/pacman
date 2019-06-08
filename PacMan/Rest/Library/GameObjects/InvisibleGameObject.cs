using System;
using amulware.Graphics;
using OpenTK;

namespace PacMan
{
    public abstract class InvisibleGameObject
    {
        public Vector2 Position;
        public Vector2 Velocity;

        public InvisibleGameObject(Vector2 p, Vector2 v) {
            this.Position = p;
            this.Velocity = v;
        }

        public virtual void Update(UpdateEventArgs e)
        {
            this.Position += this.Velocity * e.ElapsedTimeInSf;
        }
    }
}
