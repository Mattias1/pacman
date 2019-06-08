using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    abstract class FavMenuScreen : MenuScreen
    {
        public MenuItem FavMenuItem { get; private set; }

        public FavMenuScreen(ScreenManager sM, bool visible = true, bool transparent = false)
            : base(sM, visible, transparent)
        {
            // Some basic settings for all screens
            this.HorizontalAndVerticalArrowKeys = false;

            // A basic menu item to copy values from
            this.FavMenuItem = new MenuItem("Default", sM.Graphics);
            this.FavMenuItem.Colour = Color.White;
            this.FavMenuItem.SelectedColour = Color.Orange;
            this.FavMenuItem.Size = new Vector2(400, 40);
            this.FavMenuItem.OnClick += (o, e) => { this.screenManager.Graphics.AddScreenshake(4, 1, .04f); };
        }

        protected void drawVersion()
        {
            this.screenManager.Graphics.FontGeometry.Height = 15;
            this.screenManager.Graphics.FontGeometry.DrawString(
                this.screenManager.ScreenResolution - new Vector2(25, 15), "Version " + Settings.VersionString, 1, 1
            );
        }
    }
}
