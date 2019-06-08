using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class MeshData
    {
        private List<Vector4> positions;
        private List<Vector3> uvCoordinates;
        private List<Vector3> normals;
        private List<Face> faces;

        public MeshData()
        {
            this.positions = new List<Vector4>();
            this.uvCoordinates = new List<Vector3>();
            this.normals = new List<Vector3>();
            this.faces = new List<Face>();
        }

        public MeshData(string filename, bool invertUVs = true)
            : this()
        {
            this.fromObjFile(filename, invertUVs);
        }

        #region Create indexed surfaces

        public IndexedSurface<TVertexData> ToIndexedSurface<TVertexData>(
            Func<Vector4, Vector3?, Vector3?, Color, TVertexData> vertexMaker)
            where TVertexData : struct, IVertexData
        {
            return this.ToIndexedSurface(
                ids => vertexMaker(this.positions[ids.Position],
                    ids.UV == -1 ? (Vector3?)null : this.uvCoordinates[ids.UV],
                    ids.Normal == -1 ? (Vector3?)null : this.normals[ids.Normal],
                    Color.White
                    ));
        }

        public IndexedSurface<TVertexData> ToIndexedSurface<TVertexData>(
            Func<Face.VertexIds, TVertexData> makeVertex)
           where TVertexData : struct, IVertexData
        {
            var surface = new IndexedSurface<TVertexData> { ClearOnRender = false };

            foreach (var face in this.faces) {
                ushort v0 = surface.AddVertex(makeVertex(face.Ids[0]));
                ushort v1 = surface.AddVertex(makeVertex(face.Ids[1]));
                for (int i = 2; i < face.Ids.Count; i++) {
                    ushort v2 = surface.AddVertex(makeVertex(face.Ids[i]));

                    surface.AddIndices(v0, v1, v2);

                    v1 = v2;
                }
            }

            surface.IsStatic = true;

            return surface;
        }

        #endregion

        #region Parse Obj file

        private void fromObjFile(string filename, bool invertUVs)
        {
            StreamReader stream = new StreamReader(File.OpenRead(filename));

            char[] whiteSpaces = new[] { ' ', '\t' };

            string line;
            while ((line = stream.ReadLine()) != null) {
                int commentSignId = line.IndexOf('#');
                line = (commentSignId == -1 ? line : line.Substring(0, commentSignId)).Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] splitLine = line.Split(whiteSpaces, StringSplitOptions.RemoveEmptyEntries);

                string keyword = splitLine[0];
                int parameterCount = splitLine.Length - 1;

                switch (keyword) {
                case "v":
                    if (parameterCount != 4 && parameterCount != 3)
                        throw new InvalidDataException("Vertex must have 3 or 4 coordinates.");
                    this.positions.Add(MeshData.parseObjVertex(splitLine));
                    break;
                case "vt":
                    if (parameterCount != 3 && parameterCount != 2)
                        throw new InvalidDataException("UV Coordinate must have 2 or 3 coordinates.");
                    this.uvCoordinates.Add(MeshData.parseObjUVCoordinate(splitLine, invertUVs));
                    break;
                case "vn":
                    if (parameterCount != 3)
                        throw new InvalidDataException("Normal must have 3 coordinates.");
                    this.normals.Add(MeshData.parseObjNormal(splitLine));
                    break;
                case "f":
                    if (parameterCount < 3)
                        throw new InvalidDataException("Face must have at least 3 positions");
                    this.faces.Add(MeshData.parseObjFace(splitLine));
                    //if (this.faces.Count == 2) {
                    //    Vector3 n0 = this.normals[this.faces[0].Ids[0].Normal];
                    //    Vector3 m1 = this.normals[this.faces[1].Ids[0].Normal];
                    //    break;
                    //}
                    break;
                default:
                    if (keyword == "s")
                        break;
                    throw new InvalidDataException(string.Format("Unknown keyword '{0}'.", keyword));
                }
            }
        }

        private static Vector4 parseObjVertex(string[] splitLine)
        {
            return new Vector4(
                float.Parse(splitLine[1]),
                float.Parse(splitLine[2]),
                float.Parse(splitLine[3]),
                splitLine.Length == 4 ? 1 : float.Parse(splitLine[4])
                );
        }

        private static Vector3 parseObjUVCoordinate(string[] splitLine, bool invertUVs)
        {
            if (invertUVs)
                return new Vector3(
                    float.Parse(splitLine[1]),
                    1 - float.Parse(splitLine[2]),
                    splitLine.Length == 3 ? 0 : float.Parse(splitLine[3])
                    );
            return new Vector3(
                float.Parse(splitLine[1]),
                float.Parse(splitLine[2]),
                splitLine.Length == 3 ? 0 : float.Parse(splitLine[3])
                );
        }

        private static Vector3 parseObjNormal(string[] splitLine)
        {
            return new Vector3(
                float.Parse(splitLine[1]),
                float.Parse(splitLine[2]),
                float.Parse(splitLine[3])
                );
        }

        private static Face parseObjFace(string[] splitLine)
        {
            List<Face.VertexIds> ids = new List<Face.VertexIds>();

            char[] splitAt = new[] { '/' };

            for (int i = 1; i < splitLine.Length; i++) {
                string[] splitVertexIds = splitLine[i].Split(splitAt, StringSplitOptions.None);

                int p = int.Parse(splitVertexIds[0]) - 1;
                int uv = splitVertexIds.Length > 1 ? int.Parse(splitVertexIds[1]) - 1 : -1;
                int n = splitVertexIds.Length > 2 ? int.Parse(splitVertexIds[2]) - 1 : -1;

                ids.Add(new Face.VertexIds(p, uv, n));
            }
            return new Face(ids);
        }

        #endregion

        public void AddWallSegment(Vector2[] corners, float negY, float posY)
        {
            // Initialization
            if (this.uvCoordinates.Count == 0) {
                this.uvCoordinates.AddRange(new Vector3[] {
                    new Vector3(1.9f, 0, 0), new Vector3(1.9f, 9, 0), new Vector3(2, 0, 0), new Vector3(2, 9, 0), // Left     (0-3)
                    new Vector3(3.9f, 1, 0), new Vector3(3.9f, 9, 0), new Vector3(4, 1, 0), new Vector3(4, 9, 0), // Middle   (4-7)
                    new Vector3(6, 1, 0), new Vector3(6, 9, 0), new Vector3(9, 1, 0), new Vector3(9, 9, 0)        // Right    (8-11)
                });
                this.normals.AddRange(new Vector3[] {   // N E S W U D - NU EU SU WU - NW NE SE SW - NWU NEU SEU SWU
                    -Vector3.UnitZ, Vector3.UnitX, Vector3.UnitZ, -Vector3.UnitX, Vector3.UnitY, -Vector3.UnitY, // 0-5
                    new Vector3(0, 1, -1), new Vector3(1, 1, 0), new Vector3(0, 1, 1), new Vector3(-1, 1, 0),    // 6-9
                    new Vector3(-1, 0, -1), new Vector3(1, 0, -1), new Vector3(1, 0, 1), new Vector3(-1, 0, 1),  // 10-13
                    new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(-1, 1, 1)   // 14-17
                });
                for (int i = 0; i < this.uvCoordinates.Count; i++)
                    this.uvCoordinates[i] = this.uvCoordinates[i] * 0.1f;
                for (int i = 6; i < this.normals.Count; i++)
                    this.normals[i].Normalize();
            }
            int baseId = this.positions.Count;

            // Add the positions
            if (corners.Length != 4)
                throw new Exception("A wall must have 4 (2D) corners.");
            for (int i = 0; i < 4; i++) {               // NW, NE, SW, SE
                float xOffset = i % 2 == 0 ? 0.1f : -0.1f;
                float yOffset = i < 2 ? 0.1f : -0.1f;
                this.positions.Add(new Vector4(corners[i].X + xOffset, -negY, corners[i].Y, 1));
                this.positions.Add(new Vector4(corners[i].X, -negY, corners[i].Y + yOffset, 1));
                this.positions.Add(new Vector4(corners[i].X + xOffset, posY - 0.2f, corners[i].Y, 1));
                this.positions.Add(new Vector4(corners[i].X, posY - 0.2f, corners[i].Y + yOffset, 1));
                this.positions.Add(new Vector4(corners[i].X + 2 * xOffset, posY, corners[i].Y + 2 * yOffset, 1));
            }

            // Add the faces
            int nwd1 = baseId + 0; int nwd2 = baseId + 1; int nwm1 = baseId + 2; int nwm2 = baseId + 3; int nwu = baseId + 4;
            int ned1 = baseId + 5; int ned2 = baseId + 6; int nem1 = baseId + 7; int nem2 = baseId + 8; int neu = baseId + 9;
            int swd1 = baseId + 10; int swd2 = baseId + 11; int swm1 = baseId + 12; int swm2 = baseId + 13; int swu = baseId + 14;
            int sed1 = baseId + 15; int sed2 = baseId + 16; int sem1 = baseId + 17; int sem2 = baseId + 18; int seu = baseId + 19;
            Face.VertexIds[] rectangles = new Face.VertexIds[] { // These are all the rectangle faces
                vids(nwm1, 0, 10), vids(nwd1, 1, 10), vids(nwm2, 2, 10), vids(nwd2, 3, 10), // NW
                vids(nem1, 0, 0), vids(ned1, 1, 0), vids(nwm1, 2, 0), vids(nwd1, 3, 0),     // N
                vids(nem2, 0, 10), vids(ned2, 1, 10), vids(nem1, 2, 10), vids(ned1, 3, 10), // NE
                vids(sem2, 0, 1), vids(sed2, 1, 1), vids(nem2, 2, 1), vids(ned2, 3, 1),     // E
                vids(sem1, 0, 10), vids(sed1, 1, 10), vids(sem2, 2, 10), vids(sed2, 3, 10), // SE
                vids(swm1, 0, 2), vids(swd1, 1, 2), vids(sem1, 2, 2), vids(sed1, 3, 2),     // S
                vids(swm2, 0, 10), vids(swd2, 1, 10), vids(swm1, 2, 10), vids(swd1, 3, 10), // SW
                vids(nwm2, 0, 3), vids(nwd2, 1, 3), vids(swm2, 2, 3), vids(swd2, 3, 3),     // W
                vids(nwu, 8, 4), vids(swu, 9, 4), vids(neu, 10, 4), vids(seu, 11, 4),       // U
                // vids(swd, 8, 5), vids(nwd, 9, 5), vids(sed, 10, 5), vids(ned, 11, 5),    // D

                vids(neu, 4, 6), vids(nem1, 5, 6), vids(nwu, 6, 6), vids(nwm1, 7, 6),       // NU
                vids(seu, 4, 7), vids(sem2, 5, 7), vids(neu, 6, 7), vids(nem2, 7, 7),       // EU
                vids(swu, 4, 8), vids(swm1, 5, 8), vids(seu, 6, 8), vids(sem1, 7, 8),       // SU
                vids(nwu, 4, 9), vids(nwm2, 5, 9), vids(swu, 6, 9), vids(swm2, 7, 9)        // WU
            };
            Face.VertexIds[] triangles = new Face.VertexIds[] { // These are all the triangle faces
                vids(nwu, 4, 14), vids(nwm1, 5, 14), vids(nwm2, 7, 14),                        // NWU
                vids(neu, 4, 15), vids(nem2, 5, 15), vids(nem1, 7, 15),                        // NEU
                vids(seu, 4, 16), vids(sem1, 5, 16), vids(sem2, 7, 16),                        // SEU
                vids(swu, 4, 17), vids(swm2, 5, 17), vids(swm1, 7, 17)                         // SWU
            };
            for (int i = 0; i < rectangles.Length; i += 4) {
                this.faces.Add(new Face(new List<Face.VertexIds>() {
                    rectangles[i + 0], rectangles[i + 1], rectangles[i + 2]
                }));
                this.faces.Add(new Face(new List<Face.VertexIds>() {
                    rectangles[i + 1], rectangles[i + 3], rectangles[i + 2]
                }));
            }
            for (int i = 0; i < triangles.Length; i += 3) {
                this.faces.Add(new Face(new List<Face.VertexIds>() {
                    triangles[i + 0], triangles[i + 1], triangles[i + 2]
                }));
            }
        }

        Face.VertexIds vids(int p, int t, int n)
        {
            return new Face.VertexIds(p, t, n);
        }

        public class Face
        {
            public List<VertexIds> Ids;

            public Face(List<VertexIds> ids)
            {
                this.Ids = ids;
            }

            public struct VertexIds
            {
                public int Position, UV, Normal;

                public VertexIds(int position, int uv, int normal)
                    : this()
                {
                    this.Position = position;
                    this.UV = uv;
                    this.Normal = normal;
                }
            }
        }
    }
}
