using System;
using amulware.Graphics;
using OpenTK;

namespace PacMan
{
    public abstract class GameObject : InvisibleGameObject
    {
        public GameObject(Vector2 p, Vector2 v)
            : base(p, v) { }

        abstract public void Draw(UpdateEventArgs e);
    }
}
