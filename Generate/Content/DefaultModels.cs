using Generate.D3D;
using System;

namespace Generate.Content
{
    class DefaultModels
    {
        // Credits to SharpDX Toolkit
        internal static Model Sphere(int Seed)
        {
            int tessellation = 16;
            float diameter = 4.0f;

            int verticalSegments = tessellation;
            int horizontalSegments = tessellation * 2;

            var Vertices = new Vertex[(verticalSegments + 1) * (horizontalSegments + 1)];
            var Indices = new int[(verticalSegments) * (horizontalSegments + 1) * 6];

            float radius = diameter / 2;

            int vertexCount = 0;
            // Create rings of vertices at progressively higher latitudes.
            for (int i = 0; i <= verticalSegments; i++)
            {
                float v = 1.0f - (float)i / verticalSegments;

                var latitude = (float)((i * Math.PI / verticalSegments) - Math.PI / 2.0);
                var dy = (float)Math.Sin(latitude);
                var dxz = (float)Math.Cos(latitude);

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j <= horizontalSegments; j++)
                {
                    float u = (float)j / horizontalSegments;

                    var longitude = (float)(j * 2.0 * Math.PI / horizontalSegments);
                    var dx = (float)Math.Sin(longitude);
                    var dz = (float)Math.Cos(longitude);

                    dx *= dxz;
                    dz *= dxz;

                    Vertices[vertexCount++] = new Vertex
                    {
                        Position = new SharpDX.Vector4(dx * radius, dy * radius, dz * radius, 1),
                        TexCoords = new SharpDX.Vector2(u, v),
                        Normal = new SharpDX.Vector3(dx, dy, dz)
                    };
                }
            }

            // Fill the index buffer with triangles joining each pair of latitude rings.
            int stride = horizontalSegments + 1;

            int indexCount = 0;
            for (int i = 0; i < verticalSegments; i++)
            {
                for (int j = 0; j <= horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % stride;

                    Indices[indexCount++] = (i * stride + j);
                    Indices[indexCount++] = (i * stride + nextJ);
                    Indices[indexCount++] = (nextI * stride + j);

                    Indices[indexCount++] = (i * stride + nextJ);
                    Indices[indexCount++] = (nextI * stride + nextJ);
                    Indices[indexCount++] = (nextI * stride + j);
                }
            }

            return new Model(new SharpDX.Vector3(5, 0, 0), Vertices, Indices, Seed);
        }

        internal static Model Ground(int Seed)
        {
            var Vertices = new[]
            {
                new Vertex
                {
                    Position = new SharpDX.Vector4(-32, 0, -32, 1),
                    TexCoords = new SharpDX.Vector2(0, 0)
                },
                new Vertex
                {
                    Position = new SharpDX.Vector4(-32, 0, 32, 1),
                    TexCoords = new SharpDX.Vector2(0, 1)
                },
                new Vertex
                {
                    Position = new SharpDX.Vector4(32, 0, 32, 1),
                    TexCoords = new SharpDX.Vector2(1, 1)
                },
                new Vertex
                {
                    Position = new SharpDX.Vector4(32, 0, -32, 1),
                    TexCoords = new SharpDX.Vector2(1, 0)
                },
            };

            Vertex.CalcSurfaceNormal(ref Vertices);
            return new Model(new SharpDX.Vector3(0, -5, 0), Vertices, new[]
            {
                0, 1, 2,
                0, 2, 3
            }, Seed);
        }

        internal static Model Triangle(int Seed)
        {
            return new Model(new SharpDX.Vector3(0, -5, 0), new[]
            {
                new Vertex
                {
                    Position = new SharpDX.Vector4(-1, -1, 0, 1),
                    Normal = new SharpDX.Vector3(0, 0, -1),
                    TexCoords = new SharpDX.Vector2(0, 0) //Bottom left
                },
                new Vertex
                {
                    Position = new SharpDX.Vector4(0, 1, 0, 1),
                    Normal = new SharpDX.Vector3(0, 0, -1),
                    TexCoords = new SharpDX.Vector2(1, 0.5f) //Top middle
                },
                new Vertex
                {
                    Position = new SharpDX.Vector4(1, -1, 0, 1),
                    Normal = new SharpDX.Vector3(0, 0, -1),
                    TexCoords = new SharpDX.Vector2(0, 1) //Bottom right
                }
            }, new[]
            {
                0, // Bottom left
			    1, // Top middle
			    2  // Bottom right
			}, Seed);
        }
    }
}
