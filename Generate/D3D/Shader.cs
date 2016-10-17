using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D;

namespace Generate.D3D
{
    class Shader : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        struct ConstantMatrixLayout
        {
            internal Matrix World;
            internal Matrix View;
            internal Matrix Projection;
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
        private SamplerState Sampler;

        private DataStream BufferStream;
        private ConstantMatrixLayout ConstantMatrix;

        private LightLayout Light;

        internal Shader(Device Device, Matrix Perspective)
        {
            ConstantMatrix.Projection = Perspective;
            Context = Device.ImmediateContext;

            using (var ByteCode = ShaderBytecode.CompileFromFile("D3D/VertexShader.hlsl", "VS", "vs_4_0"))
            {
                VS = new VertexShader(Device, ByteCode);
                Signature = ShaderSignature.GetInputSignature(ByteCode);
            }

            using (var ByteCode = ShaderBytecode.CompileFromFile("D3D/PixelShader.hlsl", "PS", "ps_4_0"))
            {
                PS = new PixelShader(Device, ByteCode);
            }
            
            Context.VertexShader.Set(VS);
            Context.PixelShader.Set(PS);

            Context.InputAssembler.InputLayout = new InputLayout(Device, Signature, Vertex.Layout);
            Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            Context.VertexShader.SetConstantBuffer(0, ConstantMatrixBuffer = new Buffer(Device, new BufferDescription
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<ConstantMatrixLayout>(),
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            }));
            
            Context.PixelShader.SetConstantBuffer(0, LightBuffer = new Buffer(Device, new BufferDescription
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<LightLayout>(),
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            }));
            
            Context.PixelShader.SetSampler(0, Sampler = new SamplerState(Device, new SamplerStateDescription
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

            var LightColor = new[] { Procedure.Constants.Hue, Procedure.Constants.Saturation * 0.75f, Procedure.Constants.Brightness * 0.75f + 0.25f }.ToRGB();
            Light = new LightLayout
            {
                Color = new Vector4(LightColor.R, LightColor.G, LightColor.B, 1),
                Direction = new Vector3(1, 2, 1),
                Padding = 0
            };

            Context.MapSubresource(LightBuffer, MapMode.WriteDiscard, MapFlags.None, out BufferStream);
            BufferStream.Write(Light);
            Context.UnmapSubresource(LightBuffer, 0);
        }

        internal void UpdateBuffers(Vector3 MoveWorld, float Rotate, float Scale)
        {
            ConstantMatrix.World = Matrix.Scaling(Scale) * Matrix.RotationY(Rotate) * Matrix.Translation(MoveWorld);
            ConstantMatrix.World.Transpose();
            ConstantMatrix.View = Input.Camera.View;
            ConstantMatrix.WVP = ConstantMatrix.Projection * ConstantMatrix.View * ConstantMatrix.World;

            Context.MapSubresource(ConstantMatrixBuffer, MapMode.WriteDiscard, MapFlags.None, out BufferStream);
            BufferStream.Write(ConstantMatrix);
            Context.UnmapSubresource(ConstantMatrixBuffer, 0);
        }

        public void Dispose()
        {
            Utilities.Dispose(ref BufferStream);
            Utilities.Dispose(ref LightBuffer);
            Utilities.Dispose(ref Sampler);
            Utilities.Dispose(ref ConstantMatrixBuffer);
            Utilities.Dispose(ref PS);
            Utilities.Dispose(ref VS);
            Utilities.Dispose(ref Signature);
        }
    }
}
