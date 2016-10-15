using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using System;
using System.Linq;
using SharpDX;

namespace Generate.D3D
{
    class Renderer : IDisposable
    {
        internal static SampleDescription AA = new SampleDescription(8, 0);
        private Device Device;
        private SwapChain1 SwapChain;
        private DeviceDebug Debug;
        private Depth Depth;

        internal Renderer(Window Window)
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
            Window.Borderless(Resolution.Width, Resolution.Height);

            SwapChain.ResizeTarget(ref Resolution);
            SwapChain.ResizeBuffers(2, Resolution.Width, Resolution.Height, Resolution.Format, SwapChainFlags.None);
            
            Depth = new Depth(Device, Resolution);
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

        public void Dispose()
        {
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
