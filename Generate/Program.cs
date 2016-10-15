using Generate.D3D;
using System;

namespace Generate
{
    class Program
    {
        internal static Renderer Renderer;

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

            using (var Window = new Window())
            using (Renderer = new Renderer(Window))
            {
                Window.Show();
            }

            Console.ReadKey();
        }

        static void LogLine(object In, string From = null)
            => Log(In + "\r\n", From);

        static void Log(object In, string From = null)
            => Console.Write($"[{DateTime.Now.ToLongTimeString()}] {From ?? "Main"} - {In}");
    }
}
