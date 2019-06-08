using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    class TitleScreen : FavMenuScreen
    {
        BackgroundLevel bgLevel;
        MenuItem miExit;

        public TitleScreen(ScreenManager sM)
            : base(sM)
        {
            // Some settings
            this.bgLevel = new BackgroundLevel(this.screenManager);
            this.bgLevel.RandomizeMapColor(true);
            this.MenuSize = new Vector2(400, 270);
            this.PositionMenuScreen(MenuItem.HorizontalAlign.Center, MenuItem.VerticalAlign.Middle);
            this.FavMenuItem.TextAlign = MenuItem.HorizontalAlign.Center;

            // The menu
            MenuItem miSelectLevel = new MenuItem("Singleplayer", sM.Graphics);
            miSelectLevel.OnClick += (o, e) => {
                this.screenManager.AddScreen(new LobbyScreen(this.screenManager, this.bgLevel));
            };
            this.AddMenuItem(miSelectLevel);
            MenuItem miSelectLevelMP = new MenuItem("Multiplayer", sM.Graphics);
            miSelectLevelMP.OnClick += (o, e) => {
                this.screenManager.AddScreen(new FindGamesScreen(this.screenManager, this.bgLevel));
            };
            this.AddMenuItem(miSelectLevelMP);
            MenuItem miHowToPLay = new MenuItem("How to play", sM.Graphics);
            miHowToPLay.OnClick += (o, e) => {
                this.screenManager.AddScreen(new HowToPlayScreen(this.screenManager, this.bgLevel));
            };
            this.AddMenuItem(miHowToPLay);
            MenuItem miOptions = new MenuItem("Options", sM.Graphics);
            miOptions.OnClick += (o, e) => {
                this.screenManager.AddScreen(new OptionsScreen(this.screenManager, this.bgLevel));
            };
            this.AddMenuItem(miOptions);
            this.miExit = new MenuItem("Exit", sM.Graphics);
            this.miExit.OnClick += (o, e) => {
                this.screenManager.Exit();
            };
            this.AddMenuItem(this.miExit);

            // Use the fast layout method to set the layout of the menu's
            this.FastLayout(this.FavMenuItem, 10);
        }

        public override void OnEscapePress()
        {
            this.SelectMenuItem(this.miExit);
        }

        public override void Draw(UpdateEventArgs e)
        {
            this.bgLevel.Draw(e);

            base.Draw(e);
        }
    }
}
