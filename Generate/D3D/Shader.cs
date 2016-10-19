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
        internal struct MatricesLayout
        {
            internal Matrix World;
            internal Matrix View;
            internal Matrix Projection;
            internal Matrix WVP;

            internal Matrix LightView;
            internal Matrix LightProjection;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct VertexLightLayout
        {
            internal Vector3 LightPosition;
            internal float UseLight;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PixelLightLayout
        {
            internal Vector3 LightColor;
            internal float UseLight;
        }

        private DeviceContext Context;

        private ShaderSignature Signature;
        private VertexShader VS;
        private PixelShader PS;

        private DataStream BufferStream;

        internal MatricesLayout Matrices;
        private Buffer MatricesBuffer;

        private VertexLightLayout VertexLight;
        private Buffer VertexLightBuffer;

        private PixelLightLayout PixelLight;
        private Buffer PixelLightBuffer;

        private bool UseLight
        {
            set
            {
                VertexLight.UseLight = PixelLight.UseLight = (value ? 1 : 0);

                Context.MapSubresource(VertexLightBuffer, MapMode.WriteDiscard, MapFlags.None, out BufferStream);
                BufferStream.Write(VertexLight);
                Context.UnmapSubresource(VertexLightBuffer, 0);

                Context.MapSubresource(PixelLightBuffer, MapMode.WriteDiscard, MapFlags.None, out BufferStream);
                BufferStream.Write(PixelLight);
                Context.UnmapSubresource(PixelLightBuffer, 0);
            }
        }

        private SamplerState Sampler;
        internal Resource ShadowDepthMap;
        private ShaderResourceView ShadowDepthMapView;

        internal Shader(Device Device, Matrix Perspective)
        {
            Matrices.Projection = Perspective;
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

            Context.VertexShader.SetConstantBuffer(0, MatricesBuffer = new Buffer(Device, new BufferDescription
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<MatricesLayout>(),
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            }));

            Context.VertexShader.SetConstantBuffer(1, VertexLightBuffer = new Buffer(Device, new BufferDescription
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<VertexLightLayout>(),
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            }));

            Context.PixelShader.SetConstantBuffer(0, PixelLightBuffer = new Buffer(Device, new BufferDescription
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<PixelLightLayout>(),
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

            VertexLight = new VertexLightLayout
            {
                LightPosition = Input.Camera.Position
            };

            var LightColor = new[] { Procedure.Constants.Hue, Procedure.Constants.Saturation * 0.75f, Procedure.Constants.Brightness * 0.75f + 0.25f }.ToRGB();
            PixelLight = new PixelLightLayout
            {
                LightColor = new Vector3(LightColor)
            };
            
            Matrices.LightProjection = Matrix.PerspectiveFovLH((float)(Math.PI / 2.0f), 1, Depth.ScreenNear, Depth.ScreenFar);
            Matrices.LightProjection.Transpose();

            ShadowDepthMap = new Texture2D(Device, new Texture2DDescription
            {
                Width = 1024 * 4,
                Height = 1024 * 4,
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0)
            });

            ShadowDepthMapView = new ShaderResourceView(Device, ShadowDepthMap);
        }

        internal void DepthMode(DeviceContext Context, Vector3 Position)
        {
            VertexLight.LightPosition = Position;

            Matrices.LightView = Matrix.LookAtLH(Position, Position + new Vector3(0, -1, 0.00001f), Vector3.UnitY);
            Matrices.LightView.Transpose();

            Context.PixelShader.SetShaderResource(1, null);
            UseLight = false;
        }

        internal void RenderMode(DeviceContext Context)
        {
            Context.PixelShader.SetShaderResource(1, ShadowDepthMapView);
            UseLight = true;
        }

        internal void UpdateMatrices(Vector3 MoveWorld, float Rotate, float Scale)
        {
            Matrices.World = Matrix.Scaling(Scale) * Matrix.RotationY(Rotate) * Matrix.Translation(MoveWorld);
            Matrices.World.Transpose();
            Matrices.View = Input.Camera.View;
            Matrices.WVP = Matrices.Projection * Matrices.View * Matrices.World;

            Context.MapSubresource(MatricesBuffer, MapMode.WriteDiscard, MapFlags.None, out BufferStream);
            BufferStream.Write(Matrices);
            Context.UnmapSubresource(MatricesBuffer, 0);
        }

        public void Dispose()
        {
            Utilities.Dispose(ref ShadowDepthMapView);
            Utilities.Dispose(ref ShadowDepthMap);
            Utilities.Dispose(ref BufferStream);
            Utilities.Dispose(ref VertexLightBuffer);
            Utilities.Dispose(ref Sampler);
            Utilities.Dispose(ref MatricesBuffer);
            Utilities.Dispose(ref PS);
            Utilities.Dispose(ref VS);
            Utilities.Dispose(ref Signature);
        }
    }
}
