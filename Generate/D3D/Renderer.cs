using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using System;
using System.Linq;
using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.Direct3D;

namespace Generate.D3D
{
    class Renderer : IDisposable
    {
        internal Device Device;
        private SwapChain SwapChain;
        private DeviceDebug Debug;

        internal const Format FormatRGB = Format.R8G8B8A8_UNorm;
        internal ModeDescription Resolution = new ModeDescription
        {
            Width = 0,
            Height = 0
        };

        internal Texture2D AntiAliasedBackBuffer;
        
        private ShadowShader ShadowShader;
        private Depth ShadowDepth;

        private CameraShader CameraShader;
        private Depth CameraDepth;

        internal IShader ActiveShader;
        
        internal Renderer(LoopWindow Window)
        {
            var Levels = new[]
            {
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0,
                FeatureLevel.Level_9_3,
                FeatureLevel.Level_9_2,
                FeatureLevel.Level_9_1,
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport | (Program.DebugMode ? DeviceCreationFlags.Debug : DeviceCreationFlags.None), Levels, new SwapChainDescription
            {
                BufferCount = 1,
                Flags = SwapChainFlags.AllowModeSwitch,
                IsWindowed = true,
                ModeDescription = new ModeDescription
                {
                    Format = FormatRGB
                },
                OutputHandle = Window.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            }, out Device, out SwapChain);
            

            var Bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            
            using (var Factory = SwapChain.GetParent<Factory>())
            {
                foreach (var Output in Factory.Adapters.First().Outputs)
                {
                    foreach (var PossibleResolution in Output.GetDisplayModeList(FormatRGB, 0))
                    {
                        if (PossibleResolution.Width == Bounds.Width && PossibleResolution.Height == Bounds.Height)
                        {
                            Resolution = PossibleResolution;
                        }
                    }
                }
            }

            Window.Borderless(Resolution.Width, Resolution.Height);
            ResizeBuffers();

            Depth.InitStencilRasterState(Device);
            LoadWithAA(1);

            Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            if (Program.DebugMode)
            {
                Debug = new DeviceDebug(Device);
            }
        }

        internal void LoadWithAA(int AA)
        {
            var AntiAliasing = new SampleDescription(AA, 0);

            Utilities.Dispose(ref ShadowShader);
            ShadowShader = new ShadowShader(Device);

            Utilities.Dispose(ref AntiAliasedBackBuffer);
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
            
            Utilities.Dispose(ref CameraDepth);
            CameraDepth = new Depth(Device, Resolution, AntiAliasing);

            Utilities.Dispose(ref ShadowDepth);
            ShadowDepth = new Depth(Device, new ModeDescription
            {
                Width = 1500 * AA,
                Height = 1500 * AA
            }, new SampleDescription(1, 0));

            Utilities.Dispose(ref CameraShader);
            CameraShader = new CameraShader(Device, Resolution, ShadowDepth.DepthStencilView.Resource, AA);
        }

        private void ResizeBuffers()
        {
            SwapChain.ResizeTarget(ref Resolution);
            SwapChain.ResizeBuffers(2, Resolution.Width, Resolution.Height, Resolution.Format, SwapChainFlags.AllowModeSwitch);
        }

        internal void PrepareShadow()
        {
            ShadowDepth.Prepare(Device.ImmediateContext);
            Device.ImmediateContext.OutputMerger.SetRenderTargets(ShadowDepth.DepthStencilView, (RenderTargetView)null);

            ShadowShader.Prepare();
            ActiveShader = ShadowShader;
        }

        internal void EndShadow()
        {
            ShadowShader.End();
        }

        internal RenderTargetView PrepareCamera(RawColor4 Background)
        {
            CameraDepth.Prepare(Device.ImmediateContext);

            var TargetView = new RenderTargetView(Device, AntiAliasedBackBuffer);
            Device.ImmediateContext.OutputMerger.SetRenderTargets(CameraDepth.DepthStencilView, TargetView);
            Device.ImmediateContext.ClearRenderTargetView(TargetView, Background);

            CameraShader.VertexLight.LightVP = ShadowShader.LightVP;
            CameraShader.Prepare();
            ActiveShader = CameraShader;

            return TargetView;
        }

        internal void EndCamera(int VSync)
        {
            using (var BackBuffer = SwapChain.GetBackBuffer<Texture2D>(0))
            {
                Device.ImmediateContext.ResolveSubresource(AntiAliasedBackBuffer, 0, BackBuffer, 0, FormatRGB);
            }

            if ((VSync == 0) != SwapChain.IsFullScreen)
            {
                SwapChain.SetFullscreenState(VSync == 0, null);
                ResizeBuffers();
            }

            SwapChain.Present(VSync, PresentFlags.None);
            CameraShader.End();
        }

        public void Dispose()
        {
            Utilities.Dispose(ref CameraShader);
            Utilities.Dispose(ref CameraDepth);

            Utilities.Dispose(ref ShadowShader);
            Utilities.Dispose(ref ShadowDepth);

            Depth.DisposeStates();

            if (Device?.ImmediateContext != null)
            {
                Device.ImmediateContext.ClearState();
                Device.ImmediateContext.Flush();
                Device.ImmediateContext.Dispose();
            }

            Utilities.Dispose(ref Device);
            Utilities.Dispose(ref SwapChain);

            Debug?.ReportLiveDeviceObjects(ReportingLevel.Detail | ReportingLevel.IgnoreInternal);
            Utilities.Dispose(ref Debug);
        }
    }
}
