using Generate.D2D;
using Generate.D3D;
using Generate.Input;
using System;
using System.Threading.Tasks;

namespace Generate
{
    class Program
    {
        internal static Renderer Renderer;
        private static LoopWindow Window;
        private static Overlay Overlay;

        internal static bool Close = false;
        internal static int VSync = 1;
        internal static SharpDX.Color4 MainColor;
        private static uint Frames = 0;

        static void Main(string[] args)
        {
            Log("Seed? ");
            Procedure.Worker.Master = new Procedure.Master(Console.ReadLine().AsciiBytes());
            LogLine(new Procedure.Worker("Main".AsciiBytes()).Next());
            LogLine(new Procedure.Worker("Slave".AsciiBytes()).Next());
            LogLine(new Procedure.Worker("Depth".AsciiBytes()).Next());
            LogLine(new Procedure.Worker("Colors".AsciiBytes()).Next());
            LogLine(new Procedure.Worker("Test A Longer String LOL".AsciiBytes()).Next());
            LogLine(new Procedure.Worker("¥¬".AsciiBytes()).Next());

            var BGRandom = new Procedure.Worker("bg");
            MainColor = new[] { (float)BGRandom.NextDouble(), 0.5f + (float)BGRandom.NextDouble() / 2f, 0.5f + (float)BGRandom.NextDouble() / 2f }.ToRGB();
            KeyboardMouse.StartCapture();

            using (Window = new LoopWindow())
            using (Renderer = new Renderer(Window))
            using (Overlay = new Overlay(Renderer.Device, Renderer.AntiAliasedBackBuffer))
            using (var Loop = Window.Loop())
            {
                while (!Close && Loop.NextFrame())
                {
                    Frame();
                    Frames++;
                }
            }
        }

        static void Frame()
        {
            var UpdateTask = Processor.Process();
            using (var RenderTarget = Renderer.PrepareFrame(MainColor))
            {
                Task.WhenAll(UpdateTask);
                Content.Chunk.RenderVisible();
            }

            Overlay.Start();
            Overlay.DrawCrosshair();
            Overlay.Draw($"Coords ({Camera.Position.X}, {Camera.Position.Y}, {Camera.Position.Z})", 10, 10, 500, 20);
            Overlay.Draw($"Rotation ({Camera.RotationX}, {Camera.RotationY})", 10, 30, 500, 20);
            Overlay.Draw($"Frames ({Frames}, VSync {VSync})", 10, 50, 500, 20);
            Overlay.End();

            Renderer.FinishFrame(VSync);
        }

        static void LogLine(object In, string From = null)
            => Log(In + "\r\n", From);

        static void Log(object In, string From = null)
            => Console.Write($"[{DateTime.Now.ToLongTimeString()}] {From ?? "Main"} - {In}");
    }
}
