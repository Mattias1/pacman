using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using amulware.Graphics;

namespace PacMan
{
    public class Graphics
    {
        public Camera Camera;
        public Audio Audio;
        private ScreenManager screenManager;
        private ShaderProgram wallShader;

        #region Surfaces, geometries, uniforms, etc.

        // Matrices
        private Matrix4Uniform view2D;
        private Matrix4Uniform projection2D;
        private Matrix4Uniform view3D;
        private Matrix4Uniform projection3D;

        // Surfaces
        public IndexedSurface<UVColorVertexData> FontSurface { get; private set; }
        public IndexedSurface<UVColorVertexData> FontSurface3D { get; private set; }
        public IndexedSurface<NormalUVColorVertexData> PacmanSurface { get; private set; }
        public IndexedSurface<NormalUVColorVertexData> PacmanMouthSurface { get; private set; }
        public IndexedSurface<NormalUVColorVertexData> GhostSurface { get; private set; }
        public IndexedSurface<NormalUVColorVertexData> OrbSurface { get; private set; }
        public IndexedSurface<NormalUVColorVertexData> WallSurface { get; private set; }

        // Geometries
        public FontGeometry FontGeometry { get; private set; }
        public FontGeometry FontGeometry3D { get; private set; }
        public FontGeometry MenuFontGeometry { get; private set; }

        // Settings
        private Vector3 lightDirection;
        private Matrix4Uniform pacManModel;
        private Matrix4Uniform ghostModel;
        private Matrix4Uniform orbModel;
        public Matrix4Uniform WallColorUniform;
        public Matrix4Uniform GhostColorUniform;
        public TextureUniform PacManMouthTextureUniform;
        public Texture PacManMouthTopTexture;
        public Texture PacManMouthBottomTexture;
        public Matrix4Uniform PacManMouthColorUniform;

        // Color matrices
        public Matrix4 Red;
        public Matrix4 Pink;
        public Matrix4 Cyan;
        public Matrix4 Orange;

        public Matrix4 Grey;
        public Matrix4 LightGrey;

        public Matrix4 WhiteToYellow;

        public Matrix4 Blue;
        public Matrix4 Green;
        public Matrix4 Purple;
        #endregion

        public Graphics(Audio audio)
        {
            // Save a pointer to the audio class, for convenience
            this.Audio = audio;

            // Load Shader Programs.
            ShaderProgram uvShader = new ShaderProgram(VertexShader.FromFile("data/shaders/uvcolor_vs.glsl"), FragmentShader.FromFile("data/shaders/uvcolor_fs.glsl"));
            ShaderProgram objShader = new ShaderProgram(VertexShader.FromFile("data/shaders/obj_vs.glsl"), FragmentShader.FromFile("data/shaders/obj_fs.glsl"));
            ShaderProgram objShaderHax = new ShaderProgram(VertexShader.FromFile("data/shaders/obj_vs_hax.glsl"), FragmentShader.FromFile("data/shaders/obj_fs.glsl"));
            this.wallShader = objShader;

            // Create matrix uniforms used for rendering.
            this.view2D = new Matrix4Uniform("viewMatrix");
            this.projection2D = new Matrix4Uniform("projectionMatrix");
            this.view3D = new Matrix4Uniform("viewMatrix");
            this.projection3D = new Matrix4Uniform("projectionMatrix");

            this.createColorMatrices();
            this.WallColorUniform = new Matrix4Uniform("mixColor", this.Blue);

            this.lightDirection = new Vector3(-1f, -0.5f, -0.4f);
            this.lightDirection.Normalize();

            // Create the surfaces
            #region Font Surface
            Texture t = new Texture("data/fonts/freshman.png");

            this.FontSurface = new IndexedSurface<UVColorVertexData>();
            this.FontSurface.AddSettings(
                this.view2D,
                this.projection2D,
                new TextureUniform("diffuseTexture", t),
                SurfaceBlendSetting.Alpha,
                SurfaceDepthMaskSetting.DontMask
            );

            this.FontSurface.SetShaderProgram(uvShader);

            // the following line loads the json file
            this.FontGeometry = new FontGeometry(this.FontSurface, amulware.Graphics.Font.FromJsonFile("data/fonts/freshman_monospaced_numbers.json"));
            // this.FontGeometry.SizeCoefficient = new Vector2(1, -1); // FLIP IT

            this.MenuFontGeometry = this.FontGeometry;
            #endregion

            #region 3D font Surface
            t = new Texture("data/fonts/freshman.png");

            this.FontSurface3D = new IndexedSurface<UVColorVertexData>();
            this.FontSurface3D.AddSettings(
                this.view3D,
                this.projection3D,
                new TextureUniform("diffuseTexture", t),
                SurfaceBlendSetting.Alpha,
                SurfaceDepthMaskSetting.DontMask
            );

            this.FontSurface3D.SetShaderProgram(uvShader);

            // the following line loads the json file
            this.FontGeometry3D = new FontGeometry(this.FontSurface3D, amulware.Graphics.Font.FromJsonFile("data/fonts/freshman_monospaced_numbers.json"));
            this.FontGeometry3D.SizeCoefficient = new Vector2(1, -1); // FLIP IT
            #endregion

            #region PacmanSurface
            MeshData m = new MeshData("data/models/Pacman.obj");
            t = new Texture("data/sprites/PacmanTexture.png");
            this.pacManModel = new Matrix4Uniform("modelMatrix", Matrix4.Identity);

            this.PacmanSurface = m.ToIndexedSurface<NormalUVColorVertexData>(NormalUVColorVertexData.FromMesh);
            this.PacmanSurface.AddSettings(
                this.pacManModel,
                this.view3D,
                this.projection3D,
                new TextureUniform("diffuseTexture", t),
                new Vector3Uniform("lightDirection", this.lightDirection),
                new FloatUniform("ambientIntensity", 0.3f),
                new FloatUniform("diffuseIntensity", 0.8f),
                new Matrix4Uniform("mixColor", Matrix4.Identity)
            );
            this.PacmanSurface.SetShaderProgram(objShader);
            #endregion

            #region PacmanMouthSurface (must be placed directly after the 'PacmanSurface')
            m = new MeshData("data/models/Pacman_mouth.obj");
            this.PacManMouthTopTexture = new Texture("data/sprites/OrbTexture.png"); // 't' is the same texture as pacman, 't-top' is a plain white pixel (as used for the orbs).
            this.PacManMouthBottomTexture = t;
            this.PacManMouthTextureUniform = new TextureUniform("diffuseTexture", t);
            this.PacManMouthColorUniform = new Matrix4Uniform("mixColor", Matrix4.Identity);

            this.PacmanMouthSurface = m.ToIndexedSurface<NormalUVColorVertexData>(NormalUVColorVertexData.FromMesh);
            this.PacmanMouthSurface.AddSettings(
                this.pacManModel,
                this.view3D,
                this.projection3D,
                this.PacManMouthTextureUniform,
                new Vector3Uniform("lightDirection", this.lightDirection),
                new FloatUniform("ambientIntensity", 0.3f),
                new FloatUniform("diffuseIntensity", 0.8f),
                this.PacManMouthColorUniform
            );
            this.PacmanMouthSurface.SetShaderProgram(objShaderHax);
            #endregion

            #region GhostSurface
            m = new MeshData("data/models/Ghost.obj");
            t = new Texture("data/sprites/GhostTexture.png");
            this.GhostColorUniform = new Matrix4Uniform("mixColor", this.Red);
            this.ghostModel = new Matrix4Uniform("modelMatrix", Matrix4.Identity);

            this.GhostSurface = m.ToIndexedSurface<NormalUVColorVertexData>(NormalUVColorVertexData.FromMesh);
            this.GhostSurface.AddSettings(
                this.ghostModel,
                this.view3D,
                this.projection3D,
                new TextureUniform("diffuseTexture", t),
                new Vector3Uniform("lightDirection", this.lightDirection),
                new FloatUniform("ambientIntensity", 0.3f),
                new FloatUniform("diffuseIntensity", 0.8f),
                this.GhostColorUniform
            );
            this.GhostSurface.SetShaderProgram(objShader);
            #endregion

            #region OrbSurface
            m = new MeshData("data/models/Orb.obj");
            t = new Texture("data/sprites/OrbTexture.png");
            this.orbModel = new Matrix4Uniform("modelMatrix", Matrix4.Identity);

            this.OrbSurface = m.ToIndexedSurface<NormalUVColorVertexData>(NormalUVColorVertexData.FromMesh);
            this.OrbSurface.AddSettings(
                this.orbModel,
                this.view3D,
                this.projection3D,
                new TextureUniform("diffuseTexture", t),
                new Vector3Uniform("lightDirection", this.lightDirection),
                new FloatUniform("ambientIntensity", 0.3f),
                new FloatUniform("diffuseIntensity", 0.8f),
                new Matrix4Uniform("mixColor", Matrix4.Identity)
            );
            this.OrbSurface.SetShaderProgram(objShader);
            #endregion
        }

        public void CreateWallSurface(MeshData m)
        {
            Texture t = new Texture("data/sprites/WallTexture.png");

            this.WallSurface = m.ToIndexedSurface<NormalUVColorVertexData>(NormalUVColorVertexData.FromMesh);
            this.WallSurface.AddSettings(
                new Matrix4Uniform("modelMatrix", Matrix4.Identity),
                this.view3D,
                this.projection3D,
                new TextureUniform("diffuseTexture", t),
                new Vector3Uniform("lightDirection", this.lightDirection),
                new FloatUniform("ambientIntensity", 1),
                new FloatUniform("diffuseIntensity", 0),
                this.WallColorUniform
            );
            this.WallSurface.SetShaderProgram(this.wallShader);
        }

        #region Some public setters

        public void Set2DMatrices(Matrix4 view, Matrix4 projection)
        {
            this.view2D.Matrix = view;
            this.projection2D.Matrix = projection;
        }
        public void Set3DMatrices(Matrix4 view, Matrix4 projection)
        {
            this.view3D.Matrix = view;
            this.projection3D.Matrix = projection;
            this.Update3DFont();
        }

        public void Update3DFont()
        {
            if (this.Camera == null)
                return;
            this.FontGeometry3D.UnitX = Vector3.Cross(this.Camera.Focus - this.Camera.Eye, this.Camera.Up).Normalized();
            this.FontGeometry3D.UnitY = Vector3.Cross(this.FontGeometry3D.UnitX, this.Camera.Focus - this.Camera.Eye).Normalized();
        }

        public void SetPacManModel(Vector2 position, float angle)
        {
            this.setModel(this.pacManModel, position, angle);
        }
        public void SetPacManMouthModel(Vector2 position, float angle, float mouthAngle, bool flipHorizontal)
        {
            this.setModel(this.pacManModel, position, angle, 1f, mouthAngle, flipHorizontal);
        }
        public void SetGhostModel(Vector2 position, float angle)
        {
            this.setModel(this.ghostModel, position, angle);
        }
        public void SetOrbModel(Vector2 position, float scale = 1f)
        {
            this.setModel(this.orbModel, position, 0f, scale);
        }
        private void setModel(Matrix4Uniform modelMatrix, Vector2 position, float angle = 0f, float scale = 1f, float horizontalAngle = 0f, bool flipHorizontal = false)
        {
            modelMatrix.Matrix = Matrix4.CreateScale(scale) * Matrix4.CreateRotationZ(horizontalAngle) * Matrix4.CreateRotationY(-angle) * Matrix4.CreateTranslation(new Vector3(position.X, 0, position.Y));
            if (flipHorizontal) {
                Matrix4 flipMatrix = Matrix4.Identity;
                flipMatrix.M22 = -1f;
                modelMatrix.Matrix = flipMatrix * modelMatrix.Matrix;
            }
        }

        public void SetScreenManager(ScreenManager sM)
        {
            this.screenManager = sM;
        }

        public void AddScreenshake(float force, float seconds, float speedInv)
        {
            this.Camera.AddScreenshake(force, seconds, speedInv);
        }

        public void SetDefaultCamera()
        {
            this.Camera = new Camera(this, PacMan.CameraEye, Vector3.Zero, Vector3.UnitY, PacManProgram.WIDTH, PacManProgram.HEIGHT);
        }

        public void PrepareRender()
        {
            GL.ClearColor(amulware.Graphics.Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        #endregion

        private void createColorMatrices()
        {
            // The ghost matrices - 0n the original texture the background is purely blue, while the eyes are red, green and blue.
            // So to make sure the eyes stay white, we can add some of the red (or green) part to each of the colours.
            this.Red = createColorMatrix( // Also used for walls, it's generic enough (it only swaps blue and red) so can be used just fine.
                0, 0, 1,
                0, 1, 0,
                1, 0, 0
            );
            this.Pink = createColorMatrix(
                0, 0, 1,
                0, 1, 0.427f,
                1, 0, 0.863f
            );
            this.Cyan = createColorMatrix(
                1, 0, 0,
                0, 1, 1,
                1, 0, 1
            );
            this.Orange = createColorMatrix(
                0, 0, 1,
                0, 0.6f, 0.415f,
                1, 0, 0
            );
            this.Grey = createColorMatrix(
                1, 0, 0.5f,
                1, 0, 0.5f,
                1, 0, 0.5f
            );
            this.LightGrey = createColorMatrix(
                1, 0, 0.8f,
                1, 0, 0.8f,
                1, 0, 0.8f
            );

            // A pacman mouth matrix (1/256 = 0.00390625f)
            Bitmap bmp = (Bitmap)Image.FromFile("data/sprites/PacManTexture.png");
            System.Drawing.Color c = bmp.GetPixel(0, 0);
            this.WhiteToYellow = createColorMatrix(
                c.R * 0.00390625f, 0, 0,
                0, c.G * 0.00390625f, 0,
                0, 0, c.B * 0.00390625f
            );

            // The wall matrices - On the original texture the main color is (darkened) blue, the secondary colour uses blue and a bit of green.
            this.Blue = Matrix4.Identity;
            this.Green = createColorMatrix(
                0, 1, 0,
                0, 0, 1,
                1, 0, 0
            );
            this.Purple = createColorMatrix(
                1, 0, 1,
                0, 1, 0,
                0, 0, 1
            );
        }

        private Matrix4 createColorMatrix(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
        {
            // Create a matrix to transform the texture's colour (blue) to the colour you like. Transparancy is kept as is.
            // Note that the matrix4 constructor takes it's float parameters in mirrored order (though the names do make sense).
            return new Matrix4(
                m00, m10, m20, 0,
                m01, m11, m21, 0,
                m02, m12, m22, 0,
                0, 0, 0, 1
            );
        }

        public void Render()
        {
            this.FontSurface.Render();
            this.FontSurface3D.Render();
            // The ghost, orb and pacman surfaces are rendered in their own draw calls.
            // This way, we have one model that is drawn a number of times.
            // For each string we just create a new quad, that's only four vertices anyway.
        }

        #region Easy DrawString methods

        public void DrawString(string s, int height = 0)
        {
            this.DrawString(s, Vector2.Zero, height);
        }
        public void DrawString(string s, Vector2 offset, int height = 0)
        {
            if (height > 0)
                this.FontGeometry.Height = height;
            this.FontGeometry.DrawString(0.5f * this.screenManager.ScreenResolution + offset, s, 0.5f, 0.5f);
        }
        public void DrawString(int x, int y, string s, int height = 0)
        {
            this.DrawString(x, y, s, Vector2.Zero, height);
        }
        public void DrawString(int x, int y, string s, Vector2 offset, int height = 0)
        {
            if (height > 0)
                this.FontGeometry.Height = height;
            this.FontGeometry.DrawString(new Vector2(x, y) + offset, s);
        }
        public void DrawMultiLineString(string s, int height = 0)
        {
            this.DrawMultiLineString(s, Vector2.Zero, height);
        }
        public void DrawMultiLineString(string s, Vector2 offset, int height = 0)
        {
            if (height > 0)
                this.FontGeometry.Height = height;
            this.FontGeometry.DrawMultiLineString(0.5f * this.screenManager.ScreenResolution + offset, s, 0.5f, 0.5f);
        }
        public void DrawMultiLineString(int x, int y, string s, int height = 0)
        {
            this.DrawMultiLineString(x, y, s, Vector2.Zero, height);
        }
        public void DrawMultiLineString(int x, int y, string s, Vector2 offset, int height = 0)
        {
            if (height > 0)
                this.FontGeometry.Height = height;
            this.FontGeometry.DrawMultiLineString(new Vector2(x, y) + offset, s);
        }

        #endregion
    }
}
