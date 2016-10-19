using Generate.D3D;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using System;
using SharpDX.Direct3D;

namespace Generate.Content
{
    class Model : IDisposable
    {
        internal Vector3 MoveWorld;
        internal float Rotate = 0;
        internal float Scale = 1;

        private int[] Indices;
        private Buffer VertexBuffer;
        private Buffer IndexBuffer;

        private Texture2D Texture;
        private ShaderResourceView TextureView;

        internal Model(Vector3 MoveWorld, Vertex[] Vertices, int[] Indices, int Seed)
        {
            this.MoveWorld = MoveWorld;

            // Create the vertex buffer.
            VertexBuffer = Buffer.Create(Program.Renderer.Device, BindFlags.VertexBuffer, Vertices);

            // Create the index buffer.
            this.Indices = Indices;
            IndexBuffer = Buffer.Create(Program.Renderer.Device, BindFlags.IndexBuffer, this.Indices);

            var Rand = new Random(Seed);
            int Size = (int)Math.Pow(2, Rand.Next(Procedure.Constants.AvgTexDensity - 1, Procedure.Constants.AvgTexDensity + 1));

            // Allocate DataStream to receive the WIC image pixels
            using (var buffer = new DataStream(Size * Size * 4, true, true))
            {
                for (int X = 0; X < Size; X++)
                {
                    for (int Y = 0; Y < Size; Y++)
                    {
                        buffer.WriteByte((byte)Rand.Next(0, 256));
                        buffer.WriteByte((byte)Rand.Next(0, 256));
                        buffer.WriteByte((byte)Rand.Next(0, 256));

                        buffer.WriteByte(byte.MaxValue);
                    }
                }

                // Copy the content of the WIC to the buffer
                Texture = new Texture2D(Program.Renderer.Device, new Texture2DDescription
                {
                    Width = Size,
                    Height = Size,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    Usage = ResourceUsage.Immutable,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Format.R8G8B8A8_UNorm,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                }, new DataRectangle(buffer.DataPointer, Size * 4));
            }

            TextureView = new ShaderResourceView(Program.Renderer.Device, Texture);
        }

        internal void Render(Vector2 MoveChunks)
        {
            var Move = MoveWorld;
            Move.X += MoveChunks.X;
            Move.Z += MoveChunks.Y;
            
            Program.Renderer.Shader.UpdateMatrices(Move, Rotate, Scale);

            Program.Renderer.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<Vertex>(), 0));
            Program.Renderer.Device.ImmediateContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

            Program.Renderer.Device.ImmediateContext.PixelShader.SetShaderResource(0, TextureView);
            Program.Renderer.Device.ImmediateContext.DrawIndexed(Indices.Length, 0, 0);
        }

        public void Dispose()
        {
            Utilities.Dispose(ref TextureView);
            Utilities.Dispose(ref Texture);
            Utilities.Dispose(ref IndexBuffer);
            Utilities.Dispose(ref VertexBuffer);
        }
    }
}
