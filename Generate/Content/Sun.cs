using Generate.D3D;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Diagnostics;

namespace Generate.Content
{
    class Sun : Model
    {
        internal static Sun Main;

        private Stopwatch RotateWatch = new Stopwatch();
        internal Vector3 LightDirection;

        internal Sun() : base(Vector3.Zero, null, null, 0)
        {
            Scale = 500f * (float)Math.Pow(Renderer.AntiAliasing.Count, 0.3);

            int Tesselation = 8 * Renderer.AntiAliasing.Count;
            float Diameter = 4.0f;

            int VerticalSegments = Tesselation;
            int HorizontalSegments = Tesselation * 2;

            Vertices = new Vertex[(VerticalSegments + 1) * (HorizontalSegments + 1)];
            Indices = new int[(VerticalSegments) * (HorizontalSegments + 1) * 6];

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
        }

        protected override void LoadTexture()
        {
            // Allocate DataStream to receive the WIC image pixels
            using (var buffer = new DataStream(4, true, true))
            {
                for (int i = 0; i < 4; i++)
                {
                    buffer.WriteByte(byte.MaxValue);
                }

                // Copy the content of the WIC to the buffer
                Texture = new Texture2D(Program.Renderer.Device, new Texture2DDescription
                {
                    Width = 1,
                    Height = 1,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    Usage = ResourceUsage.Immutable,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Renderer.FormatRGB,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                }, new DataRectangle(buffer.DataPointer, 4));
            }
        }

        internal void Tick()
        {
            if (!RotateWatch.IsRunning)
            {
                RotateWatch.Start();
            }

            Matrix Rotation;
            Matrix.RotationY(RotateWatch.ElapsedMilliseconds / 1000f, out Rotation);
            LightDirection = Vector3.TransformCoordinate(Procedure.Constants.BaseLightDirection, Rotation);

            MoveWorld = new Vector3(Input.Camera.Position.X, 0, Input.Camera.Position.Z) - LightDirection * Scale;
            RotateScale = Matrix.Scaling(50f);
        }
    }
}
