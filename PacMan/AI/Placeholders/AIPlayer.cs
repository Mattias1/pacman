using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class AIPlayer : AI
    {
        public AIPlayer() { }

        public override Vector2 GetUpdateDir()
        {
            return this.Level.GetDirFromDirectionKey();
        }
    }
}
