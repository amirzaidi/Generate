using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using System;
using SharpDX;

namespace Generate.D3D
{
    class Depth : IDisposable
    {
        private DepthStencilState DepthStencilState;
        public DepthStencilView DepthStencilView;
        private RasterizerState RasterizerState;

        public Depth(Device Device, ModeDescription Resolution)
        {
            DepthStencilState = new DepthStencilState(Device, StencilDesc);
            Device.ImmediateContext.OutputMerger.SetDepthStencilState(DepthStencilState, 1);

            DepthStencilView = new DepthStencilView(Device, new Texture2D(Device, BufferDesc(Resolution.Width, Resolution.Height)));

            // Create the rasterizer state from the description we just filled out and set the rasterizer state.
            RasterizerState = new RasterizerState(Device, RasterDesc);
            Device.ImmediateContext.Rasterizer.State = RasterizerState;

            // Setup and create the viewport for rendering.
            Device.ImmediateContext.Rasterizer.SetViewport(0, 0, Resolution.Width, Resolution.Height, 0, 1);
        }

        public void Dispose()
        {
            Utilities.Dispose(ref RasterizerState);

            if (DepthStencilView?.Resource != null)
            {
                DepthStencilView.Resource.Dispose();
            }

            Utilities.Dispose(ref DepthStencilView);
            Utilities.Dispose(ref DepthStencilState);
        }

        // Initialize and set up the description of the stencil state.
        private static DepthStencilStateDescription StencilDesc = new DepthStencilStateDescription
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

        private static Texture2DDescription BufferDesc(int Width, int Height)
        {
            // Initialize and set up the description of the depth buffer.
            return new Texture2DDescription
            {
                Width = Width,
                Height = Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription = Renderer.AA,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
        }

        // Initialize and set up the depth stencil view.
        private static DepthStencilViewDescription StencilViewDesc = new DepthStencilViewDescription
        {
            Format = Format.D24_UNorm_S8_UInt,
            Dimension = DepthStencilViewDimension.Texture2DMultisampled,
            Texture2D = new DepthStencilViewDescription.Texture2DResource
            {
                MipSlice = 0
            }
        };

        // Setup the raster description which will determine how and what polygon will be drawn.
        private static RasterizerStateDescription RasterDesc = new RasterizerStateDescription
        {
            IsAntialiasedLineEnabled = false,
            CullMode = CullMode.Back,
            DepthBias = 0,
            DepthBiasClamp = 0.0f,
            IsDepthClipEnabled = true,
            FillMode = FillMode.Solid,
            IsFrontCounterClockwise = false,
            IsMultisampleEnabled = false,
            IsScissorEnabled = false,
            SlopeScaledDepthBias = 0.0f
        };

        public static float ScreenFar = 10000.0f;
        public static float ScreenNear = 0.01f;
    }
}
