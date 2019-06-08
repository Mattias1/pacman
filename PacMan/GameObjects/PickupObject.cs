using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class PickupObject : GameObject
    {
        public int Type { get; private set; }
        public Level Level { get; private set; }
        public Vector2 GridPosition { get; private set; }

        public PickupObject(int type, Vector2 pos, Level level)
            : base(pos, Vector2.Zero)
        {
            this.Level = level;
            this.Type = type;
            this.GridPosition = this.Level.WorldToGrid(pos);
        }

        public bool Remove()
        {
            return this.Level.PickupObjects.Remove(this);
        }

        public override void Draw(UpdateEventArgs e)
        {
            Graphics g = this.Level.Graphics;

            switch (this.Type) {
            case Level.ORB:
                g.SetOrbModel(this.Position);
                g.OrbSurface.Render();
                break;
            case Level.SUPERORB:
                g.SetOrbModel(this.Position, 2f);
                g.OrbSurface.Render();
                break;
            }
        }
    }
}
