using System;
using OpenTK;
using OpenTK.Input;
using amulware.Graphics;

namespace PacMan
{
    class GameScreen : Screen
    {
        Level level;

        public GameScreen(ScreenManager sM, string map, AI[] ais, MultiPlayerInfo mpInfo = null)
            : base(sM)
        {
            this.level = new Level(sM, map, ais, mpInfo);
        }

        public override void Update(UpdateEventArgs e)
        {
            base.Update(e);

            if (InputManager.Keys.Hit(Key.Escape) && this.level.State != Level.States.Lost && this.level.State != Level.States.Won) {
                this.level.Pause();
                this.level.DrawText = false;
                this.screenManager.AddScreen(new GameMenuScreen(this.screenManager, this));
            }

            if (InputManager.Keys.Hit(Key.C)) {
                if (this.level.PickupObjects.Count > 6)
                    this.level.PickupObjects.RemoveRange(0, this.level.PickupObjects.Count - 6);
            }

            this.level.Update(e);

            if (this.level.GameOver() && InputManager.IsConfirmHit())
                this.Remove();
        }

        public void Continue()
        {
            this.level.DrawText = true;
            this.level.Start();
        }

        public override void Draw(UpdateEventArgs e)
        {
            this.level.Draw(e);
        }
    }
}
