using System;
using System.Net;
using OpenTK;
using OpenTK.Input;
using amulware.Graphics;

namespace PacMan
{
    class FindGamesScreen : FavMenuScreen
    {
        BackgroundLevel bgLevel;
        MenuItem miBack;
        MenuItemTextbox miIP;
        MenuItemTextbox miPort;
        string errorMessages;

        public FindGamesScreen(ScreenManager sM, BackgroundLevel bgLevel = null)
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

            // IP and port
            this.miIP = new MenuItemTextbox("IP adress", this.screenManager.Graphics, 15);
            this.AddMenuItem(miIP);
            this.miPort = new MenuItemTextbox("Port", this.screenManager.Graphics, 7);
            this.AddMenuItem(miPort);

            // Create, join and back buttons
            MenuItem miCreate = new MenuItem("Create game", sM.Graphics);
            miCreate.OnClick += (o, e) => { this.Create(); };
            this.AddMenuItem(miCreate);
            MenuItem miJoin = new MenuItem("Join game", sM.Graphics);
            miJoin.OnClick += (o, e) => { this.Join(); };
            this.AddMenuItem(miJoin);
            this.miBack = new MenuItem("Back", sM.Graphics);
            this.miBack.OnClick += (o, e) => { this.Remove(); };
            this.AddMenuItem(this.miBack);

            // Colour and position the menus
            this.FastLayout(this.FavMenuItem);
            miCreate.Size = new Vector2(290, miCreate.Size.Y);
            miJoin.Size = miCreate.Size;
            miBack.Size = miCreate.Size;
            miBack.TextAlign = MenuItem.HorizontalAlign.Right;
            this.miIP.Size += new Vector2(170, 0);
            this.positionMenuItems(miCreate, miJoin);

            // Load the values
            this.Load();
        }

        public void Load()
        {
            this.miIP.Value = Settings.Get.IP.ToString();
            this.miPort.Value = Settings.Get.Port.ToString();
        }

        public void Create()
        {
            this.errorMessages = "";
            if (!this.saveIPandPort())
                return;
            MultiPlayerInfo mpInfo = new MultiPlayerInfo(true);
            mpInfo.Server.Start();

            this.Remove();
            this.screenManager.AddScreen(new LobbyScreen(this.screenManager, this.bgLevel, mpInfo));
        }

        public void Join()
        {
            // Try to connect
            bool lastErrorWasConnectionFailed = this.errorMessages == "Connection failed.";
            this.errorMessages = "";
            if (!this.saveIPandPort())
                return;
            MultiPlayerInfo mpInfo = new MultiPlayerInfo(false);
            if (!mpInfo.Client.Connect(Settings.Get.IP, Settings.Get.Port)) {
                this.errorMessages = lastErrorWasConnectionFailed ? "Connection failed!" : "Connection failed.";
                return;
            }
            mpInfo.Client.Start();

            // Start the lobby
            this.Remove();
            this.screenManager.AddScreen(new LobbyScreen(this.screenManager, this.bgLevel, mpInfo));
        }

        private bool saveIPandPort()
        {
            // Get the IP and port
            int port;
            if (!int.TryParse(this.miPort.Value, out port))
                this.errorMessages = "The port should be a number.";

            IPAddress ip;
            if (!IPAddress.TryParse(this.miIP.Value, out ip)) {
                this.errorMessages = "The IP adress should contain 4 numbers between 0 and 255";
                this.errorMessages += Environment.NewLine + " seperated by dots, like '127.0.0.1'";
            }

            // If there are no errors, save the values
            if (this.errorMessages != "")
                return false;

            Settings.Get.IP = ip;
            Settings.Get.Port = port;
            return true;
        }

        private void positionMenuItems(MenuItem miCreate, MenuItem miJoin)
        {
            // The create, join and back buttons
            miCreate.PositionInsideMenuScreen(this, MenuItem.HorizontalAlign.Left, MenuItem.VerticalAlign.Bottom, 80);
            miJoin.PositionFrom(miCreate, MenuItem.HorizontalAlign.Right, MenuItem.VerticalAlign.CopyBottom, 10);
            this.miBack.PositionInsideMenuScreen(this, MenuItem.HorizontalAlign.Right, MenuItem.VerticalAlign.Bottom, 80);

            // The IP and port
            this.miIP.PositionInsideMenuScreen(this, MenuItem.HorizontalAlign.Left, MenuItem.VerticalAlign.Top, 80);
            this.miPort.PositionFrom(this.miIP, MenuItem.HorizontalAlign.Right, MenuItem.VerticalAlign.CopyTop, 10);
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
            this.screenManager.Graphics.DrawMultiLineString(this.errorMessages, new Vector2(0, 50), 30);
            this.screenManager.Graphics.FontGeometry.Color = Color.White;
        }
    }
}
