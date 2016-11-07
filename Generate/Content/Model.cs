using Generate.D3D;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using System;
using System.Collections.Concurrent;

namespace Generate.Content
{
    class Model : IDisposable
    {
        internal static ConcurrentStack<Model> ModelsToLoad = new ConcurrentStack<Model>();

        internal Vector3 MoveWorld;
        internal float Rotate = 0;
        internal float Scale = 1;

        private int Seed;
        protected Vertex[] Vertices;
        protected int[] Indices;
        private VertexBufferBinding VertexBinding;
        private Buffer IndexBuffer;

        protected Texture2D Texture;
        private ShaderResourceView TextureView;

        protected Matrix RotateScale;
        internal bool Loaded = false;

        internal Model(Vector3 MoveWorld, Vertex[] Vertices, int[] Indices, int Seed)
        {
            this.MoveWorld = MoveWorld;
            this.Seed = Seed;
            this.Vertices = Vertices;
            this.Indices = Indices;

            ModelsToLoad.Push(this);
        }

        internal void Load()
        {
            // Create the vertex buffer.
            var VertexBuffer = Buffer.Create(Program.Renderer.Device, BindFlags.VertexBuffer, Vertices);
            VertexBinding = new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<Vertex>(), 0);

            // Create the index buffer.
            IndexBuffer = Buffer.Create(Program.Renderer.Device, BindFlags.IndexBuffer, Indices);

            LoadTexture();

            TextureView = new ShaderResourceView(Program.Renderer.Device, Texture);
            RotateScale = Matrix.Scaling(Scale) * Matrix.RotationY(Rotate);

            Loaded = true;
        }

        protected virtual void LoadTexture()
        {
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
                    Format = Renderer.FormatRGB,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                }, new DataRectangle(buffer.DataPointer, Size * 4));
            }
        }

        internal void Render()
            => Render(Vector2.Zero);

        internal void Render(Vector2 MoveChunks)
        {
            if (Loaded)
            {
                var Move = MoveWorld;
                Move.X += MoveChunks.X;
                Move.Z += MoveChunks.Y;

                var Context = Program.Renderer.Device.ImmediateContext;
                Program.Renderer.ActiveShader.UpdateWorld(RotateScale * Matrix.Translation(Move));

                Context.InputAssembler.SetVertexBuffers(0, VertexBinding);
                Context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

                Context.PixelShader.SetShaderResource(0, TextureView);

                if (IndexBuffer != null)
                {
                    Context.DrawIndexed(Indices.Length, 0, 0);
                }
                else
                {
                    Program.LogLine("Unneeded render..", "Model.Render");
                }
            }
        }

        public void Dispose()
        {
            Utilities.Dispose(ref TextureView);
            Utilities.Dispose(ref Texture);
            Utilities.Dispose(ref IndexBuffer);
            VertexBinding.Buffer?.Dispose();
        }
    }
}
