using SharpDX;
using System;
using Generate.D3D;

namespace Generate.Content
{
    class DefaultModels
    {
        // Credits to SharpDX Toolkit
        internal static Model Sphere(int Seed, Vector3 Place, float Diameter = 4.0f)
        {
            int Tesselation = 8 * Renderer.AntiAliasing.Count;

            int VerticalSegments = Tesselation;
            int HorizontalSegments = Tesselation * 2;

            var Vertices = new Vertex[(VerticalSegments + 1) * (HorizontalSegments + 1)];
            var Indices = new int[(VerticalSegments) * (HorizontalSegments + 1) * 6];

            float Radius = Diameter / 2;

            int VertexCount = 0;
            // Create rings of vertices at progressively higher latitudes.
            for (int i = 0; i <= VerticalSegments; i++)
            {
                float v = 1.0f - (float)i / VerticalSegments;

                var latitude = (float)((i * Math.PI / VerticalSegments) - Math.PI / 2.0);
                var dy = (float)Math.Sin(latitude);
                var dxz = (float)Math.Cos(latitude);

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j <= HorizontalSegments; j++)
                {
                    float u = (float)j / HorizontalSegments;

                    var longitude = (float)(j * 2.0 * Math.PI / HorizontalSegments);
                    var dx = (float)Math.Sin(longitude);
                    var dz = (float)Math.Cos(longitude);

                    dx *= dxz;
                    dz *= dxz;

                    Vertices[VertexCount++] = new Vertex
                    {
                        Position = new Vector3(dx * Radius, dy * Radius, dz * Radius),
                        TexCoords = new Vector2(u, v),
                        Normal = new Vector3(dx, dy, dz)
                    };
                }
            }

            // Fill the index buffer with triangles joining each pair of latitude rings.
            int Stride = HorizontalSegments + 1;

            int Index = 0;
            for (int i = 0; i < VerticalSegments; i++)
            {
                for (int j = 0; j <= HorizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % Stride;

                    Indices[Index++] = (i * Stride + j);
                    Indices[Index++] = (i * Stride + nextJ);
                    Indices[Index++] = (nextI * Stride + j);

                    Indices[Index++] = (i * Stride + nextJ);
                    Indices[Index++] = (nextI * Stride + nextJ);
                    Indices[Index++] = (nextI * Stride + j);
                }
            }

            return new Model(Place, Vertices, Indices, Seed);
        }

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
                new Vertex //Bottom Right
                {
                    Position = new Vector3(32, Heights[1, 0], -32),
                    TexCoords = new Vector2(1, 0)
                },
            };
            
            Vertex.CalcSurfaceNormal(ref Vertices, 0, 3);
            Vertex.CalcSurfaceNormal(ref Vertices, 1, 4);

            return new Model(Vector3.Zero, Vertices, new[]
            {
                0, 1, 2, //Top Left
                0, 2, 3 //Bottom Right
            }, Seed);
        }

        internal static Model Triangle(int Seed)
        {
            var Vertices = new[]
            {
                new Vertex //Bottom left
                {
                    Position = new Vector3(-1, -1, 0),
                    TexCoords = new Vector2(0, 0)
                },
                new Vertex //Top middle
                {
                    Position = new Vector3(0, 1, 0),
                    TexCoords = new Vector2(1, 0.5f)
                },
                new Vertex //Bottom right
                {
                    Position = new Vector3(1, -1, 0),
                    TexCoords = new Vector2(0, 1)
                }
            };

            Vertex.CalcSurfaceNormal(ref Vertices);

            return new Model(new Vector3(0, 0, Seed / int.MaxValue * 10), Vertices, new[]
            {
                0, // Bottom left
			    1, // Top middle
			    2  // Bottom right
			}, Seed);
        }
    }
}
