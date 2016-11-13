using Generate.D3D;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using System;
using System.Collections.Concurrent;
using Generate.Procedure;

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

            RotateScale = Matrix.Scaling(Scale) * Matrix.RotationY(Rotate);
            TextureView = new ShaderResourceView(Program.Renderer.Device, Texture);
        }

        protected virtual void LoadTexture()
        {
            var Colors = new float[Constants.TextureDensity, Constants.TextureDensity, 3];

            for (int X = 0; X < Colors.GetLength(0); X++)
            {
                for (int Y = 0; Y < Colors.GetLength(1); Y++)
                {
                    for (int C = 0; C < Colors.GetLength(2); C++)
                    {
                        Colors[X, Y, C] = 1f;
                    }
                }
            }

            var Random = new Random(Seed);
            for (int i = 0; i < Procedure.Texture.Handlers.Length; i++)
            {
                Procedure.Texture.Handlers[i](Colors, Random);
            }

            // Allocate DataStream to receive the WIC image pixels
            using (var Buffer = new DataStream(Constants.TextureDensity * Constants.TextureDensity * 4, true, true))
            {
                for (int X = 0; X < Colors.GetLength(0); X++)
                {
                    for (int Y = 0; Y < Colors.GetLength(1); Y++)
                    {
                        for (int C = 0; C < Colors.GetLength(2); C++)
                        {
                            if (Colors[X, Y, C] > 1f)
                            {
                                Buffer.WriteByte(byte.MaxValue);
                            }
                            else if (Colors[X, Y, C] < 0f)
                            {
                                Buffer.WriteByte(0);
                            }
                            else
                            {
                                Buffer.WriteByte((byte)Math.Round(Colors[X, Y, C] * byte.MaxValue));
                            }
                        }
                        
                        Buffer.WriteByte(byte.MaxValue); //Alpha
                    }
                }

                // Copy the content of the WIC to the buffer
                Texture = new Texture2D(Program.Renderer.Device, new Texture2DDescription
                {
                    Width = Constants.TextureDensity,
                    Height = Constants.TextureDensity,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    Usage = ResourceUsage.Immutable,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Renderer.FormatRGB,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                }, new DataRectangle(Buffer.DataPointer, Constants.TextureDensity * 4));
            }
        }

        internal void Render()
            => Render(Vector2.Zero);

        internal void Render(Vector2 MoveChunks)
        {
            if (TextureView != null)
            {
                var Move = MoveWorld;
                Move.X += MoveChunks.X;
                Move.Z += MoveChunks.Y;

                var Context = Program.Renderer.Device.ImmediateContext;
                Program.Renderer.ActiveShader.UpdateWorld(RotateScale * Matrix.Translation(Move));

                Context.InputAssembler.SetVertexBuffers(0, VertexBinding);
                Context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

                Context.PixelShader.SetShaderResource(0, TextureView);
                Context.DrawIndexed(Indices.Length, 0, 0);
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
