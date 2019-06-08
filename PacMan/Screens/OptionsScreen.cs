using System;
using OpenTK;
using OpenTK.Input;
using amulware.Graphics;

namespace PacMan
{
    class OptionsScreen : FavMenuScreen
    {
        BackgroundLevel bgLevel;
        MenuItem miBack;
        MenuItemTextbox miName;
        MenuItemYesNo miGhostSpeed;
        string errorMessages;

        public OptionsScreen(ScreenManager sM, BackgroundLevel bgLevel = null)
            : base(sM)
        {
            // Some settings
            if (bgLevel == null)
                this.bgLevel = new BackgroundLevel(this.screenManager);
            else
                this.bgLevel = bgLevel;
            this.errorMessages = "";
            this.MenuSize = this.screenManager.ScreenResolution - 150 * Vector2.UnitX;
            this.PositionMenuScreen(MenuItem.HorizontalAlign.Center, MenuItem.VerticalAlign.Top);
            this.FavMenuItem.TextAlign = MenuItem.HorizontalAlign.Left;

            // The options
            this.miName = new MenuItemTextbox("Name", this.screenManager.Graphics);
            this.AddMenuItem(miName);
            this.miGhostSpeed = new MenuItemYesNo("Fast ghosts", this.screenManager.Graphics);
            this.AddMenuItem(miGhostSpeed);

            // Save and back buttons
            MenuItem miSave = new MenuItem("Save", sM.Graphics);
            miSave.OnClick += (o, e) => { this.Save(); };
            this.AddMenuItem(miSave);
            this.miBack = new MenuItem("Back", sM.Graphics);
            this.miBack.OnClick += (o, e) => { this.Remove(); };
            this.AddMenuItem(this.miBack);

            // Colour the menus (and position them)
            this.FastLayout(this.FavMenuItem);
            miBack.TextAlign = MenuItem.HorizontalAlign.Right;

            // Position the menus (override previous positioning)
            this.positionMenuItems(miSave);

            // Load the values
            this.Load();
        }

        public void Load()
        {
            this.miName.Value = Settings.Get.Name;
            this.miGhostSpeed.SelectedIndex = Settings.Get.Speed ? 0 : 1;
        }

        public void Save()
        {
            // Initialize some variables
            this.errorMessages = "";

            // The name should not be empty
            if (this.miName.Value == "")
                this.errorMessages = "Please enter a name";

            // Don't continue if there are errors
            if (this.errorMessages != "")
                return;

            // Set the actual settings
            Settings.Get.Name = this.miName.Value;
            Settings.Get.Speed = this.miGhostSpeed.SelectedIndex == 0;

            // Go back
            this.Remove();
        }

        private void positionMenuItems(MenuItem miSave)
        {
            // The options
            this.miName.PositionInsideMenuScreen(this, MenuItem.HorizontalAlign.Left, MenuItem.VerticalAlign.Top, 80);
            this.miGhostSpeed.PositionFrom(this.miName, MenuItem.HorizontalAlign.CopyLeft, MenuItem.VerticalAlign.Bottom);

            // The save and back buttons
            miSave.PositionInsideMenuScreen(this, MenuItem.HorizontalAlign.Left, MenuItem.VerticalAlign.Bottom, 80);
            this.miBack.PositionInsideMenuScreen(this, MenuItem.HorizontalAlign.Right, MenuItem.VerticalAlign.Bottom, 80);
        }

        public override void OnEscapePress()
        {
            this.SelectMenuItem(this.miBack);
        }

        public override void Draw(UpdateEventArgs e)
        {
            if (this.bgLevel != null)
                this.bgLevel.Draw(e);

            base.Draw(e);

            this.drawVersion();

            this.screenManager.Graphics.FontGeometry.Color = Color.Red;
            this.screenManager.Graphics.DrawMultiLineString(this.errorMessages, 50 * Vector2.UnitY, 30);
            this.screenManager.Graphics.FontGeometry.Color = Color.White;
        }
    }
}
