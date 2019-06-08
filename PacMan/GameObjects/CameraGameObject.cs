using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public abstract class CameraGameObject : GameObject
    {
        public Level Level;
        public Camera Camera
        {
            get { return this.Level.Graphics.Camera; }
            set { this.Level.Graphics.Camera = value; }
        }
        public IndexedSurface<NormalUVColorVertexData> Surface;
        public abstract float Speed { get; }
        public float Angle;
        public AI AI;

        private int score;
        public int Score
        {
            get { return this.score; }
            set
            {
                int diff = value - this.score;
                if (this.Level.Player == this)
                    this.Level.AddScoreParticle(this, Math.Abs(diff), this.Position, true, diff < 0 ? Color.Red : Color.White);
                this.score = Math.Max(0, value);
            }
        }

        public CameraGameObject(Vector2 p, Vector2 v, Level level, IndexedSurface<NormalUVColorVertexData> surface)
            : base(p, v)
        {
            this.Level = level;
            this.Surface = surface;
            this.AI = null;
        }

        public bool IsPlayer()
        {
            return this == this.Level.Player;
        }

        public override void Update(UpdateEventArgs e)
        {
            Vector2 oldPosition = this.Position;
            Vector2 oldVelocity = this.Velocity;

            // Update the AI
            this.AI.UpdateBeforeMove(e);

            // Move and turn
            bool turned = false;
            Vector2 gridCenter = this.GetGridCenter();
            Vector2 unitVec = Vector2.Zero;
            unitVec = this.AI.GetUpdateDir();
            // Turn back
            if (GameMath.OppositeDirections(unitVec, this.Velocity)) {
                this.Velocity *= -1;
                oldVelocity = this.Velocity;
                this.updateAngle();
                this.clearDirectionKey();
            }
            // Normal turn
            else if (unitVec != Vector2.Zero && (this.Velocity == Vector2.Zero || this.WillPassGridCenter(e, gridCenter))) {
                turned = this.moveTurn(e, gridCenter, unitVec);
            }

            // Do a normal move
            if (!turned)
                this.moveNormal(e);

            // Correct if the move was impossible
            gridCenter = this.GetGridCenter(); // We moved, remember?
            if (this.Velocity != Vector2.Zero && this.Level.IsNotMoveable(this.GetNextGridPos()) && this.IsPastGridCenter(gridCenter)) {
                // Restrict the position
                this.Position = gridCenter;
                this.Velocity = Vector2.Zero;

                // Manage teleports
                Vector2 nextGridPos = this.GetNextGridPos(oldVelocity);
                if (!this.Level.InRange(nextGridPos)) {
                    Vector2 gridPos = this.GetGridPos();
                    if (nextGridPos.X < 0 || nextGridPos.X >= this.Level.Width)
                        this.Position = this.Level.GridToWorld(new Vector2(this.Level.Width - 1 - gridPos.X, gridPos.Y));
                    else
                        this.Position = this.Level.GridToWorld(new Vector2(gridPos.X, this.Level.Height - 1 - gridPos.Y));
                    this.Velocity = oldVelocity;
                    this.updateAngle();
                }
                // If we turned, maybe the old move would be valid - if so, do that one.
                else if (turned && this.Level.IsMoveable(nextGridPos)) { // this.GetNextGridPos(oldVelocity))) {
                    this.Position = oldPosition;
                    this.Velocity = oldVelocity;
                    this.moveNormal(e);
                    this.updateAngle();
                    turned = false;
                    if (this.IsPlayer())
                        this.Level.ResetDirectionKeyFromDir(unitVec);
                }
            }

            if (this.IsPlayer()) {
                // Check if you bumped against a wall
                if (oldVelocity != Vector2.Zero && this.Velocity == Vector2.Zero) {
                    this.Level.Graphics.AddScreenshake(4, 0.7f, 0.05f);
                }

                // Update the camera
                this.UpdateCamera();
            }

            // Update the AI
            this.AI.UpdateAfterMove(e, unitVec);
        }

        protected void clearDirectionKey()
        {
            if (this.IsPlayer())
                this.Level.ClearDirectionKey();
        }

        protected bool moveTurn(UpdateEventArgs e, Vector2 gridCenter, Vector2 dir)
        {
            this.clearDirectionKey();

            // If it's standing still, set the correct velocity and let the Update method do the rest
            if (this.Velocity == Vector2.Zero) {
                this.Velocity = this.Speed * dir;
                this.updateAngle();
                return false;
            }

            // Calculate fraction after grid center
            float d = 1 - (gridCenter - this.Position).Length / this.Speed * e.ElapsedTimeInSf;

            // Set new velocity
            this.Velocity = this.Speed * dir;
            this.updateAngle();

            // Set the new position
            this.Position = gridCenter + this.Velocity * d * e.ElapsedTimeInSf;
            return true;
        }

        protected void moveNormal(UpdateEventArgs e)
        {
            base.Update(e);
        }

        protected void updateAngle()
        {
            this.Angle = GameMath.Angle(this.Velocity);
        }

        public virtual void UpdateCamera()
        {
            this.Camera.MoveFocusTo(new Vector3(this.Position.X, 0, this.Position.Y));
        }

        public override void Draw(UpdateEventArgs e)
        {
            this.setModelMatrix();
            this.Surface.Render();
        }

        protected abstract void setModelMatrix();

        #region Grid methods

        public Vector2 GetGridPos()
        {
            return this.Level.WorldToGrid(this.Position);
        }

        public Vector2 GetNextGridPosOffset()
        {
            return this.GetNextGridPosOffset(this.Velocity);
        }
        public Vector2 GetNextGridPosOffset(Vector2 velocity)
        {
            bool horizontal = velocity.Y == 0;
            if (horizontal)
                return new Vector2(velocity.X > 0 ? 1 : -1, 0);
            return new Vector2(0, velocity.Y > 0 ? 1 : -1);
        }

        public Vector2 GetNextGridPos()
        {
            return this.GetNextGridPos(this.Velocity);
        }
        public Vector2 GetNextGridPos(Vector2 velocity)
        {
            return this.GetGridPos() + this.GetNextGridPosOffset(velocity);
        }

        public Vector2 GetGridCenter()
        {
            return this.Level.GridToWorld(this.GetGridPos());
        }

        public bool WillPassGridCenter(UpdateEventArgs e)
        {
            return this.WillPassGridCenter(e, this.GetGridCenter());
        }
        public bool WillPassGridCenter(UpdateEventArgs e, Vector2 gridCenter)
        {
            return this.WillPassGridCenter(gridCenter - this.Position, gridCenter - (this.Position + this.Velocity * e.ElapsedTimeInSf));
        }
        public bool WillPassGridCenter(Vector2 dif, Vector2 nextDif)
        {
            return (dif.X > 0) != (nextDif.X > 0) || (dif.Y > 0) != (nextDif.Y > 0);
        }

        public bool IsPastGridCenter()
        {
            return this.IsPastGridCenter(this.GetGridCenter());
        }
        public bool IsPastGridCenter(Vector2 gridCenter)
        {
            return this.IsPastGridCenter(gridCenter - this.Position, gridCenter - (this.Position + this.Velocity));
        }
        public bool IsPastGridCenter(Vector2 dif, Vector2 nextDif)
        {
            // If you will pass the grid center next move, you are not past yet :)
            if (this.WillPassGridCenter(dif, nextDif))
                return false;

            // If both the position and the next position are at the same side of the center, check if the next position is further away
            return nextDif.LengthSquared > dif.LengthSquared;
        }

        #endregion

        #region Network game data

        public string ToGameData()
        {
            int velocityAndDir = this.UnitVecToString(this.Velocity) + 8 * this.UnitVecToString(this.Level.GetDirFromDirectionKey());
            string s = Settings.Vec2StrF(this.Position) + ";" + velocityAndDir.ToString() + ";" + this.Score.ToString();
            return s;
        }

        public void FromGameData(string data)
        {
            string[] s = data.Split(new char[] { ';' });
            this.Position = Settings.Str2VecF(s[0]);
            int velocityAndDir = int.Parse(s[1]);
            this.Score = int.Parse(s[2]);
            this.Velocity = this.Speed * this.UnitVecFromInt(velocityAndDir & 7);
            if (!this.IsPlayer())
                ((AISim)this.AI).DirectionVector = this.UnitVecFromInt(velocityAndDir >> 3);
        }

        public int UnitVecToString(Vector2 v)
        {
            if (v.Y > 0)    // N
                return 1;
            if (v.X > 0)    // E
                return 2;
            if (v.Y < 0)    // S
                return 3;
            if (v.X < 0)    // W
                return 4;
            return 0;       // Zero
        }

        public Vector2 UnitVecFromInt(int i)
        {
            if (i == 1)                 // N
                return Vector2.UnitY;
            if (i == 2)                 // E
                return Vector2.UnitX;
            if (i == 3)                 // S
                return -Vector2.UnitY;
            if (i == 4)                 // W
                return -Vector2.UnitX;
            return Vector2.Zero;        // Zero
        }

        #endregion
    }
}
