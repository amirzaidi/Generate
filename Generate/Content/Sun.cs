using Generate.D3D;
using Generate.Procedure;
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

        internal Sun(int Seed) : base(Vector3.Zero, null, null, Seed, false)
        {
            Scale = 600f;

            int Tesselation = 16;

            int VerticalSegments = Tesselation;
            int HorizontalSegments = Tesselation * 2;

            BallVertices(VerticalSegments, HorizontalSegments, 3f);

            // Fill the index buffer with triangles joining each pair of latitude rings.
            Indices = new int[(VerticalSegments) * (HorizontalSegments + 1) * 6];
            int Stride = HorizontalSegments + 1;

            int Index = 0;
            for (int i = 0; i < VerticalSegments; i++)
            {
                for (int j = 0; j <= HorizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % Stride;

                    //Inside out
                    Indices[Index++] = (i * Stride + j);
                    Indices[Index++] = (i * Stride + nextJ);
                    Indices[Index++] = (nextI * Stride + j);

                    Indices[Index++] = (i * Stride + nextJ);
                    Indices[Index++] = (nextI * Stride + nextJ);
                    Indices[Index++] = (nextI * Stride + j);
                }
            }

            Load();
        }

        protected override void LoadTexture()
        {
            // Allocate DataStream to receive the WIC image pixels
            using (var Buffer = new DataStream(4, true, true))
            {
                Buffer.WriteByte((byte)Math.Round((Constants.Light.X * 0.75 + 0.25) * 255));
                Buffer.WriteByte((byte)Math.Round((Constants.Light.Y * 0.75 + 0.25) * 255));
                Buffer.WriteByte((byte)Math.Round((Constants.Light.Z * 0.75 + 0.25) * 255));
                Buffer.WriteByte(byte.MaxValue);

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
                }, new DataRectangle(Buffer.DataPointer, 4));
            }
        }

        internal void Tick()
        {
            if (!RotateWatch.IsRunning)
            {
                RotateWatch.Start();
            }

            Matrix Rotation;
            Matrix.RotationY(RotateWatch.ElapsedMilliseconds / Constants.SunRotateTime, out Rotation);
            LightDirection = Vector3.TransformCoordinate(Constants.BaseLightDirection, Rotation);

            MoveWorld = new Vector3(Input.Camera.Position.X, Input.Camera.Position.Y, Input.Camera.Position.Z) - LightDirection * Scale;
            RotateScale = Matrix.Scaling(50f);
        }
    }
}
