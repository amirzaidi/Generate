using Generate.D2D;
using Generate.D3D;
using Generate.Input;
using Generate.Procedure;
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
        private static uint Frames = 0;

        static void Main(string[] args)
        {
            Log("Seed? ");
            Worker.Master = new Master(Console.ReadLine().AsciiBytes());
            Constants.Load();
            
            using (Window = new LoopWindow())
            using (Renderer = new Renderer(Window))
            using (Overlay = new Overlay(Renderer.Device, Renderer.AntiAliasedBackBuffer))
            using (var Loop = Window.Loop())
            {
                KeyboardMouse.StartCapture();

                while (!Close && Loop.NextFrame())
                {
                    Frame();
                    Frames++;
                }
            }
        }

        static void Frame()
        {
            Task.WhenAll(Processor.Process());

            using (var RenderTarget = Renderer.PrepareShadow())
            {
                Content.Chunk.RenderVisible();
            }

            using (var RenderTarget = Renderer.PrepareFrame(Constants.BG))
            {
                Content.Chunk.RenderVisible();
            }

            Overlay?.Start();
            Overlay?.DrawCrosshair();
            Overlay?.Draw($"Coords ({Camera.Position.X}, {Camera.Position.Y}, {Camera.Position.Z})", 10, 10, 500, 20);
            Overlay?.Draw($"Rotation ({Camera.RotationX}, {Camera.RotationY})", 10, 30, 500, 20);
            Overlay?.Draw($"Frames ({Frames}, VSync {VSync})", 10, 50, 500, 20);
            Overlay?.End();

            Renderer.FinishFrame(VSync);
        }

        static void LogLine(object In, string From = null)
            => Log(In + "\r\n", From);

        static void Log(object In, string From = null)
            => Console.Write($"[{DateTime.Now.ToLongTimeString()}] {From ?? "Main"} - {In}");
    }
}
