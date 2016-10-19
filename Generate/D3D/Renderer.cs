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
        private Depth Depth;
        private Depth ShadowDepth;
        private ModeDescription Resolution = new ModeDescription
        {
            Width = 0,
            Height = 0
        };

        internal Shader Shader;
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

                foreach (var Output in Factory.Adapters.First().Outputs)
                {
                    foreach (var PossibleResolution in Output.GetDisplayModeList(Format.R8G8B8A8_UNorm, 0))
                    {
                        if (PossibleResolution.Scaling == DisplayModeScaling.Unspecified && PossibleResolution.Width >= Resolution.Width && PossibleResolution.Height >= Resolution.Height)
                        {
                            Resolution = PossibleResolution;
                        }
                    }
                }
            }

            Window.Borderless(Resolution.Width, Resolution.Height);
            ResizeBuffers();

            var Perspective = Matrix.PerspectiveFovLH((float)(Math.PI / 2), (float)(Resolution.Width) / Resolution.Height, Depth.ScreenNear, Depth.ScreenFar);
            Perspective.Transpose();
            Shader = new Shader(Device, Perspective);

            Depth = new Depth(Device, Resolution, AntiAliasing);
            ShadowDepth = new Depth(Device, new ModeDescription
            {
                Width = 1024 * 4,
                Height = 1024 * 4
            }, new SampleDescription(1, 0));

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

        private void ResizeBuffers()
        {
            SwapChain.ResizeTarget(ref Resolution);
            SwapChain.ResizeBuffers(2, Resolution.Width, Resolution.Height, Resolution.Format, SwapChainFlags.AllowModeSwitch);
        }

        internal RenderTargetView PrepareShadow()
        {
            ShadowDepth.Prepare(Device.ImmediateContext);

            var TargetView = new RenderTargetView(Device, Shader.ShadowDepthMap);
            Device.ImmediateContext.OutputMerger.SetRenderTargets(ShadowDepth.DepthStencilView, TargetView);
            Device.ImmediateContext.ClearRenderTargetView(TargetView, new RawColor4(0, 0, 0, 1));

            var ShadowCoords = Input.Camera.Position;
            ShadowCoords.Y = (float)Math.Pow(2, 7);
            Shader.DepthMode(Device.ImmediateContext, ShadowCoords);
            
            return TargetView;
        }

        internal RenderTargetView PrepareFrame(RawColor4 Background)
        {
            Depth.Prepare(Device.ImmediateContext);

            var TargetView = new RenderTargetView(Device, AntiAliasedBackBuffer);
            Device.ImmediateContext.OutputMerger.SetRenderTargets(Depth.DepthStencilView, TargetView);
            Device.ImmediateContext.ClearRenderTargetView(TargetView, Background);

            Shader.RenderMode(Device.ImmediateContext);

            return TargetView;
        }

        internal void FinishFrame(int VSync)
        {
            using (var Surface = SwapChain.GetBackBuffer<Texture2D>(0))
            {
                Device.ImmediateContext.ResolveSubresource(AntiAliasedBackBuffer, 0, Surface, 0, Format.R8G8B8A8_UNorm);
            }

            if ((VSync == 0) != SwapChain.IsFullScreen)
            {
                SwapChain.SetFullscreenState(VSync == 0, null);
                ResizeBuffers();
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
        }
    }
}
