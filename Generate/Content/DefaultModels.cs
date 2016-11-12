using Generate.D3D;
using System;

namespace Generate.Content
{
    class DefaultModels
    {
        // Credits to SharpDX Toolkit
        internal static Model Sphere(int Seed)
        {
            int Tesselation = 8;
            float Diameter = 4.0f;

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
                        Position = new SharpDX.Vector3(dx * Radius, dy * Radius, dz * Radius),
                        TexCoords = new SharpDX.Vector2(u, v),
                        Normal = new SharpDX.Vector3(dx, dy, dz)
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

            return new Model(new SharpDX.Vector3(Seed / int.MaxValue * 50f, 0, Seed % 50), Vertices, Indices, Seed);
        }

        internal static Model Ground(int Seed, float[,] Heights)
        {
            var Vertices = new[]
            {
                new Vertex //Bottom Left
                {
                    Position = new SharpDX.Vector3(-32, Heights[0, 0], -32),
                    TexCoords = new SharpDX.Vector2(0, 0)
                },
                new Vertex //Top Left
                {
                    Position = new SharpDX.Vector3(-32, Heights[0, 1], 32),
                    TexCoords = new SharpDX.Vector2(0, 1)
                },
                new Vertex //Top Right
                {
                    Position = new SharpDX.Vector3(32, Heights[1, 1], 32),
                    TexCoords = new SharpDX.Vector2(1, 1)
                },
                new Vertex //Bottom Right
                {
                    Position = new SharpDX.Vector3(32, Heights[1, 0], -32),
                    TexCoords = new SharpDX.Vector2(1, 0)
                },
            };

            // ToDo: Fix Ground Triangle Warp
            Vertex.CalcSurfaceNormal(ref Vertices, 0, 3);
            Vertex.CalcSurfaceNormal(ref Vertices, 1, 4);

            return new Model(new SharpDX.Vector3(0, -5, 0), Vertices, new[]
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
                    Position = new SharpDX.Vector3(-1, -1, 0),
                    TexCoords = new SharpDX.Vector2(0, 0)
                },
                new Vertex //Top middle
                {
                    Position = new SharpDX.Vector3(0, 1, 0),
                    TexCoords = new SharpDX.Vector2(1, 0.5f)
                },
                new Vertex //Bottom right
                {
                    Position = new SharpDX.Vector3(1, -1, 0),
                    TexCoords = new SharpDX.Vector2(0, 1)
                }
            };

            Vertex.CalcSurfaceNormal(ref Vertices);

            return new Model(new SharpDX.Vector3(0, 0, Seed / int.MaxValue * 10), Vertices, new[]
            {
                0, // Bottom left
			    1, // Top middle
			    2  // Bottom right
			}, Seed);
        }
    }
}
