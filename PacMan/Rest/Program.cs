using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using amulware.Graphics;
using Cireon.Audio;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace PacMan
{
    class PacManProgram : Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            using (PacManProgram program = new PacManProgram()) {
                Settings.Get.Load();
                AudioManager.Initialize();
                program.Run(50);
                AudioManager.Instance.Dispose();
                Settings.Get.Save();
            }
        }

        public const int WIDTH = 1080;
        public const int HEIGHT = 580;

        Graphics graphics;
        Audio audio;
        ScreenManager screenManager;

        public PacManProgram()
            : base(WIDTH, HEIGHT)
        {
            Console.WriteLine("Supported OpenGL version: " + GL.GetString(StringName.Version));
            Console.WriteLine("Supported GLSL version: " + GL.GetString(StringName.ShadingLanguageVersion));

            this.Title = "PacMan";
            this.WindowBorder = OpenTK.WindowBorder.Fixed;
        }

        protected override void OnLoad(EventArgs e)
        {
            // Create the sound class
            this.audio = new Audio();
         
            // Create graphics and camera
            this.graphics = new Graphics(this.audio);
            this.graphics.SetDefaultCamera();

            // Create screen manager and add the title screen
            this.screenManager = new ScreenManager(this, this.graphics);
            this.graphics.SetScreenManager(this.screenManager);
            this.screenManager.AddScreen(new TitleScreen(this.screenManager));

            InputManager.Keys.AddMenu().AddAtoZ().Add0to9().AddKeys(Key.BackSpace, Key.Delete, Key.ShiftLeft, Key.ShiftRight, Key.Period);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle.X, this.ClientRectangle.Y, this.ClientRectangle.Width, this.ClientRectangle.Height);

            // Make sure to update anything custom made that relies on the screen resolution.
            int w = this.ClientRectangle.Width;
            int h = this.ClientRectangle.Height;

            // Set the 3D matrices
            this.graphics.Camera.UpdateScreenSize(w, h);

            // These 2D matrices create a pixel perfect projection with a scale from 1:1 from the z=0 plane to the screen.
            this.graphics.Set2DMatrices(
                Matrix4.CreateTranslation(-w / 2f, -h / 2f, 0)
                    * new Matrix4(Vector4.UnitX, -Vector4.UnitY, Vector4.UnitZ, Vector4.UnitW)
                    * Matrix4.LookAt(-2f * Vector3.UnitZ, Vector3.UnitZ, -Vector3.UnitY),
                Matrix4.CreatePerspectiveOffCenter(-w / 4f, w / 4f, h / 4f, -h / 4f, 1f, 64f)
            );
        }

        protected override void OnUpdate(UpdateEventArgs e)
        {
            // Close program on shift + escape
            if (this.Keyboard[Key.ShiftLeft] && this.Keyboard[Key.Escape])
                this.Close();

            this.screenManager.Update(e);

            this.graphics.Camera.Update(e);

            AudioManager.Instance.Update(e.ElapsedTimeInSf);
        }

        protected override void OnRender(UpdateEventArgs e)
        {
            this.graphics.PrepareRender();

            this.screenManager.Draw(e);

            this.graphics.Render();

            this.SwapBuffers();
        }
    }
}
