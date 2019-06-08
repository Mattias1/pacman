using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class PacMan : CameraGameObject
    {
        public const float SIZE = 2f;

        public int Lives;
        public static Vector3 CameraEye
        {
            get { return new Vector3(-6f, 20f, 24f); }
        }
        public IndexedSurface<NormalUVColorVertexData> MouthSurface;

        public override float Speed
        {
            get { return 7; }
        }

        public PacMan(Vector2 startingPos, Level level)
            : base(startingPos, Vector2.Zero, level, level.Graphics.PacmanSurface)
        {
            this.MouthSurface = level.Graphics.PacmanMouthSurface;
            this.Lives = Settings.Get.Lives;
            this.Velocity = Vector2.UnitX * this.Speed;
            this.updateAngle();
        }

        public void Die(Ghost killer)
        {
            killer.Score += this.Level.Score(Level.PACMANSPAWN);
            this.Lives--;
            if (this.Lives <= 0) {
                this.Score -= this.Level.Score(Level.PACMANSPAWN);
                this.Level.DoLose(killer);
            }
            else {
                this.Level.ResetPositions();
            }
        }

        public override void Update(UpdateEventArgs e)
        {
            base.Update(e);

            // Collide with pickup objects
            var pickupObject = this.Level.GetPickupObject(this.GetGridPos());
            if (pickupObject != null) {
                if (pickupObject.Type == Level.ORB) {
                    if (pickupObject.Remove()) {
                        this.Level.Graphics.Audio.PlayOmnomnom();
                        this.Score += this.Level.Score(pickupObject.Type);
                        if (this.Level.PickupObjects.Count <= 5)
                            this.Level.CheckWinCondition();
                    }
                }
                else if (pickupObject.Type == Level.SUPERORB) {
                    if (pickupObject.Remove()) {
                        this.Level.Graphics.Audio.PlayOmnomnom();
                        this.Score += this.Level.Score(pickupObject.Type);
                        foreach (Ghost ghost in this.Level.Ghosts)
                            if (ghost != null)
                                ghost.VulnerabilityStart();
                    }
                }
            }

            // Collide with ghosts
            Ghost killer = null;
            foreach (Ghost ghost in this.Level.Ghosts)
                if (ghost != null)
                    if (this.Collides(ghost)) {
                        if (ghost.State == Ghost.States.Vulnerable) {
                            this.Score += this.Level.Score(Level.GHOSTSPAWN);
                            ghost.Die();
                        }
                        else if (ghost.State == Ghost.States.Alive) {
                            killer = ghost; // Don't die twice (can't use break, as other colliding ghosts might be vulnerable).
                        }
                    }
            if (killer != null)
                this.Die(killer);
        }

        public override void Draw(UpdateEventArgs e)
        {
            float shutAngle = (float)Math.Abs(e.TimeInS % 0.5 - 0.25) * 2.36f;

            // Render the top mouth part
            this.Level.Graphics.PacManMouthColorUniform.Matrix = this.Level.Graphics.WhiteToYellow;
            this.Level.Graphics.PacManMouthTextureUniform.Texture = this.Level.Graphics.PacManMouthTopTexture;
            this.Level.Graphics.SetPacManMouthModel(this.Position, this.Angle + MathHelper.Pi, shutAngle, true);
            this.MouthSurface.Render();
            // Render the bottom mouth part
            this.Level.Graphics.PacManMouthColorUniform.Matrix = this.Level.Graphics.Blue;
            this.Level.Graphics.PacManMouthTextureUniform.Texture = this.Level.Graphics.PacManMouthBottomTexture;
            this.Level.Graphics.SetPacManMouthModel(this.Position, this.Angle + MathHelper.Pi, -shutAngle, false);
            this.MouthSurface.Render();

            // Then render the normal pacman body over the part of the mouth that's inside the body
            base.Draw(e);
        }

        public bool Collides(Ghost ghost)
        {
            return (ghost.Position - this.Position).LengthSquared < SIZE * SIZE;
        }

        public override void UpdateCamera()
        {
            this.Camera.MoveFocusTo(new Vector3(0.3f * this.Level.PreferredCameraFocus.X + 0.7f * this.Position.X,
                0, 0.3f * this.Level.PreferredCameraFocus.Y + 0.7f * this.Position.Y));
        }

        protected override void setModelMatrix()
        {
            this.Level.Graphics.SetPacManModel(this.Position, this.Angle + MathHelper.Pi);
        }
    }
}
