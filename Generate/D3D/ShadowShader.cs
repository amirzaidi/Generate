using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System;
using System.Runtime.InteropServices;
using SharpDX;

namespace Generate.D3D
{
    class ShadowShader : IShader
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct MatricesLayout
        {
            internal Matrix LightWVP;
        }

        internal Matrix LightVP;
        private MatricesLayout Matrices;
        private Buffer MatricesBuffer;

        internal ShadowShader(Device Device)
        {
            Context = Device.ImmediateContext;
            Projection = Matrix.Transpose(Matrix.PerspectiveFovLH((float)(Math.PI / 2.0f), 1, 64f, float.MaxValue));

            using (var ByteCode = ShaderBytecode.CompileFromFile("D3D/ShadowShaderVertex.hlsl", "VS", "vs_4_0"))
            {
                VS = new VertexShader(Device, ByteCode);
                Signature = ShaderSignature.GetInputSignature(ByteCode);
            }

            using (var ByteCode = ShaderBytecode.CompileFromFile("D3D/ShadowShaderPixel.hlsl", "PS", "ps_4_0"))
            {
                PS = new PixelShader(Device, ByteCode);
            }

            MatricesBuffer = new Buffer(Device, new BufferDescription
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<MatricesLayout>(),
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            });

            InputLayout = new InputLayout(Device, Signature, Vertex.Layout);
        }

        internal override void Prepare()
        {
            base.Prepare();

            var Position = Input.Camera.Position;
            Position.Y = (float)Math.Pow(2, 7);
            LightVP = Projection * Matrix.Transpose(Matrix.LookAtLH(Position, Position + new Vector3(0, -1, 0.00001f), Vector3.UnitY));

            Context.VertexShader.SetConstantBuffer(0, MatricesBuffer);
        }

        internal override void UpdateWorld(Matrix World)
        {
            Matrices.LightWVP = LightVP * Matrix.Transpose(World);

            Context.MapSubresource(MatricesBuffer, MapMode.WriteDiscard, MapFlags.None, out BufferStream);
            BufferStream.Write(Matrices);
            Context.UnmapSubresource(MatricesBuffer, 0);

            Context.InputAssembler.InputLayout = InputLayout;
        }

        internal override void End()
        {
            Context.VertexShader.SetConstantBuffer(0, null);
        }

        public override void Dispose()
        {
            Utilities.Dispose(ref MatricesBuffer);
            base.Dispose();
        }
    }
}
