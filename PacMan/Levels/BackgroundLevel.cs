using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    class BackgroundLevel : Level
    {
        public BackgroundLevel(ScreenManager sM, string map = "")
            : base(sM, map, new AI[6]) { }

        public override void Initialize()
        {
            this.setPreferredCameraFocus();
            this.Camera.MoveFocusTo(new Vector3(this.PreferredCameraFocus.X, 0, this.PreferredCameraFocus.Y));
        }

        public override void Update(UpdateEventArgs e) { }

        public override void Draw(UpdateEventArgs e)
        {
            this.Graphics.WallSurface.Render();
        }
    }
}
