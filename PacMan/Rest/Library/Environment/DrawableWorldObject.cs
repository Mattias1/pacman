using System;
using amulware.Graphics;
using OpenTK;

namespace Squarrel
{
    public abstract class DrawableWorldObject<Env> : WorldObject<Env> where Env : GameEnvironment<Env>
    {
        protected Vector2 position;
        protected Vector2 velocity; // pixels/second

        public Vector2 Position {
            get { return this.position; }
        }
        public Vector2 Velocity {
            get { return this.velocity; }
        }

        public DrawableWorldObject(Env environment)
            : base(environment) { }

        public override void Update(UpdateEventArgs e) {
            this.position += this.velocity * (float)e.ElapsedTimeInS;
        }

        abstract public void Draw(UpdateEventArgs e);
    }
}
