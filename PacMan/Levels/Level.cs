using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using amulware.Graphics;

namespace PacMan
{
    public class Level : GameObject
    {
        #region Grid constants

        public const int BLOCKED = 0;
        public const int WALL = 1;
        public const int GHOSTSPAWN = 2;
        public const int PACMANSPAWN = 3;
        public const int PICKUPSPAWN = 4;
        public const int EMPTY = 5;
        public const int ORB = 6;
        public const int SUPERORB = 7;
        public const int CHERRY = 8;
        public const int STRAWBERRY = 9;

        public const float SQUARESIZE = 2f;
        public const float WALLHEIGHT = 1.9f;
        public const float WALLSIZE = 1.2f;
        public const float INV_SQUARESIZE = 1f / SQUARESIZE;

        #endregion

        #region Fields

        public const int MAXPLAYERCOUNT = 6;
        public int PlayerCount
        {
            get { return this.Ais.Length; }
        }

        public enum States { Starting, Running, Paused, Won, Lost }
        public enum DirectionKeys { Up, Right, Down, Left, None }
        public int TransFormKey { get; private set; }
        public States State { get; private set; }
        public DirectionKeys LastDirectionKey;
        public ScreenManager ScreenManager { get; private set; }
        public Graphics Graphics { get; private set; }
        public Camera Camera
        {
            get { return this.Graphics.Camera; }
            set { this.Graphics.Camera = value; }
        }
        public Vector2 PreferredCameraFocus { get; private set; }
        public float TimeLeftInS { get; private set; }
        public bool DrawText;
        private ParticleSystem particleSystem;
        private int maxPickUpCounter;
        public AI[] Ais { get; private set; }
        private MultiPlayerInfo multiplayerInfo;
        public bool MultiPlayer
        {
            get { return this.multiplayerInfo != null; }
        }

        public CameraGameObject Player;
        public PacMan PacMan;
        public PacMan MsPacMan = null;
        public List<Ghost> Ghosts;
        public List<PickupObject> PickupObjects; // They have to be in sorted order (sorted on Y, X)!
        public int[,] Grid = null;
        public int Width { get; private set; }
        public int Height { get; private set; }

        #endregion

        public Level(ScreenManager sM, string map, AI[] ais, MultiPlayerInfo mpInfo = null)
            : base(Vector2.Zero, Vector2.Zero)
        {
            this.ScreenManager = sM;
            this.particleSystem = new ParticleSystem();
            this.DrawText = true;
            this.Graphics = sM.Graphics;
            if (ais.Length != Level.MAXPLAYERCOUNT)
                throw new Exception(String.Format("There should be {0} AIs", Level.MAXPLAYERCOUNT));
            this.Ais = ais;
            if (map == "")
                map = Settings.Get.Map;
            this.loadGrid(map);
            this.CreateMesh();
            this.multiplayerInfo = mpInfo;
            this.Initialize();
        }

        public virtual void Initialize()
        {
            // Create the list objects
            this.Ghosts = new List<Ghost>();
            this.PickupObjects = new List<PickupObject>();

            // Spawn all gameobjects
            this.maxPickUpCounter = 0;
            for (int y = 0; y < this.Height; y++)
                for (int x = 0; x < this.Width; x++) {
                    Vector2 pos = this.GridToWorld(x, y);
                    if (this.UnsafeIsPacManSpawn(x, y)) {
                        this.PacMan = new PacMan(pos, this);
                        this.PacMan.AI = this.setAI(0, this.PacMan);
                    }
                    else if (this.UnsafeIsGhostSpawn(x, y)) {
                        int i = this.Ghosts.Count;
                        if (Ais[i + 2] != null) {
                            this.Ghosts.Add(new Ghost(pos, this, i));
                            this.Ghosts[i].AI = this.setAI(i + 2, this.Ghosts[i]);
                        }
                        else {
                            this.Ghosts.Add(null);
                        }
                    }
                    else if (this.UnsafeIsOrb(x, y)) {
                        this.PickupObjects.Add(new PickupObject(ORB, pos, this));
                        this.maxPickUpCounter++;
                    }
                    else if (this.UnsafeIsSuperOrb(x, y)) {
                        this.PickupObjects.Add(new PickupObject(SUPERORB, pos, this));
                        this.maxPickUpCounter++;
                    }
                    else if (this.UnsafeIsPickupSpawn(x, y)) {
                        this.maxPickUpCounter++;
                    }
                }

            // Some default settings
            this.Start();
            this.LastDirectionKey = DirectionKeys.None;
            this.setPreferredCameraFocus();
            this.Graphics.SetDefaultCamera();
            this.Player.UpdateCamera();
        }

        public void RandomizeMapColor(bool firstRun = false)
        {
            Matrix4 color = this.Graphics.Blue;

            for (int i = 0; i < 50; i++) {
                int rand = GlobalRandom.Next(5);

                color = this.Graphics.Blue;
                if (rand == 1)
                    color = this.Graphics.Green;
                if (rand == 2)
                    color = this.Graphics.Purple;
                if (rand == 3)
                    color = this.Graphics.Orange;
                if (rand == 4)
                    color = this.Graphics.Red;

                if (!firstRun && color != this.Graphics.WallColorUniform.Matrix)
                    break;
            }

            this.Graphics.WallColorUniform.Matrix = color;
        }

        public virtual void ResetPositions()
        {
            // Set the correct AI's
            if (this.MultiPlayer) {
                foreach (UserInfo user in this.multiplayerInfo.Users)
                    this.GetGameObject(user.Id).AI = this.setAI(user.Id, this.GetGameObject(user.Id));
            }

            // Set the game state
            this.Start();
            this.LastDirectionKey = DirectionKeys.None;

            // Reposition PacMan and the ghosts
            int ghostCount = 0;
            for (int y = 0; y < this.Height; y++)
                for (int x = 0; x < this.Width; x++) {
                    if (this.UnsafeIsPacManSpawn(x, y)) {
                        this.PacMan.Position = this.GridToWorld(x, y);
                        this.PacMan.Velocity = Vector2.Zero;
                    }
                    if (this.UnsafeIsGhostSpawn(x, y)) {
                        if (this.Ghosts[ghostCount] != null) {
                            this.Ghosts[ghostCount].Position = this.GridToWorld(x, y);
                            this.Ghosts[ghostCount].Velocity = Vector2.Zero;
                        }
                        ghostCount++;
                    }
                }

            // Update the camera
            this.Graphics.SetDefaultCamera();
            this.Player.UpdateCamera();
        }

        protected void setPreferredCameraFocus()
        {
            this.PreferredCameraFocus = 0.33f * (this.GridToWorld(0, 0) + this.GridToWorld(this.Width, this.Height)
                + this.GridToWorld((int)(0.2f * this.Width), this.Height));
        }

        public PickupObject GetPickupObject(Vector2 v)
        {
            return this.GetPickupObject((int)v.X, (int)v.Y);
        }
        public PickupObject GetPickupObject(int x, int y)
        {
            // They are added in sorted order (y, x), so we can do a binary search
            int min = 0, max = this.PickupObjects.Count, mid = 0;
            while (max - min > 0) {
                mid = (min + max) / 2;
                var obj = this.PickupObjects[mid];
                if (obj.GridPosition.Y < y || (obj.GridPosition.Y == y && obj.GridPosition.X < x)) {
                    if (max - min == 1)
                        break;
                    min = mid;
                    continue;
                }
                if (obj.GridPosition.Y > y || (obj.GridPosition.Y == y && obj.GridPosition.X > x)) {
                    max = mid;
                    continue;
                }
                return obj;
            }
            return null;
        }

        public Vector2 GetDirFromDirectionKey()
        {
            return this.GetDirFromDirectionKey(this.LastDirectionKey);
        }
        public Vector2 GetDirFromDirectionKey(DirectionKeys key)
        {
            switch (key) {
            case DirectionKeys.Up:
                return -Vector2.UnitY;
            case DirectionKeys.Down:
                return Vector2.UnitY;
            case DirectionKeys.Right:
                return Vector2.UnitX;
            case DirectionKeys.Left:
                return -Vector2.UnitX;
            default:
                return Vector2.Zero;
            }
        }

        public void ResetDirectionKeyFromDir(Vector2 dir)
        {
            foreach (DirectionKeys d in (DirectionKeys[])Enum.GetValues(typeof(DirectionKeys))) {
                this.LastDirectionKey = d;
                if (this.GetDirFromDirectionKey() == dir)
                    break;
            }
        }
        public void ClearDirectionKey()
        {
            this.LastDirectionKey = DirectionKeys.None;
        }

        private DirectionKeys transformDirKey(DirectionKeys key)
        {
            if (key == DirectionKeys.None)
                return DirectionKeys.None;
            return (DirectionKeys)(((int)key + this.TransFormKey) % 4);
        }
        private void calculateTransformDirKey()
        {
            Vector3 eye2focus = this.Camera.Focus - this.Camera.Eye;
            float angle = GameMath.Angle(new Vector2(eye2focus.X, eye2focus.Z)) + MathHelper.PiOver2;
            if (angle < 0)
                angle += MathHelper.TwoPi;
            this.TransFormKey = (int)(angle / MathHelper.PiOver2 + 0.5f);
        }

        public int Score(int type)
        {
            if (type == ORB)
                return 10;
            if (type == SUPERORB)
                return 25;
            if (type == GHOSTSPAWN)
                return 500;
            if (type == PACMANSPAWN)
                return 1000;
            return 0;
        }

        public override void Update(UpdateEventArgs e)
        {
            // Update the timer
            if (this.TimeLeftInS > 0) {
                this.TimeLeftInS -= e.ElapsedTimeInSf;
                if (this.TimeLeftInS == 0)
                    this.TimeLeftInS = -0.1f;
            }

            // Update the particle system
            this.particleSystem.Update(e);

            // Manage the camera
            if (InputManager.Keys.Down(Key.W))
                this.Graphics.Camera.ChangeDistance(-15f * e.ElapsedTimeInSf);
            if (InputManager.Keys.Down(Key.S))
                this.Graphics.Camera.ChangeDistance(15f * e.ElapsedTimeInSf);
            if (InputManager.Keys.Down(Key.A)) {
                this.Graphics.Camera.RotateAroundFocusRad(-2f * e.ElapsedTimeInSf);
                this.calculateTransformDirKey();
            }
            if (InputManager.Keys.Down(Key.D)) {
                this.Graphics.Camera.RotateAroundFocusRad(2f * e.ElapsedTimeInSf);
                this.calculateTransformDirKey();
            }
            if (InputManager.Keys.Down(Key.R)) {
                this.Graphics.Camera = new Camera(this.Graphics, PacMan.CameraEye, Vector3.Zero, Vector3.UnitY, PacManProgram.WIDTH, PacManProgram.HEIGHT);
                this.TransFormKey = 0;
                this.Player.UpdateCamera();
            }

            // Update the keyboard directions
            if (InputManager.Keys.Hit(Key.Up))
                this.LastDirectionKey = transformDirKey(DirectionKeys.Up);
            if (InputManager.Keys.Hit(Key.Down))
                this.LastDirectionKey = transformDirKey(DirectionKeys.Down);
            if (InputManager.Keys.Hit(Key.Right))
                this.LastDirectionKey = transformDirKey(DirectionKeys.Right);
            if (InputManager.Keys.Hit(Key.Left))
                this.LastDirectionKey = transformDirKey(DirectionKeys.Left);

            // Cheat
            if (InputManager.Keys.Down(Key.C) && InputManager.Keys.Down(Key.ShiftLeft)) {
                if (this.PickupObjects.Count > 6)
                    this.PickupObjects.RemoveRange(0, this.PickupObjects.Count - 6);
            }

            if (this.State == States.Running) {
                if (InputManager.Keys.Hit(Key.P)) {
                    this.Pause();
                    InputManager.Update();
                }

                // Update the game objects
                if (this.PacMan != null)
                    this.PacMan.Update(e);
                if (this.MsPacMan != null)
                    this.MsPacMan.Update(e);
                foreach (Ghost ghost in this.Ghosts)
                    if (ghost != null)
                        ghost.Update(e);
            }

            if (this.State == States.Paused) {
                if (InputManager.Keys.Hit(Key.P)) {
                    this.Start();
                }
            }

            if (this.State == States.Starting) {
                // Set the correct rotation
                if (this.LastDirectionKey != DirectionKeys.None) {
                    Vector2 dir = this.GetDirFromDirectionKey();
                    if (this.IsMoveable(this.Player.GetGridPos() + dir))
                        this.Player.Angle = GameMath.Angle(dir);
                    else
                        this.ClearDirectionKey();
                }

                // Update the camera
                this.Player.UpdateCamera();

                // Start the game
                if (this.TimeLeftInS < 0) {
                    this.State = States.Running;
                    this.TimeLeftInS = 0;
                }
            }

            if (this.State == States.Won || this.State == States.Lost) {

            }
        }

        public override void Draw(UpdateEventArgs e)
        {
            this.PacMan.Draw(e);

            foreach (Ghost ghost in this.Ghosts)
                if (ghost != null)
                    ghost.Draw(e);

            foreach (PickupObject pickupObject in this.PickupObjects)
                pickupObject.Draw(e);

            this.Graphics.WallSurface.Render();

            this.drawAllText();

            this.particleSystem.Draw(e);
        }

        protected virtual void drawAllText()
        {
            if (!this.DrawText)
                return;

            // HUD
            Graphics g = this.Graphics;
            g.FontGeometry.Height = 30;
            if (this.Player == this.PacMan)
                g.DrawString(10, 10, "Lives: " + this.PacMan.Lives.ToString());
            g.DrawString(this.Player == this.PacMan ? 170 : 10, 10, "Score: " + this.Player.Score.ToString());

            // Game starting
            if (this.State == States.Starting) {
                int nr = (int)this.TimeLeftInS;
                g.DrawString(nr == 0 ? "Go!" : nr.ToString(), (int)((1 - this.TimeLeftInS + nr) * 120));
            }

            // Game paused
            g.FontGeometry.Height = 60;
            if (this.State == States.Paused) {
                g.DrawString("Paused");
            }

            // Game over
            else if (this.State == States.Won) {
                g.DrawString("Congratulations!");
            }
            else if (this.State == States.Lost) {
                g.DrawString("Too bad, you lost the game!");
            }
            if (this.GameOver()) {
                g.DrawString("Press Enter to end.", new Vector2(0, g.FontGeometry.Height), 40);
            }
        }

        public void AddScoreParticle(CameraGameObject player, int score, Vector2 position, bool moveText = false, amulware.Graphics.Color? color = null)
        {
            if (!player.IsPlayer())
                return;
            Vector2 offset = Vector2.Zero;
            float awesomeFactor = score / 900f + 0.5f;
            if (moveText) {
                RandomVector2 rv = new RandomVector2(1);
                offset = rv.Create2() * awesomeFactor;
            }
            if (!color.HasValue)
                color = amulware.Graphics.Color.White;
            this.particleSystem.Add(new ScoreText(score, position + offset, this.Graphics, awesomeFactor, color.Value));
        }

        #region State methods

        public void Start()
        {
            this.State = States.Starting;
            this.TimeLeftInS = 0.99f; // 3.99f;
        }

        public void Pause()
        {
            this.State = States.Paused;
        }

        public void Win()
        {
            this.State = States.Won;
            this.TimeLeftInS = 0.99f;
        }

        public void Lose()
        {
            this.State = States.Lost;
            this.TimeLeftInS = 0.99f;
        }

        public void DoLose(Ghost killer)
        {
            // Multiplayer
            if (this.MultiPlayer) {
                // The client should just wait untill the server informs him
                if (!this.multiplayerInfo.IsHost())
                    return;

                // The host should switch players and restart, untill the condition is met
                for (int g = 0; g < this.Ghosts.Count; g++)
                    if (this.Ghosts[g] == killer) {
                        // Swap players
                        if (this.PacMan.Collides(this.Ghosts[g]))
                            this.multiplayerInfo.SwapUsersCommand(0, g + 2, this.Ais);
                        else if (this.MsPacMan.Collides(this.Ghosts[g]))
                            this.multiplayerInfo.SwapUsersCommand(1, g + 2, this.Ais);
                        else
                            throw new Exception("The lose method is called, but both mr and ms pacman do not collide with the killer.");
                        // Restart
                        if (!this.CheckWinCondition(false)) {
                            this.PacMan.Lives = Settings.Get.Lives;
                            this.ResetPositions();
                        }
                        return;
                    }
                throw new Exception("The killer is not found.");
            }

            // Singleplayer
            if (this.Player == this.PacMan || this.Player == this.MsPacMan)
                this.Lose();
            else
                this.Win();
        }

        private bool checkOrbsCleared(bool givePoints = true)
        {
            // If no orbs left, you won
            if (this.PickupObjects.Count <= 5) {
                for (int i = 0; i < this.PickupObjects.Count; i++) {
                    if (this.PickupObjects[i].Type == ORB)
                        return false;
                }
                if (givePoints)
                    this.PacMan.Score += 1000;
                return true;
            }
            return false;
        }

        public bool GameOver()
        {
            return this.TimeLeftInS < 0 && (this.State == States.Won || this.State == States.Lost);
        }

        public virtual bool CheckWinCondition(bool givePoints = true)
        {
            // Multiplayer (possibly) has multiple win conditions
            if (this.MultiPlayer) {
                // Currently, the first one to clear the screen wins
                bool ended = this.checkOrbsCleared(givePoints);
                if (ended) {
                    if (this.Player == this.PacMan)
                        this.Win();
                    else
                        this.Lose();
                }
                else if (this.checkOrbsCleared(givePoints)) {
                    if (this.multiplayerInfo.IsHost())
                        this.multiplayerInfo.SwapUsersCommand(0, 0, this.Ais);
                    this.ResetPositions();
                }
                return ended;
            }

            // Singleplayer has only the orbs win condition
            if (this.checkOrbsCleared()) {
                if (this.Player == this.PacMan)
                    this.Win();
                else
                    this.Lose();
                return true;
            }
            return false;
        }

        #endregion

        #region Grid methods

        public bool UnsafeIsBlocked(int x, int y)
        {
            return this.Grid[x, y] == BLOCKED;
        }
        public bool UnsafeIsWall(int x, int y)
        {
            return this.Grid[x, y] == WALL;
        }
        public bool UnsafeIsGhostSpawn(Vector2 v)
        {
            return this.UnsafeIsGhostSpawn((int)v.X, (int)v.Y);
        }
        public bool UnsafeIsGhostSpawn(int x, int y)
        {
            return this.Grid[x, y] == GHOSTSPAWN;
        }
        public bool UnsafeIsPacManSpawn(int x, int y)
        {
            return this.Grid[x, y] == PACMANSPAWN;
        }
        public bool UnsafeIsEmpty(int x, int y)
        {
            return this.Grid[x, y] == EMPTY;
        }
        public bool UnsafeIsOrb(int x, int y)
        {
            return this.Grid[x, y] == ORB;
        }
        public bool UnsafeIsSuperOrb(int x, int y)
        {
            return this.Grid[x, y] == SUPERORB;
        }
        public bool UnsafeIsPickupSpawn(int x, int y)
        {
            return this.Grid[x, y] == PICKUPSPAWN;
        }
        public bool UnsafeIsPickUpObject(int x, int y)
        {
            return this.Grid[x, y] == ORB || this.Grid[x, y] == SUPERORB || this.Grid[x, y] == PICKUPSPAWN;
        }

        public bool UnsafeIsNotMoveable(int x, int y)
        {
            return this.UnsafeIsBlocked(x, y) || this.UnsafeIsWall(x, y);
        }
        public bool UnsafeIsMoveable(int x, int y)
        {
            return !this.UnsafeIsNotMoveable(x, y);
        }

        public bool IsBlocked(int x, int y)
        {
            if (this.InRange(x, y))
                return this.UnsafeIsBlocked(x, y);
            return false;
        }
        public bool IsWall(int x, int y)
        {
            if (this.InRange(x, y))
                return this.UnsafeIsWall(x, y);
            return false;
        }
        public bool IsGhostSpawn(Vector2 v)
        {
            return this.IsGhostSpawn((int)v.X, (int)v.Y);
        }
        public bool IsGhostSpawn(int x, int y)
        {
            if (this.InRange(x, y))
                return this.UnsafeIsGhostSpawn(x, y);
            return false;
        }
        public bool IsPacManSpawn(int x, int y)
        {
            if (this.InRange(x, y))
                return this.UnsafeIsPacManSpawn(x, y);
            return false;
        }
        public bool IsEmpty(int x, int y)
        {
            if (this.InRange(x, y))
                return this.UnsafeIsEmpty(x, y);
            return false;
        }
        public bool IsOrb(int x, int y)
        {
            if (this.InRange(x, y))
                return this.UnsafeIsOrb(x, y);
            return false;
        }
        public bool IsSuperOrb(int x, int y)
        {
            if (this.InRange(x, y))
                return this.UnsafeIsSuperOrb(x, y);
            return false;
        }
        public bool IsPickupSpawn(int x, int y)
        {
            if (this.InRange(x, y))
                return this.UnsafeIsPickupSpawn(x, y);
            return false;
        }
        public bool IsPickupObject(int x, int y)
        {
            if (this.InRange(x, y))
                return this.UnsafeIsPickUpObject(x, y);
            return false;
        }

        public bool IsNotMoveable(int x, int y)
        {
            if (this.InRange(x, y))
                return this.UnsafeIsNotMoveable(x, y);
            return true;
        }
        public bool IsNotMoveable(Vector2 v)
        {
            return this.IsNotMoveable((int)v.X, (int)v.Y);
        }
        public bool IsMoveable(int x, int y)
        {
            if (this.InRange(x, y))
                return this.UnsafeIsMoveable(x, y);
            return false;
        }
        public bool IsMoveable(Vector2 v)
        {
            return this.IsMoveable((int)v.X, (int)v.Y);
        }

        public bool InRange(Vector2 v)
        {
            return this.InRange((int)v.X, (int)v.Y);
        }
        public bool InRange(int x, int y)
        {
            return 0 <= x && x < this.Width && 0 <= y && y < this.Height;
        }

        /// <summary>
        /// Convert grid coordinates to world coordinates
        /// </summary>
        /// <param name="x">The grid x-coordinate, assumed to be integer.</param>
        /// <param name="y">The grid y-coordinate, assumed to be integer.</param>
        /// <returns>The world coordinates.</returns>
        public Vector2 GridToWorld(int x, int y)
        {
            return this.GridToWorld(new Vector2(x, y));
        }
        /// <summary>
        /// Convert grid coordinates to world coordinates
        /// </summary>
        /// <param name="v">The grid coordinates, assumed to be integers.</param>
        /// <returns>The world coordinates.</returns>
        public Vector2 GridToWorld(Vector2 v)
        {
            return SQUARESIZE * v;
        }
        public Vector2 WorldToGrid(Vector2 v)
        {
            return GameMath.Round(INV_SQUARESIZE * v);
        }

        #endregion

        #region Loading methods

        Dictionary<System.Drawing.Color, int> colorDict = new Dictionary<System.Drawing.Color, int>() {
            { rgb(255, 255, 255), BLOCKED },
            { rgb(0, 0, 255), WALL },
            { rgb(255, 0, 0), GHOSTSPAWN },
            { rgb(255, 255, 0), PACMANSPAWN },
            { rgb(0, 255, 0), PICKUPSPAWN },
            { rgb(0, 0, 0), EMPTY },
            { rgb(128, 128, 128), ORB },
            { rgb(64, 64, 64), SUPERORB }
        };

        static System.Drawing.Color rgb(int r, int g, int b)
        {
            return System.Drawing.Color.FromArgb(r, g, b);
        }

        void loadGrid(string levelImg)
        {
            try {
                Bitmap bmp = (Bitmap)Image.FromFile(levelImg);
                this.Width = bmp.Width;
                this.Height = bmp.Height;
                this.Grid = new int[this.Width, this.Height];
                for (int y = 0; y < this.Height; y++)
                    for (int x = 0; x < this.Width; x++) {
                        System.Drawing.Color c = bmp.GetPixel(x, y);
                        this.Grid[x, y] = this.colorDict[c];
                    }
                bmp.Dispose();
            }
            catch {
                throw new NotImplementedException("Error catching not implemented, lolz");
            }
        }

        public void CreateMesh()
        {
            MeshData mesh = new MeshData();
            bool wall = false;

            // Horizontal walls
            for (int y = 0; y < this.Height; y++) {
                for (int x = 0; x < this.Width; x++) {
                    if (this.UnsafeIsWall(x, y)) {
                        if (!wall) {
                            if (!this.IsWall(x, y - 1) || !this.IsWall(x, y + 1)) {
                                this.createWallSegment(mesh, x, y, true);
                                wall = true;
                            }
                        }
                    }
                    else {
                        wall = false;
                    }
                }
                wall = false;
            }

            // Vertical walls
            for (int x = 0; x < this.Width; x++) {
                for (int y = 0; y < this.Height; y++) {
                    if (this.UnsafeIsWall(x, y)) {
                        if (!wall) {
                            if (!this.IsWall(x - 1, y) || !this.IsWall(x + 1, y)) {
                                this.createWallSegment(mesh, x, y, false);
                                wall = true;
                            }
                        }
                    }
                    else {
                        wall = false;
                    }
                }
                wall = false;
            }

            // Create the surface
            this.Graphics.CreateWallSurface(mesh);
        }

        void createWallSegment(MeshData mesh, int startX, int startY, bool horizontal)
        {
            float halfSize = WALLSIZE * 0.5f;
            float topWall = WALLHEIGHT - SQUARESIZE * 0.5f;

            // Possibly go one step back
            if (this.IsWall(startX - (horizontal ? 1 : 0), startY - (horizontal ? 0 : 1))) {
                startX -= (horizontal ? 1 : 0);
                startY -= (horizontal ? 0 : 1);
            }

            // Find begin and end grid coordinates
            int endX = startX;
            int endY = startY;
            while (this.IsWall(endX, endY)) {
                if (horizontal)
                    endX++;
                else
                    endY++;
            }
            // Correct for the extra count
            if (horizontal)
                endX--;
            else
                endY--;
            // Don't create single blocks
            if (startX == endX && startY == endY)
                return;

            // Calculate corners (world coordinates)
            Vector2[] corners = new Vector2[4];
            corners[0] = new Vector2(SQUARESIZE * startX - halfSize, SQUARESIZE * startY - halfSize); // NW
            corners[1] = new Vector2(SQUARESIZE * endX + halfSize, SQUARESIZE * startY - halfSize);   // NE
            corners[2] = new Vector2(SQUARESIZE * startX - halfSize, SQUARESIZE * endY + halfSize);   // SW
            corners[3] = new Vector2(SQUARESIZE * endX + halfSize, SQUARESIZE * endY + halfSize);     // SE

            // Add to mesh
            mesh.AddWallSegment(corners, SQUARESIZE * 0.5f, topWall);
        }

        AI setAI(int id, CameraGameObject puppet)
        {
            AI ai = this.Ais[id];
            ai.Initialize(puppet);
            AI test = ai as AIPlayer;
            if (test != null)
                this.Player = puppet;
            return ai;
        }

        #endregion

        #region Network game data

        public string ToGameData()
        {
            string s = this.pickupsToString() + "|";
            if (this.PacMan != null)
                s += this.PacMan.ToGameData();
            s += "|";
            if (this.MsPacMan != null)
                s += this.MsPacMan.ToGameData();
            foreach (Ghost ghost in this.Ghosts) {
                s += "|";
                if (ghost != null)
                    s += ghost.ToGameData();
            }
            return s;
        }

        public void FromGameData(string data)
        {
            string[] s = data.Split(new char[] { '|' }, StringSplitOptions.None);
            this.pickupsFromString(s[0]);
            if (this.PacMan != null)
                this.PacMan.FromGameData(s[1]);
            if (this.MsPacMan != null)
                this.MsPacMan.FromGameData(s[2]);
            for (int i = 0; i < this.Ghosts.Count; i++)
                if (this.Ghosts[i] != null)
                    this.Ghosts[i].FromGameData(s[i + 3]);
        }

        protected string pickupsToString()
        {
            // <3
            byte[] b = new byte[(this.maxPickUpCounter >> 3) + 1];
            if (this.PickupObjects.Count == 0)
                return Convert.ToBase64String(b);
            int i = 0, j = 0, nr = 0;
            PickupObject obj = this.PickupObjects[nr];
            for (int y = 0; y < this.Height; y++)
                for (int x = 0; x < this.Width; x++)
                    if (this.UnsafeIsPickUpObject(x, y)) {
                        if ((int)obj.GridPosition.X == x && (int)obj.GridPosition.Y == y) {
                            b[i] |= (byte)(1 << j);
                            if (this.PickupObjects.Count <= ++nr) {
                                y = this.Height;
                                break;
                            }
                            obj = this.PickupObjects[nr];
                        }
                        if (++j == 8) {
                            j = 0;
                            i++;
                        }
                    }
            return Convert.ToBase64String(b);
        }

        protected void pickupsFromString(string data)
        {
            if (this.PickupObjects.Count == 0)
                return;
            byte[] b = Convert.FromBase64String(data);
            int i = 0, j = 0, nr = 0;
            PickupObject obj = this.PickupObjects[nr];
            for (int y = 0; y < this.Height; y++)
                for (int x = 0; x < this.Width; x++)
                    if (this.UnsafeIsPickUpObject(x, y)) {
                        bool objBeHere = (b[i] & (byte)(1 << j)) != 0;
                        if ((int)obj.GridPosition.X == x && (int)obj.GridPosition.Y == y) {
                            if (objBeHere) {
                                // It is as it should be, so go on to the next one
                                if (this.PickupObjects.Count <= ++nr) {
                                    y = this.Height;
                                    break;
                                }
                                obj = this.PickupObjects[nr];
                            }
                            else {
                                // The object should be gone
                                this.PickupObjects.RemoveAt(nr);
                                if (this.PickupObjects.Count <= nr) {
                                    y = this.Height;
                                    break;
                                }
                                obj = this.PickupObjects[nr];
                            }
                        }
                        else if (objBeHere) {
                            // There should be an object here, but it's not in the list
                            obj = null;
                            Vector2 pos = this.GridToWorld(x, y);
                            if (this.UnsafeIsOrb(x, y))
                                obj = new PickupObject(ORB, pos, this);
                            else if (this.UnsafeIsSuperOrb(x, y))
                                obj = new PickupObject(SUPERORB, pos, this);
                            else if (this.UnsafeIsPickupSpawn(x, y))
                                obj = null;
                            if (obj != null) {
                                this.PickupObjects.Insert(nr, obj);
                                obj = this.PickupObjects[++nr];
                            }
                            else {
                                obj = this.PickupObjects[nr];
                            }
                        }
                        if (++j == 8) {
                            j = 0;
                            i++;
                        }
                    }
        }

        public void UpdateSimAIs(string[] gameDatas)
        {
            for (int i = 0; i < gameDatas.Length; i++)
                if (gameDatas[i] != null) {
                    CameraGameObject cgo = this.GetGameObject(i);
                    if (!cgo.IsPlayer())
                        ((AISim)cgo.AI).DirectionVector = cgo.UnitVecFromInt(int.Parse(gameDatas[i]));
                }
        }

        public CameraGameObject GetGameObject(int i)
        {
            if (i == 0)
                return this.PacMan;
            if (i == 1)
                return this.MsPacMan;
            return this.Ghosts[i - 2];
        }

        #endregion
    }
}
