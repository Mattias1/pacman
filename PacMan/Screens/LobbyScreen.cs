using System;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    class LobbyScreen : FavMenuScreen
    {
        #region Menu constants

        public const string OPEN = "Open";
        public const string CLOSED = "Closed";
        public const string OCCUPY = "Occupy";
        public const string NONE = "None";

        public static List<string> ConstList
        {
            get
            {
                return new List<string>() {
                    LobbyScreen.NONE, LobbyScreen.OPEN, LobbyScreen.CLOSED, LobbyScreen.OCCUPY 
                };
            }
        }

        #endregion

        BackgroundLevel bgLevel;
        MenuItem miBack;
        MenuItemList miPacMan;
        MenuItemList miMsPacMan;
        MenuItemList[] miGhosts;
        MultiPlayerInfo multiplayerInfo;
        string mapFile;
        string errorMessages;
        public bool MultiPlayer
        {
            get { return this.multiplayerInfo != null; }
        }

        public LobbyScreen(ScreenManager sM, BackgroundLevel bgLevel = null, MultiPlayerInfo mpInfo = null)
            : base(sM)
        {
            // Some settings
            if (bgLevel == null)
                this.bgLevel = new BackgroundLevel(this.screenManager);
            else
                this.bgLevel = bgLevel;
            this.errorMessages = "";
            this.multiplayerInfo = mpInfo;
            this.MenuSize = this.screenManager.ScreenResolution - 150 * Vector2.UnitX;
            this.NrOfColumns = 2;
            this.PositionMenuScreen(MenuItem.HorizontalAlign.Center, MenuItem.VerticalAlign.Top);
            this.FavMenuItem.TextAlign = MenuItem.HorizontalAlign.Left;
            this.HorizontalAndVerticalArrowKeys = true;

            // Start and back buttons
            MenuItem miStart = new MenuItem("Start", sM.Graphics);
            if (!this.MultiPlayer || this.multiplayerInfo.IsHost())
                miStart.OnClick += (o, e) => { this.Start(); };
            this.AddMenuItem(miStart);
            this.miBack = new MenuItem("Back", sM.Graphics);
            this.miBack.OnClick += (o, e) => { this.Remove(); };
            this.AddMenuItem(this.miBack, 1);

            // The player dropdowns
            List<string> options = new List<string>() { Settings.Get.Name, "Stupid", "Normal", "Euclidian", NONE };
            this.miPacMan = new MenuItemList("PacMan", new List<string>(options), sM.Graphics);
            this.miMsPacMan = new MenuItemList("Ms PacMan", new List<string>(options), sM.Graphics);
            this.miGhosts = new MenuItemList[4];
            for (int i = 0; i < this.miGhosts.Length; i++) {
                this.miGhosts[i] = new MenuItemList(Ghost.GetName(i), new List<string>(options), sM.Graphics);
            }
            if (this.MultiPlayer) {
                this.managePlayersMP();
            }
            this.finishCreatingMenus();

            // The maps
            MenuItem[] miMaps = new MenuItem[6];
            for (int i = 0; i < miMaps.Length; i++) {
                miMaps[i] = new MenuItem("Map " + i.ToString(), sM.Graphics);
                miMaps[i].OnClick += generateSetMap(i);
                this.AddMenuItem(miMaps[i], 1);
            }

            // Colour the menus (and position them)
            this.FastLayout(this.FavMenuItem);
            miBack.TextAlign = MenuItem.HorizontalAlign.Right;

            // Set the PacMan and ghost colours
            this.miPacMan.SelectedColour = Color.Yellow;
            this.miMsPacMan.SelectedColour = Color.Yellow;
            this.miGhosts[0].SelectedColour = Color.Red;
            this.miGhosts[1].SelectedColour = Color.DeepPink;
            this.miGhosts[2].SelectedColour = Color.Cyan;
            this.miGhosts[3].SelectedColour = Color.Orange;

            // Position the menus (override previous positioning)
            this.positionMenuItems(miStart, miMaps);

            // Load the values
            this.Load();

            // Subscribe to the multiplayer info events
            if (this.MultiPlayer)
                this.multiplayerInfo.OnRefresh += (o, e) => { this.managePlayersMP(); };
        }

        public void Load()
        {
            if (!this.MultiPlayer) {
                int[] players = Settings.Get.Players;
                this.miPacMan.SelectedIndex = players[0];
                this.miMsPacMan.SelectedIndex = players[1];
                for (int i = 0; i < this.miGhosts.Length; i++)
                    miGhosts[i].SelectedIndex = players[i + 2];
            }
            this.mapFile = Settings.Get.Map;
        }
        public void Save()
        {
            if (!this.MultiPlayer) {
                int[] players = new int[6];
                players[0] = this.miPacMan.SelectedIndex;
                players[1] = this.miMsPacMan.SelectedIndex;
                for (int i = 0; i < this.miGhosts.Length; i++)
                    players[i + 2] = miGhosts[i].SelectedIndex;
                Settings.Get.Players = players;
            }
            Settings.Get.Map = this.mapFile;
        }
        public void Start()
        {
            this.errorMessages = "";

            // Only allow one player
            int playerCount = this.miPacMan.SelectedValue == Settings.Get.Name ? 1 : 0;
            playerCount += this.miMsPacMan.SelectedValue == Settings.Get.Name ? 1 : 0;
            foreach (MenuItemList miGhost in this.miGhosts)
                playerCount += miGhost.SelectedValue == Settings.Get.Name ? 1 : 0;
            if (playerCount < 1)
                this.errorMessages = "At least one player is nescessary.";
            if (playerCount > 1)
                this.errorMessages = "Only one player is allowed.";

            // Force the presence of at least one PacMan
            if (this.notSelected(this.miPacMan) && this.notSelected(this.miMsPacMan))
                this.errorMessages = "Either mr of ms PacMan is required.";

            // Currently disallow ms PacMan
            if (this.notSelected(this.miMsPacMan))
                this.errorMessages = "Ms PacMan is currently not allowed.";

            // Don't continue if there are errors
            if (this.errorMessages != "")
                return;

            // Save the ini file
            this.Save();

            // Start the level
            if (this.MultiPlayer && this.multiplayerInfo.IsHost())
                this.multiplayerInfo.StartGame();
            Settings.Get.Lives = this.MultiPlayer ? 1 : 2;
            if (this.MultiPlayer)
                this.Remove();
            this.screenManager.AddScreen(new GameScreen(this.screenManager, this.mapFile, this.createAIs(), this.multiplayerInfo));
        }

        public void SetMap(int nr)
        {
            this.setMap("data/levels/level" + (nr + 1).ToString() + ".bmp");
        }
        private void setMap(string file)
        {
            // Set the map
            this.mapFile = file;
            this.screenManager.Graphics.SetDefaultCamera();
            this.bgLevel = new BackgroundLevel(this.screenManager, this.mapFile);
            // Choose a random colour for the walls
            this.bgLevel.RandomizeMapColor();
        }

        private void finishCreatingMenus()
        {
            for (int i = 0; i < Level.MAXPLAYERCOUNT; i++) {
                MenuItemList mi = this.getMenu(i);
                if (this.MultiPlayer)
                    mi.OnDeselected += this.generateMenuLeave(i);
                this.AddMenuItem(mi);
            }
        }
        private void managePlayersMP()
        {
            // Manage the values of the menus for the multiplayer lobby
            if (!this.MultiPlayer)
                return;
            if (this.multiplayerInfo.Error)
                this.Remove();

            // Manage the menus for the host
            if (this.multiplayerInfo.IsHost()) {
                // Set all empty menu's
                for (int i = 0; i < Level.MAXPLAYERCOUNT; i++)
                    this.getMenu(i).Values = new List<string>() { OPEN, CLOSED, OCCUPY };
                // Set all occupied menu's
                foreach (UserInfo user in this.multiplayerInfo.Users)
                    if (user.Id != -1)
                        this.getMenu(user.Id).Values = new List<string>() { user.Name, OPEN, CLOSED, OCCUPY };
                // Set my menu
                if (this.multiplayerInfo.Me.Id != -1)
                    this.getMenu(this.multiplayerInfo.Me.Id).Values = new List<string>() { this.multiplayerInfo.Me.Name, OPEN, CLOSED };
            }

            // Manage the menus for the client
            else {
                // Set all empty menu's
                for (int i = 0; i < Level.MAXPLAYERCOUNT; i++)
                    this.getMenu(i).Values = new List<string>() { OPEN, OCCUPY };
                // Set all occupied menu's
                foreach (UserInfo user in this.multiplayerInfo.Users)
                    if (user.Id != -1)
                        this.getMenu(user.Id).Values = new List<string>() { user.Name };
                // Set my menu
                if (this.multiplayerInfo.Me.Id != -1)
                    this.getMenu(this.multiplayerInfo.Me.Id).Values = new List<string>() { this.multiplayerInfo.Me.Name, OPEN };
            }
        }
        private MenuItemList getMenu(int i)
        {
            if (i == -1)
                return null;
            if (i == 0)
                return this.miPacMan;
            else if (i == 1)
                return this.miMsPacMan;
            else
                return this.miGhosts[i - 2];
        }
        bool notSelected(MenuItemList mi)
        {
            return mi.SelectedValue != NONE && mi.SelectedValue != OPEN;
        }

        private EventHandler generateSetMap(int i)
        {
            return (o, e) => {
                if (this.MultiPlayer && !this.multiplayerInfo.IsHost())
                    return;
                this.SetMap(i);
                if (this.MultiPlayer)
                    this.multiplayerInfo.ChangeMap(this.mapFile);
            };
        }
        private EventHandler generateMenuLeave(int i)
        {
            return (o, e) => {
                MenuItemList mi = this.getMenu(i);
                // Occupy
                if (mi.SelectedValue == OCCUPY) {
                    this.errorMessages = "This position is already occupied.";
                    if (this.multiplayerInfo.TakePosition(i))
                        this.errorMessages = "";
                }
                // Open
                if (mi.SelectedValue == OPEN) {
                    this.errorMessages = "You cannot open this position.";
                    if (this.multiplayerInfo.EmptyPosition(i))
                        this.errorMessages = "";
                }
                // Closed
                if (mi.SelectedValue == CLOSED) {
                    this.errorMessages = "Currently, you cannot close a position.";
                    if (this.multiplayerInfo.ClosePosition(i))
                        this.errorMessages = "";
                }
            };
        }

        private AI[] createAIs()
        {
            AI[] ais = new AI[Level.MAXPLAYERCOUNT];
            ais[0] = this.createAI(this.miPacMan, true);
            ais[1] = this.createAI(this.miMsPacMan, true);
            for (int i = 2; i < ais.Length; i++)
                ais[i] = this.createAI(this.miGhosts[i - 2]);
            return ais;
        }
        private AI createAI(MenuItemList mi, bool pacMan = false)
        {
            // Single player
            if (!this.MultiPlayer) {
                if (mi.SelectedIndex == 0)
                    return new AIPlayer();
                if (mi.SelectedIndex == 1)
                    return new AIRandom();
                if (mi.SelectedIndex == 2) {
                    if (pacMan)
                        return new AISimplePacMan();
                    else
                        return new AISimpleGhost();
                }
                if (mi.SelectedIndex == 3) {
                    if (pacMan)
                        return new AIEuclidianPacMan();
                    else
                        return new AIEuclidianGhost();
                }
                return null;
            }

            // Multi player
            if (mi.SelectedValue == Settings.Get.Name) {
                if (this.multiplayerInfo.IsHost())
                    return new AIServer(this.multiplayerInfo.Server, this.multiplayerInfo);
                else
                    return new AIClient(this.multiplayerInfo.Client, this.multiplayerInfo);
            }
            if (LobbyScreen.ConstList.Contains(mi.SelectedValue))
                return null;
            return new AISim();
        }

        private void positionMenuItems(MenuItem miStart, MenuItem[] miMaps)
        {
            // The player dropdowns
            this.miPacMan.PositionInsideMenuScreen(this, MenuItem.HorizontalAlign.Left, MenuItem.VerticalAlign.Top, 80);
            this.miMsPacMan.PositionFrom(this.miPacMan, MenuItem.HorizontalAlign.CopyLeft, MenuItem.VerticalAlign.Bottom, 10);
            this.miGhosts[0].PositionFrom(this.miMsPacMan, MenuItem.HorizontalAlign.CopyLeft, MenuItem.VerticalAlign.Bottom, 10);
            for (int i = 1; i < this.miGhosts.Length; i++)
                this.miGhosts[i].PositionFrom(this.miGhosts[i - 1], MenuItem.HorizontalAlign.CopyLeft, MenuItem.VerticalAlign.Bottom, 10);

            // The maps
            miMaps[0].PositionInsideMenuScreen(this, MenuItem.HorizontalAlign.Right, MenuItem.VerticalAlign.Top, 80);
            for (int i = 1; i < miMaps.Length; i++)
                miMaps[i].PositionFrom(miMaps[i - 1], MenuItem.HorizontalAlign.CopyLeft, MenuItem.VerticalAlign.Bottom, 10);

            // The start and back buttons
            miStart.PositionInsideMenuScreen(this, MenuItem.HorizontalAlign.Left, MenuItem.VerticalAlign.Bottom, 80);
            this.miBack.PositionInsideMenuScreen(this, MenuItem.HorizontalAlign.Right, MenuItem.VerticalAlign.Bottom, 80);
        }

        public override void OnEscapePress()
        {
            this.SelectMenuItem(this.miBack);
        }

        public override bool OnRemove()
        {
            if (this.MultiPlayer)
                this.multiplayerInfo.Abort();
            return base.OnRemove();
        }

        public override void Update(UpdateEventArgs e)
        {
            if (this.MultiPlayer) {
                // Change the map
                if (this.mapFile != Settings.Get.Map) {
                    this.setMap(Settings.Get.Map);
                }

                // Start game (if the host started)
                if (this.multiplayerInfo.Started) {
                    this.managePlayersMP();
                    this.Start();
                }
            }

            base.Update(e);
        }

        public override void Draw(UpdateEventArgs e)
        {
            this.bgLevel.Draw(e);

            base.Draw(e);

            this.drawVersion();

            this.screenManager.Graphics.FontGeometry.Color = Color.Red;
            this.screenManager.Graphics.DrawMultiLineString(this.errorMessages, 50 * Vector2.UnitY, 30);
            this.screenManager.Graphics.FontGeometry.Color = Color.White;
        }
    }
}
