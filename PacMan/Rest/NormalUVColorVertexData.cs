using OpenTK;
using OpenTK.Graphics.OpenGL;
using amulware.Graphics;

namespace PacMan
{
    /// <summary>
    /// Light vertex data used for rendering textured vertices, like sprites.
    /// </summary>
    public struct NormalUVColorVertexData : IVertexData
    {
        // add attributes and constructors here
        /// <summary>
        /// The position
        /// </summary>
        public Vector3 Position; // 12 bytes
        /// <summary>
        /// The normal
        /// </summary>
        public Vector3 Normal; // 12 bytes
        /// <summary>
        /// The uv coordinate
        /// </summary>
        public Vector2 TexCoord; // 8 bytes
        /// <summary>
        /// The color
        /// </summary>
        public Color Color; // 4 bytes

        static private VertexAttribute[] vertexAttributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalUVColorVertexData"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="uv">The uv coordinate.</param>
        /// <param name="color">The color.</param>
        public NormalUVColorVertexData(Vector3 position, Vector3 normal, Vector2 uv, Color color)
        {
            this.Position = position;
            this.Normal = normal;
            this.TexCoord = uv;
            this.Color = color;
        }

        static private void setVertexAttributes()
        {
            NormalUVColorVertexData.vertexAttributes = new[]{
                new VertexAttribute("v_position", 3, VertexAttribPointerType.Float, 36, 0),
                new VertexAttribute("v_normal", 3, VertexAttribPointerType.Float, 36, 12), 
                new VertexAttribute("v_texcoord", 2, VertexAttribPointerType.Float, 36, 24),
                new VertexAttribute("v_color", 4, VertexAttribPointerType.UnsignedByte, 36, 32, true)
            };
        }

        /// <summary>
        /// Returns the vertex' <see cref="VertexAttributes" />
        /// </summary>
        /// <returns>
        /// Array of <see cref="VertexAttribute" />
        /// </returns>
        public VertexAttribute[] VertexAttributes()
        {
            if (NormalUVColorVertexData.vertexAttributes == null)
                NormalUVColorVertexData.setVertexAttributes();
            return NormalUVColorVertexData.vertexAttributes;
        }

        /// <summary>
        /// This method returns the size of the vertex data struct in bytes
        /// </summary>
        /// <returns>
        /// Struct's size in bytes
        /// </returns>
        public int Size()
        {
            return 36;
        }

        public static NormalUVColorVertexData FromMesh(Vector4 position, Vector3? uv, Vector3? normal, Color diffuseColor)
        {
            return new NormalUVColorVertexData(position.Xyz, normal.HasValue ? normal.Value.Normalized() : Vector3.Zero,
                uv.HasValue ? uv.Value.Xy : Vector2.Zero, diffuseColor);
        }
    }
}
