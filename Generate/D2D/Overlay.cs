using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using System;

namespace Generate.D2D
{
    using SharpDX.Direct2D1;
    class Overlay : IDisposable
    {
        private TextFormat TextFormat;
        internal DeviceContext Context2D;
        private SolidColorBrush Brush;
        private static BitmapProperties1 Properties = new BitmapProperties1(new PixelFormat(D3D.Renderer.FormatRGB, AlphaMode.Premultiplied), 96, 96, BitmapOptions.Target | BitmapOptions.CannotDraw);

        internal Overlay(SharpDX.Direct3D11.Device Device3D, Texture2D BackBuffer)
        {
            using (var FontFactory = new SharpDX.DirectWrite.Factory())
            {
                TextFormat = new TextFormat(FontFactory, "Segoe UI", 16.0f);
            }

            using (var Device2D = new Device(Device3D.QueryInterface<SharpDX.DXGI.Device>()))
            {
                Context2D = new DeviceContext(Device2D, DeviceContextOptions.EnableMultithreadedOptimizations);
            }

            Context2D.PrimitiveBlend = PrimitiveBlend.SourceOver;
            Brush = new SolidColorBrush(Context2D, Color.White);

            SetBackBuffer(BackBuffer);
        }

        internal void SetBackBuffer(Texture2D BackBuffer)
        {
            Context2D.Target = new Bitmap1(Context2D, BackBuffer.QueryInterface<Surface>());
        }

        internal void Start()
        {
            Context2D.BeginDraw();
        }

        internal void Draw(string Text, int X, int Y, int Width, int Height)
        {
            Context2D.DrawText(Text, TextFormat, new RectangleF(X, Y, Width, Height), Brush);
        }

        internal void DrawCrosshair()
        {
            var MidWidth = Program.Renderer.Resolution.Width / 2;
            var MidHeight = Program.Renderer.Resolution.Height / 2;

            Context2D.DrawLine(new SharpDX.Mathematics.Interop.RawVector2(MidWidth, MidHeight - 10), new SharpDX.Mathematics.Interop.RawVector2(MidWidth, MidHeight + 10), Brush);
            Context2D.DrawLine(new SharpDX.Mathematics.Interop.RawVector2(MidWidth - 10, MidHeight), new SharpDX.Mathematics.Interop.RawVector2(MidWidth + 10, MidHeight), Brush);
        }

        internal void End()
        {
            Context2D.EndDraw();
        }

        public void Dispose()
        {
            Utilities.Dispose(ref Brush);
            Utilities.Dispose(ref TextFormat);
            Context2D?.Target?.Dispose();
            Utilities.Dispose(ref Context2D);
        }
    }
}
