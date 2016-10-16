using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using System;
using System.Linq;
using SharpDX;
using SharpDX.Mathematics.Interop;

namespace Generate.D3D
{
    class Renderer : IDisposable
    {
        internal static SampleDescription AntiAliasing = new SampleDescription(8, 0);
        internal Device Device;
        private SwapChain1 SwapChain;
        private DeviceDebug Debug;
        private Depth Depth;
        private Shader Shader;
        internal Texture2D AntiAliasedBackBuffer;

        internal Renderer(LoopWindow Window)
        {
            var SwapChainDescription = new SwapChainDescription1()
            {
                AlphaMode = AlphaMode.Unspecified,
                BufferCount = 2,
                Flags = SwapChainFlags.AllowModeSwitch,
                Format = Format.R8G8B8A8_UNorm,
                Height = 768,
                SampleDescription = new SampleDescription(1, 0),
                Scaling = Scaling.None,
                Stereo = false,
                SwapEffect = SwapEffect.FlipDiscard,
                Usage = Usage.RenderTargetOutput,
                Width = 1024
            };

            using (var Factory = new Factory2())
            {
                Device = new Device(Factory.Adapters1[0], DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport);
                SwapChain = new SwapChain1(Factory, Device, Window.Handle, ref SwapChainDescription);
            }

            Debug = Device.QueryInterface<DeviceDebug>();
            Debug.ReportLiveDeviceObjects(ReportingLevel.Detail);

            var Resolution = GetResolution();
            Resolution.Format = Format.R8G8B8A8_UNorm;

            Window.Borderless(Resolution.Width, Resolution.Height);

            SwapChain.ResizeTarget(ref Resolution);
            SwapChain.ResizeBuffers(2, Resolution.Width, Resolution.Height, Resolution.Format, SwapChainFlags.None);
            
            Depth = new Depth(Device, Resolution);

            var Perspective = Matrix.PerspectiveFovLH((float)(Math.PI / 4), (float)(Resolution.Width) / Resolution.Height, Depth.ScreenNear, Depth.ScreenFar);
            Shader = new Shader(Device, Perspective);

            AntiAliasedBackBuffer = new Texture2D(Device, new Texture2DDescription
            {
                Format = Resolution.Format,
                Width = Resolution.Width,
                Height = Resolution.Height,
                ArraySize = 1,
                MipLevels = 1,
                BindFlags = BindFlags.RenderTarget,
                SampleDescription = AntiAliasing
            });
        }

        private ModeDescription GetResolution()
        {
            var Resolution = new ModeDescription
            {
                Width = 0,
                Height = 0
            };

            using (var Factory = SwapChain.GetParent<Factory>())
            {
                //Factory.MakeWindowAssociation(Window.Handle, WindowAssociationFlags.IgnoreAll);

                foreach (var Output in Factory.Adapters.First().Outputs)
                {
                    foreach (Format Format in Enum.GetValues(typeof(Format)))
                    {
                        foreach (var PossibleResolution in Output.GetDisplayModeList(Format, 0))
                        {
                            if (PossibleResolution.Scaling == DisplayModeScaling.Unspecified && PossibleResolution.Width >= Resolution.Width && PossibleResolution.Height >= Resolution.Height)
                            {
                                Resolution = PossibleResolution;
                            }
                        }
                    }
                }
            }

            return Resolution;
        }

        internal RenderTargetView PrepareFrame(RawColor4 Background)
        {
            var TargetView = new RenderTargetView(Device, AntiAliasedBackBuffer);
            Device.ImmediateContext.OutputMerger.SetTargets(Depth.DepthStencilView, TargetView);
            Device.ImmediateContext.ClearDepthStencilView(Depth.DepthStencilView, DepthStencilClearFlags.Depth, 1, 0);
            Device.ImmediateContext.ClearRenderTargetView(TargetView, Background);
            return TargetView;
        }

        internal void FinishFrame(int VSync)
        {
            using (var Surface = SwapChain.GetBackBuffer<Texture2D>(0))
            {
                Device.ImmediateContext.ResolveSubresource(AntiAliasedBackBuffer, 0, Surface, 0, Format.R8G8B8A8_UNorm);
            }

            SwapChain.Present(VSync, PresentFlags.None);
        }

        public void Dispose()
        {
            Utilities.Dispose(ref Shader);
            Utilities.Dispose(ref Depth);

            if (Device?.ImmediateContext != null)
            {
                Device.ImmediateContext.ClearState();
                Device.ImmediateContext.Flush();
                Device.ImmediateContext.Dispose();
            }

            Utilities.Dispose(ref Device);
            Utilities.Dispose(ref SwapChain);
            Utilities.Dispose(ref Debug);
        }
    }
}
