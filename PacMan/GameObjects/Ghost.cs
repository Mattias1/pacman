using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class Ghost : CameraGameObject
    {
        public const float SEEINGRANGE = 10f;
        public enum States { Alive, Vulnerable, Dead }
        public States State { get; private set; }
        public int ListIndex { get; private set; }
        public float TimeLeftInS { get; private set; }
        Matrix4 colorMatrix;
        Matrix4Uniform colorMatrixUniform;

        public override float Speed
        {
            get
            {
                if (this.State == States.Alive && Settings.Get.Speed)
                    return 7.4f;
                if (this.State == States.Vulnerable)
                    return 6.0f;
                if (this.State == States.Dead)
                    return 4f;
                return this.Level.PacMan.Speed;
            }
        }

        public Ghost(Vector2 startingPos, Level level, int listIndex)
            : base(startingPos, Vector2.Zero, level, level.Graphics.GhostSurface)
        {
            this.State = States.Alive;
            this.ListIndex = listIndex;
            this.colorMatrix = this.Level.Graphics.Red;
            if (listIndex == 1)
                this.colorMatrix = this.Level.Graphics.Pink;
            if (listIndex == 2)
                this.colorMatrix = this.Level.Graphics.Cyan;
            if (listIndex == 3)
                this.colorMatrix = this.Level.Graphics.Orange;
            this.colorMatrixUniform = this.Level.Graphics.GhostColorUniform;
        }

        public void VulnerabilityStart()
        {
            if (this.State == States.Dead)
                return;
            this.State = States.Vulnerable;
            this.TimeLeftInS = 11f;
            if (this.Velocity != Vector2.Zero) {
                this.Velocity.Normalize();
                this.Velocity *= this.Speed;
            }
        }

        public void VulnerabilityOver(bool revive = false)
        {
            if (!revive && this.State == States.Dead)
                return;
            this.State = States.Alive;
            if (this.Velocity != Vector2.Zero) {
                this.Velocity.Normalize();
                this.Velocity *= this.Speed;
            }
        }

        bool flicker()
        {
            if (this.State != States.Vulnerable || this.TimeLeftInS > 3f)
                return false;
            return ((int)(4 * this.TimeLeftInS)) % 2 == 0;
        }

        public void Die()
        {
            // Manage score and state
            this.Score -= this.Level.Score(Level.GHOSTSPAWN);
            this.State = States.Dead;

            // Set the velocity
            int count = -1;
            Vector2 spawnPos = Vector2.Zero;
            for (int y = 0; y < this.Level.Height; y++) {
                for (int x = 0; x < this.Level.Width; x++) {
                    if (this.Level.UnsafeIsGhostSpawn(x, y))
                        count++;
                    if (count == this.ListIndex) {
                        spawnPos = this.Level.GridToWorld(x, y);
                        break;
                    }
                }
                if (count == this.ListIndex)
                    break;
            }
            this.Velocity = spawnPos - this.Position;
            this.Velocity.Normalize();
            this.Velocity *= this.Speed;
        }

        public override void Update(UpdateEventArgs e)
        {
            // Update the timer
            if (this.TimeLeftInS > 0) {
                this.TimeLeftInS -= e.ElapsedTimeInSf;
                if (this.TimeLeftInS == 0)
                    this.TimeLeftInS = -0.1f;
            }

            // Update the position (and velocity)
            if (this.State == States.Dead) {
                // Update the AI manually
                this.AI.UpdateBeforeMove(e);

                this.moveNormal(e);

                Vector2 gridPos = this.GetGridPos();
                if (!this.Level.InRange(gridPos)) {
                    // In very rare cases (multiplayer) the gridPos can give invalid coordinates.
                    for (int y = 0; y < this.Level.Height; y++)
                        for (int x = 0; x > this.Level.Width; x++)
                            if (this.Level.UnsafeIsGhostSpawn(x, y)) {
                                this.VulnerabilityOver(true);
                                this.Position = this.Level.GridToWorld(x, y);
                                this.Velocity = Vector2.Zero;
                                y = this.Level.Height;
                                break;
                            }
                }
                else if (this.Level.UnsafeIsGhostSpawn(gridPos)) {
                    Vector2 gridCenter = this.GetGridCenter();
                    if ((gridCenter - this.Position).LengthSquared < this.Speed * this.Speed * e.ElapsedTimeInSf * e.ElapsedTimeInSf * 2) {
                        this.VulnerabilityOver(true);
                        this.Position = gridCenter;
                        this.Velocity = Vector2.Zero;
                    }
                }

                // Update the camera
                if (this.IsPlayer())
                    this.UpdateCamera();

                // Update the AI manually
                this.AI.UpdateAfterMove(e, Vector2.Zero);
            }
            else {
                base.Update(e);
            }

            // Manage ghost vulnerability
            if (this.TimeLeftInS < 0 && this.State == States.Vulnerable) {
                this.VulnerabilityOver();
                this.TimeLeftInS = 0;
            }
        }

        public override void UpdateCamera()
        {
            this.Camera.SetDistance(Ghost.SEEINGRANGE - 2f);
            Vector2 offset = (this.Camera.Eye - this.Camera.Focus).Xz;
            offset.NormalizeFast();
            offset *= 2f;
            this.Camera.MoveFocusTo(new Vector3(this.Position.X + offset.X, 0, this.Position.Y + offset.Y));
        }

        public override void Draw(UpdateEventArgs e)
        {
            if (this.State == States.Dead) {
                this.Level.Graphics.SetOrbModel(this.Position + new Vector2(-0.4f, 0.1f));
                this.Level.Graphics.OrbSurface.Render();
                this.Level.Graphics.SetOrbModel(this.Position + new Vector2(0.4f, -0.1f));
                this.Level.Graphics.OrbSurface.Render();
            }
            else if (this.State == States.Vulnerable) {
                this.colorMatrixUniform.Matrix = flicker() ? this.Level.Graphics.Grey : this.Level.Graphics.LightGrey;
                base.Draw(e);
            }
            else {
                this.colorMatrixUniform.Matrix = this.colorMatrix;
                base.Draw(e);
            }
        }

        protected override void setModelMatrix()
        {
            this.Level.Graphics.SetGhostModel(this.Position, this.Angle - MathHelper.PiOver2);
        }

        public static string GetName(int index)
        {
            if (index == 0)
                return "Blinky";
            if (index == 1)
                return "Pinky";
            if (index == 2)
                return "Inky";
            if (index == 3)
                return "Clyde";
            return "Ghost " + index.ToString();
        }
    }
}
