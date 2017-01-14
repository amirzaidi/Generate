using SharpDX;
using System;
using Generate.D3D;
using System.Collections.Generic;

namespace Generate.Content
{
    class DefaultModels
    {
        internal static Model Ground(int Seed, float[,] Heights)
        {
            var Vertices = new[]
            {
                new Vertex //Bottom Left
                {
                    Position = new Vector3(-32, Heights[0, 0], -32),
                    TexCoords = new Vector2(0, 0)
                },
                new Vertex //Top Left
                {
                    Position = new Vector3(-32, Heights[0, 1], 32),
                    TexCoords = new Vector2(0, 1)
                },
                new Vertex //Top Right
                {
                    Position = new Vector3(32, Heights[1, 1], 32),
                    TexCoords = new Vector2(1, 1)
                },

                new Vertex //Bottom Left
                {
                    Position = new Vector3(-32, Heights[0, 0], -32),
                    TexCoords = new Vector2(0, 0)
                },
                new Vertex //Top Right
                {
                    Position = new Vector3(32, Heights[1, 1], 32),
                    TexCoords = new Vector2(1, 1)
                },
                new Vertex //Bottom Right
                {
                    Position = new Vector3(32, Heights[1, 0], -32),
                    TexCoords = new Vector2(1, 0)
                },
            };
            
            Vertex.CalcSurfaceNormal(ref Vertices, 0, 2);
            Vertex.CalcSurfaceNormal(ref Vertices, 3, 5);

            return new Model(Vector3.Zero, Vertices, new[]
            {
                0, 1, 2, //Top Left
                3, 4, 5 //Bottom Right
            }, Seed);
        }

        internal static Model Building(Random Rand)
        {
            var VertexList = new List<Vertex>();

            var Vertices = new[]
            {
                new Vertex //left 4
                {
                    Position = new Vector3(-1, 0, 1),
                    TexCoords = new Vector2(0, 0),
                    Normal = -Vector3.UnitX
                },
                new Vertex
                {
                    Position = new Vector3(-1, 0, -1),
                    TexCoords = new Vector2(1, 0),
                    Normal = -Vector3.UnitX
                },
                new Vertex
                {
                    Position = new Vector3(-1, 1, 1),
                    TexCoords = new Vector2(0, 1),
                    Normal = -Vector3.UnitX
                },
                new Vertex
                {
                    Position = new Vector3(-1, 1, -1),
                    TexCoords = new Vector2(1, 1),
                    Normal = -Vector3.UnitX
                },
                new Vertex //right 4
                {
                    Position = new Vector3(1, 0, -1),
                    TexCoords = new Vector2(0, 0),
                    Normal = Vector3.UnitX
                },
                new Vertex
                {
                    Position = new Vector3(1, 0, 1),
                    TexCoords = new Vector2(1, 0),
                    Normal = Vector3.UnitX
                },
                new Vertex
                {
                    Position = new Vector3(1, 1, -1),
                    TexCoords = new Vector2(0, 1),
                    Normal = Vector3.UnitX
                },
                new Vertex
                {
                    Position = new Vector3(1, 1, 1),
                    TexCoords = new Vector2(1, 1),
                    Normal = Vector3.UnitX
                },
                new Vertex //back 4
                {
                    Position = new Vector3(1, 0, 1),
                    TexCoords = new Vector2(0, 0),
                    Normal = Vector3.UnitZ
                },
                new Vertex
                {
                    Position = new Vector3(-1, 0, 1),
                    TexCoords = new Vector2(1, 0),
                    Normal = Vector3.UnitZ
                },
                new Vertex
                {
                    Position = new Vector3(1, 1, 1),
                    TexCoords = new Vector2(0, 1),
                    Normal = Vector3.UnitZ
                },
                new Vertex
                {
                    Position = new Vector3(-1, 1, 1),
                    TexCoords = new Vector2(1, 1),
                    Normal = Vector3.UnitZ
                },
                new Vertex //front 4
                {
                    Position = new Vector3(-1, 0, -1),
                    TexCoords = new Vector2(0, 0),
                    Normal = -Vector3.UnitZ
                },
                new Vertex
                {
                    Position = new Vector3(1, 0, -1),
                    TexCoords = new Vector2(1, 0),
                    Normal = -Vector3.UnitZ
                },
                new Vertex
                {
                    Position = new Vector3(-1, 1, -1),
                    TexCoords = new Vector2(0, 1),
                    Normal = -Vector3.UnitZ
                },
                new Vertex
                {
                    Position = new Vector3(1, 1, -1),
                    TexCoords = new Vector2(1, 1),
                    Normal = -Vector3.UnitZ
                },
                new Vertex //top 4
                {
                    Position = new Vector3(-1, 1, 1),
                    TexCoords = new Vector2(0, 0),
                    Normal = Vector3.UnitY
                },
                new Vertex
                {
                    Position = new Vector3(-1, 1, -1),
                    TexCoords = new Vector2(1, 0),
                    Normal = Vector3.UnitY
                },
                new Vertex
                {
                    Position = new Vector3(1, 1, 1),
                    TexCoords = new Vector2(0, 1),
                    Normal = Vector3.UnitY
                },
                new Vertex
                {
                    Position = new Vector3(1, 1, -1),
                    TexCoords = new Vector2(1, 1),
                    Normal = Vector3.UnitY
                },
            };
            
            var Model = new Model(new Vector3(0, -Procedure.Constants.HeightIntensity * 2, 0), Vertices, new[]
            {
                0, 2, 1, //left
                1, 2, 3,

                4, 6, 5, //right
                5, 6, 7,

                8, 10, 9,
                9, 10, 11,

                12, 14, 13,
                13, 14, 15,

                16, 18, 17,
                17, 18, 19
			}, Rand.Next());

            Model.ScaleVector.X = 32 * Rand.NextFloat(0.5f, 1.0f);
            Model.ScaleVector.Y = 64 * Procedure.Constants.BuildingHeight * Rand.NextFloat(0.35f, 1.0f);
            Model.ScaleVector.Z = 32 * Rand.NextFloat(0.5f, 1.0f);

            return Model;
        }
    }
}
