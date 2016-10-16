using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System;
using System.Runtime.InteropServices;
using SharpDX;

namespace Generate.D3D
{
    class Shader : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        struct ConstantMatrixLayout
        {
            internal Matrix WVP;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LightLayout
        {
            internal Vector4 Color;
            internal Vector3 Direction;
            internal float Padding;
        }

        private DeviceContext Context;

        private ShaderSignature Signature;
        private VertexShader VS;
        private PixelShader PS;
        private Buffer ConstantMatrixBuffer;
        private Buffer LightBuffer;
        private Matrix Projection;
        private DataStream BufferStream;

        internal Shader(Device Device, Matrix Projection)
        {
            this.Projection = Projection;
            Context = Device.ImmediateContext;

            using (var ByteCode = ShaderBytecode.CompileFromFile("D3D/VertexShader.hlsl", "VS", "vs_4_0"))
            {
                VS = new VertexShader(Device, ByteCode);
                Signature = ShaderSignature.GetInputSignature(ByteCode);
            }

            Context.VertexShader.Set(VS);

            ConstantMatrixBuffer = new Buffer(Device, new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<ConstantMatrixLayout>(),
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            });

            Context.VertexShader.SetConstantBuffer(0, ConstantMatrixBuffer);
            Context.InputAssembler.InputLayout = new InputLayout(Device, Signature, Vertex.Layout);

            using (var ByteCode = ShaderBytecode.CompileFromFile("D3D/PixelShader.hlsl", "PS", "ps_4_0"))
            {
                PS = new PixelShader(Device, ByteCode);
            }

            Context.PixelShader.Set(PS);
            Context.PixelShader.SetSampler(0, new SamplerState(Device, new SamplerStateDescription
            {
                Filter = Filter.MinLinearMagMipPoint,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                BorderColor = Color.Transparent,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = -float.MaxValue,
                MaximumLod = float.MaxValue
            }));

            LightBuffer = new Buffer(Device, new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<LightLayout>(),
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            });

            Context.PixelShader.SetConstantBuffer(0, LightBuffer);
            
            Context.MapSubresource(LightBuffer, MapMode.WriteDiscard, MapFlags.None, out BufferStream);
            BufferStream.Write(new LightLayout
            {
                Color = new Vector4(1, 0.5f, 1, 1),
                Direction = new Vector3(1, 2, 1),
                Padding = 0
            });
            Context.UnmapSubresource(LightBuffer, 0);
        }

        internal void UpdateBuffers(Vector3 MoveWorld, float Rotate, float Scale)
        {
            var World = Matrix.Scaling(Scale) * Matrix.RotationY(Rotate) * Matrix.Translation(MoveWorld);
            World.Transpose();

            var WVP = new ConstantMatrixLayout
            {
                WVP = Projection * Input.Camera.View() * World
            };
            
            Context.MapSubresource(ConstantMatrixBuffer, MapMode.WriteDiscard, MapFlags.None, out BufferStream);
            BufferStream.Write(WVP);
            Context.UnmapSubresource(ConstantMatrixBuffer, 0);
        }

        public void Dispose()
        {
            Utilities.Dispose(ref BufferStream);
            Utilities.Dispose(ref LightBuffer);
            Utilities.Dispose(ref ConstantMatrixBuffer);
            Utilities.Dispose(ref PS);
            Utilities.Dispose(ref VS);
            Utilities.Dispose(ref Signature);
        }
    }
}
