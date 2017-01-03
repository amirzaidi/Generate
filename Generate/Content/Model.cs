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
        internal Vector3 ScaleVector = new Vector3(1, 1, 1);
        internal float Scale
        {
            get
            {
                return ScaleVector.Length();
            }
            set
            {
                ScaleVector = new Vector3(value, value, value);
            }
        }

        protected int Seed;
        protected Vertex[] Vertices;
        protected int[] Indices;
        private VertexBufferBinding VertexBinding;
        private Buffer IndexBuffer;

        protected Texture2D Texture;
        private ShaderResourceView TextureView;

        protected Matrix RotateScale;

        internal Model(Vector3 MoveWorld, Vertex[] Vertices, int[] Indices, int Seed, bool AutoLoad = true)
        {
            this.MoveWorld = MoveWorld;
            this.Seed = Seed;
            this.Vertices = Vertices;
            this.Indices = Indices;

            if (AutoLoad)
            {
                ModelsToLoad.Push(this);
            }
        }

        internal void Load()
        {
            // Create the vertex buffer.
            var VertexBuffer = Buffer.Create(Program.Renderer.Device, BindFlags.VertexBuffer, Vertices);
            VertexBinding = new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<Vertex>(), 0);

            // Create the index buffer.
            IndexBuffer = Buffer.Create(Program.Renderer.Device, BindFlags.IndexBuffer, Indices);

            LoadTexture();

            RotateScale = Matrix.Scaling(ScaleVector.X, ScaleVector.Y, ScaleVector.Z) * Matrix.RotationY(Rotate);
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

            TextureFromFloatArray(Colors);
        }

        protected void TextureFromFloatArray(float[,,] Colors)
        {
            // Allocate DataStream to receive the WIC image pixels
            using (var Buffer = new DataStream(Colors.GetLength(1) * Colors.GetLength(0) * 4, true, true))
            {
                for (int Y = 0; Y < Colors.GetLength(1); Y++)
                {
                    for (int X = 0; X < Colors.GetLength(0); X++)
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
                    Width = Colors.GetLength(0),
                    Height = Colors.GetLength(1),
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    Usage = ResourceUsage.Immutable,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Renderer.FormatRGB,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                }, new DataRectangle(Buffer.DataPointer, Colors.GetLength(0) * 4));
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

        protected void BallVertices(int VerticalSegments, int HorizontalSegments, float Diameter)
        {
            Vertices = new Vertex[(VerticalSegments + 1) * (HorizontalSegments + 1)];

            float Radius = Diameter / 2;

            int VertexCount = 0;
            // Create rings of vertices at progressively higher latitudes.
            for (int i = 0; i <= VerticalSegments; i++)
            {
                //float v = 1.0f - (float)i / VerticalSegments;

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
                        //(u, v)
                        TexCoords = new Vector2(u, dy * 0.5f + 0.5f),
                        //Normal = new Vector3(dx, dy, dz)
                    };
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
