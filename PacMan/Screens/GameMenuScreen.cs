using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    class GameMenuScreen : FavMenuScreen
    {
        GameScreen gameScreen;

        public GameMenuScreen(ScreenManager sM, GameScreen gameScreen)
            : base(sM)
        {
            // Some settings
            this.gameScreen = gameScreen;
            this.IsTransparent = true;
            this.BlocksUpdating = true;
            this.MenuSize = new Vector2(400, 200);
            this.PositionMenuScreen(MenuItem.HorizontalAlign.Center, MenuItem.VerticalAlign.Middle);
            this.FavMenuItem.TextAlign = MenuItem.HorizontalAlign.Center;

            // The menu
            MenuItem miContinue = new MenuItem("Continue", sM.Graphics);
            miContinue.OnClick += (o, e) => { this.continueGame(); };
            this.AddMenuItem(miContinue);
            MenuItem miHowToPLay = new MenuItem("How to play", sM.Graphics);
            miHowToPLay.OnClick += (o, e) => {
                this.screenManager.AddScreen(new HowToPlayScreen(this.screenManager));
            };
            this.AddMenuItem(miHowToPLay);
            MenuItem miQuit = new MenuItem("Quit level", sM.Graphics);
            miQuit.OnClick += (o, e) => {
                this.Remove();
                this.gameScreen.Remove();
            };
            this.AddMenuItem(miQuit);
            MenuItem miExit = new MenuItem("Exit game", sM.Graphics);
            miExit.OnClick += (o, e) => { this.screenManager.Exit(); };
            this.AddMenuItem(miExit);

            // Use the fast layout method to set the layout of the menu's
            this.FastLayout(this.FavMenuItem, 10);
        }

        public override void OnEscapePress()
        {
            this.continueGame();
        }

        private void continueGame()
        {
            this.Remove();
            this.gameScreen.Continue();
        }
    }
}
