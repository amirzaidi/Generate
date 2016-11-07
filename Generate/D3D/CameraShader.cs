﻿using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Resource = SharpDX.Direct3D11.Resource;
using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;

namespace Generate.D3D
{
    class CameraShader : IShader
    {
        [StructLayout(LayoutKind.Sequential)]
        struct MatricesLayout
        {
            internal Matrix World;
            internal Matrix CameraWVP;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct VertexLightLayout
        {
            internal Matrix LightVP;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PixelLightLayout
        {
            internal Vector4 LightColor;
            internal Vector3 LightDirection;
            internal float StartLight;
        }

        private MatricesLayout Matrices;
        private Buffer MatricesBuffer;

        internal VertexLightLayout VertexLight;
        private Buffer VertexLightBuffer;

        private PixelLightLayout PixelLight;
        private Buffer PixelLightBuffer;

        private SamplerState Sampler;
        private ShaderResourceView ShadowDepthMapView;

        internal CameraShader(Device Device, ModeDescription Resolution, Resource ShadowDepthBackbuffer)
        {
            Projection = Matrix.Transpose(Matrix.PerspectiveFovLH((float)(Math.PI / 2), (float)(Resolution.Width) / Resolution.Height, 0.1f, 2 * 1024f));
            Context = Device.ImmediateContext;

            using (var ByteCode = ShaderBytecode.CompileFromFile("D3D/CameraShaderVertex.hlsl", "VS", "vs_4_0"))
            {
                VS = new VertexShader(Device, ByteCode);
                Signature = ShaderSignature.GetInputSignature(ByteCode);
            }

            using (var ByteCode = ShaderBytecode.CompileFromFile("D3D/CameraShaderPixel.hlsl", "PS", "ps_4_0"))
            {
                PS = new PixelShader(Device, ByteCode);
            }

            InputLayout = new InputLayout(Device, Signature, Vertex.Layout);

            MatricesBuffer = new Buffer(Device, new BufferDescription
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<MatricesLayout>(),
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            });

            VertexLightBuffer = new Buffer(Device, new BufferDescription
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<VertexLightLayout>(),
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            });

            PixelLightBuffer = new Buffer(Device, new BufferDescription
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<PixelLightLayout>(),
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            });

            Sampler = new SamplerState(Device, new SamplerStateDescription
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
            });

            PixelLight = new PixelLightLayout
            {
                LightColor = new Vector4(new[]
                {
                    Procedure.Constants.Hue,
                    Procedure.Constants.Saturation * 0.75f,
                    Procedure.Constants.Brightness * 0.75f + 0.25f
                }.ToRGB())
            };
            
            ShadowDepthMapView = new ShaderResourceView(Device, ShadowDepthBackbuffer, new ShaderResourceViewDescription
            {
                Format = Format.R24_UNorm_X8_Typeless,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource
                {
                    MostDetailedMip = 0,
                    MipLevels = -1
                }
            });
        }

        internal override void Prepare()
        {
            base.Prepare();

            Context.VertexShader.SetConstantBuffer(0, MatricesBuffer);
            Context.VertexShader.SetConstantBuffer(1, VertexLightBuffer);

            Context.MapSubresource(VertexLightBuffer, MapMode.WriteDiscard, MapFlags.None, out BufferStream);
            BufferStream.Write(VertexLight);
            Context.UnmapSubresource(VertexLightBuffer, 0);

            Context.PixelShader.SetConstantBuffer(0, PixelLightBuffer);

            PixelLight.LightDirection = -Content.Sun.Main.LightDirection;
            PixelLight.StartLight = 0.05f;

            Context.MapSubresource(PixelLightBuffer, MapMode.WriteDiscard, MapFlags.None, out BufferStream);
            BufferStream.Write(PixelLight);
            Context.UnmapSubresource(PixelLightBuffer, 0);

            Context.PixelShader.SetSampler(0, Sampler);
            Context.PixelShader.SetShaderResource(1, ShadowDepthMapView);

            Context.InputAssembler.InputLayout = InputLayout;
        }

        internal void DisableLighting()
        {
            PixelLight.StartLight = 1f;

            Context.MapSubresource(PixelLightBuffer, MapMode.WriteDiscard, MapFlags.None, out BufferStream);
            BufferStream.Write(PixelLight);
            Context.UnmapSubresource(PixelLightBuffer, 0);
        }

        internal override void UpdateWorld(Matrix World)
        {
            Matrices.World = Matrix.Transpose(World);
            Matrices.CameraWVP = Projection * Input.Camera.View * Matrices.World;

            Context.MapSubresource(MatricesBuffer, MapMode.WriteDiscard, MapFlags.None, out BufferStream);
            BufferStream.Write(Matrices);
            Context.UnmapSubresource(MatricesBuffer, 0);
        }

        internal override void End()
        {
            Context.VertexShader.SetConstantBuffer(0, null);
            Context.VertexShader.SetConstantBuffer(1, null);

            Context.PixelShader.SetConstantBuffer(0, null);
            Context.PixelShader.SetSampler(0, null);
            Context.PixelShader.SetShaderResource(1, null);
        }

        public override void Dispose()
        {
            Utilities.Dispose(ref ShadowDepthMapView);
            Utilities.Dispose(ref Sampler);
            Utilities.Dispose(ref PixelLightBuffer);
            Utilities.Dispose(ref VertexLightBuffer);
            Utilities.Dispose(ref MatricesBuffer);

            base.Dispose();
        }
    }
}
