using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    class AISim : AI
    {
        public Vector2 DirectionVector;

        public AISim()
        {
            this.DirectionVector = Vector2.Zero;
        }

        public override Vector2 GetUpdateDir()
        {
            return this.DirectionVector;
        }
    }
}
