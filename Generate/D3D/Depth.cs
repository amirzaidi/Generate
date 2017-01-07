using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using System;
using SharpDX;

namespace Generate.D3D
{
    class Depth : IDisposable
    {
        // Initialize and set up the description of the stencil state.
        private static DepthStencilStateDescription DepthStencilDesc = new DepthStencilStateDescription
        {
            IsDepthEnabled = true,
            DepthWriteMask = DepthWriteMask.All,
            DepthComparison = Comparison.Less,
            IsStencilEnabled = true,
            StencilReadMask = 0xFF,
            StencilWriteMask = 0xFF,
            // Stencil operation if pixel front-facing.
            FrontFace = new DepthStencilOperationDescription()
            {
                FailOperation = StencilOperation.Keep,
                DepthFailOperation = StencilOperation.Increment,
                PassOperation = StencilOperation.Keep,
                Comparison = Comparison.Always
            },
            // Stencil operation if pixel is back-facing.
            BackFace = new DepthStencilOperationDescription()
            {
                FailOperation = StencilOperation.Keep,
                DepthFailOperation = StencilOperation.Decrement,
                PassOperation = StencilOperation.Keep,
                Comparison = Comparison.Always
            }
        };

        // Setup the raster description which will determine how and what polygon will be drawn.
        private static RasterizerStateDescription RasterizerDesc = new RasterizerStateDescription
        {
            IsAntialiasedLineEnabled = false,
            CullMode = CullMode.Back,
            DepthBias = 0,
            DepthBiasClamp = 0.0f,
            IsDepthClipEnabled = true,
            FillMode = FillMode.Solid,
            IsFrontCounterClockwise = false,
            IsMultisampleEnabled = true,
            IsScissorEnabled = false,
            SlopeScaledDepthBias = 0.0f
        };

        private static DepthStencilState DepthStencilState;
        private static RasterizerState RasterizerState;

        internal static void InitStencilRasterState(Device Device)
        {
            DepthStencilState = new DepthStencilState(Device, DepthStencilDesc);
            Device.ImmediateContext.OutputMerger.SetDepthStencilState(DepthStencilState, 1);

            RasterizerState = new RasterizerState(Device, RasterizerDesc);
            Device.ImmediateContext.Rasterizer.State = RasterizerState;
        }

        internal static void DisposeStates()
        {
            Utilities.Dispose(ref RasterizerState);
            Utilities.Dispose(ref DepthStencilState);
        }

        internal DepthStencilView DepthStencilView;
        private Texture2D DepthStencilTexture;
        private ModeDescription Resolution;

        internal Depth(Device Device, ModeDescription Resolution, SampleDescription AA)
        {
            this.Resolution = Resolution;
            
            DepthStencilTexture = new Texture2D(Device, new Texture2DDescription
            {
                Width = Resolution.Width,
                Height = Resolution.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R32_Typeless,
                SampleDescription = AA,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            DepthStencilView = new DepthStencilView(Device, DepthStencilTexture, new DepthStencilViewDescription
            {
                Format = Format.D32_Float,
                Dimension = (AA.Count > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D),
                Texture2D = new DepthStencilViewDescription.Texture2DResource
                {
                    MipSlice = 0
                }
            });
        }

        internal void Prepare(DeviceContext Context)
        {
            Context.Rasterizer.SetViewport(0, 0, Resolution.Width, Resolution.Height, 0, 1);
            Context.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
        }

        public void Dispose()
        {
            Utilities.Dispose(ref DepthStencilView);
            Utilities.Dispose(ref DepthStencilTexture);
        }
    }
}
