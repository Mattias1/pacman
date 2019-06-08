using System;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class Camera
    {
        // Members
        Graphics graphics;
        Matrix4 view, projection;
        Vector3 eye, focus, up;
        float width, height;
        List<ScreenShake> screenshakes;
        float screenshakeTimer;

        // Properties
        public Vector3 Eye
        {
            get { return this.eye; }
            set { this.eye = value; }
        }
        public Vector3 Focus
        {
            get { return this.focus; }
            set { this.focus = value; }
        }
        public Vector3 Up
        {
            get { return this.up; }
        }
        public Matrix4 View
        {
            get { return this.view; }
        }
        public Matrix4 Projection
        {
            get { return this.projection; }
        }

        // Methods
        public Camera(Graphics graphics, int width, int height)
            : this(graphics, 2f * Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitY, width, height) { }

        public Camera(Graphics graphics, Vector3 camEye, Vector3 camFocus, Vector3 camUp, int width, int height)
        {
            this.graphics = graphics;
            this.up = camUp;
            this.eye = camEye;
            this.focus = camFocus;
            this.screenshakes = new List<ScreenShake>();
            this.screenshakeTimer = 0f;
            this.UpdateScreenSize(width, height, false);
            this.updateView();
        }

        public void UpdateScreenSize(int width, int height, bool updateGraphics = true)
        {
            this.width = (float)width;
            this.height = (float)height;
            this.updateProjection(updateGraphics);
        }

        void updateView(bool updateGraphics = true)
        {
            this.updateView(Vector3.Zero, updateGraphics);
        }
        void updateView(Vector3 offset, bool updateGraphics = true)
        {
            // Update the view matrix
            this.view = Matrix4.LookAt(this.eye + offset, this.focus + offset, this.up);
            if (updateGraphics)
                this.graphics.Set3DMatrices(this.view, this.projection);
        }

        void updateProjection(bool updateGraphics = true)
        {
            // Update the projection matrix
            this.projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, this.width / this.height, 1f, 90f);
            if (updateGraphics)
                this.graphics.Set3DMatrices(this.view, this.projection);
        }

        public void Move(Vector3 direction)
        {
            // Move the camera over the map
            this.eye += direction;
            this.focus += direction;
            this.updateView();
        }
        public void MoveTo(Vector3 position)
        {
            this.Move(position - this.eye);
        }
        public void MoveFocusTo(Vector3 position)
        {
            this.MoveTo(position - this.Focus + this.Eye);
        }

        public void RotateDeg(float degrees)
        {
            this.RotateRad(MathHelper.DegreesToRadians(degrees));
        }
        public void RotateRad(float radians)
        {
            Vector3 eye2focus = this.Focus - this.Eye;
            this.Focus = this.Eye + Vector3.TransformVector(eye2focus, Matrix4.CreateRotationY(radians));
        }

        public void RotateAroundFocusDeg(float degrees)
        {
            this.RotateAroundFocusRad(MathHelper.DegreesToRadians(degrees));
        }
        public void RotateAroundFocusRad(float radians)
        {
            Vector3 focus2eye = this.Eye - this.Focus;
            this.Eye = this.Focus + Vector3.TransformVector(focus2eye, Matrix4.CreateRotationY(radians));
        }

        public void ChangeDistance(float distanceChange)
        {
            Vector3 focus2eye = this.Eye - this.Focus;
            float distance = focus2eye.LengthFast;
            this.SetDistance(distance + distanceChange);
        }

        public void SetDistance(float distance)
        {
            distance = GameMath.Clamp(5f, 50f, distance);
            Vector3 focus2eye = this.Eye - this.Focus;
            focus2eye.NormalizeFast();
            this.Eye = this.Focus + focus2eye * distance;
        }

        public void Update(UpdateEventArgs e)
        {
            // Update the screen shake timer
            if (this.screenshakes.Count == 0)
                this.screenshakeTimer = 0f;
            else
                this.screenshakeTimer += e.ElapsedTimeInSf;

            // Add the screenshake effects
            Vector3 offset = Vector3.Zero;
            if (this.screenshakes.Count > 0) {
                for (int i = 0; i < this.screenshakes.Count; i++) {
                    ScreenShake ss = this.screenshakes[i];
                    offset += (Math.Abs(this.screenshakeTimer % (ss.SpeedInv * 4) - ss.SpeedInv * 2) - ss.SpeedInv) * ss.Force * ss.Direction * ss.Factor;
                    ss.Update(e);
                }
            }

            // Update the view and projection matrices
            this.updateView(offset);
        }

        public void AddScreenshake(float force, float seconds, float speedInv)
        {
            ScreenShake.Add(force, seconds, speedInv, this.screenshakes);
        }
    }

    public class ScreenShake
    {
        List<ScreenShake> container; // The pointer to the container list

        float initialLifetime;
        public float Force;
        public float Seconds;
        public float SpeedInv;
        public Vector3 Direction { get; private set; }

        public float Factor
        {
            get { return this.Seconds / this.initialLifetime; }
        }

        private ScreenShake(float force, float seconds, float speedInv, List<ScreenShake> container)
        {
            this.Force = force;
            this.initialLifetime = seconds;
            this.Seconds = seconds;
            this.SpeedInv = speedInv;
            this.container = container;
            Vector2 dir = GameMath.Vector2FromRotation((float)(MathHelper.TwoPi * GlobalRandom.NextDouble()));
            this.Direction = new Vector3(dir.X, 0, dir.Y);
        }

        public static ScreenShake Add(float force, float seconds, float speedInv, List<ScreenShake> container)
        {
            ScreenShake ss = new ScreenShake(force, seconds, speedInv, container);
            container.Add(ss);
            return ss;
        }

        public void Update(UpdateEventArgs e)
        {
            this.Seconds -= e.ElapsedTimeInSf;
            if (this.Seconds < 0)
                this.container.Remove(this);
        }
    }
}
