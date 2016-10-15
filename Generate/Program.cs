using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generate
{
    class Program
    {
        static void Main(string[] args)
        {
            Log("Seed? ");
            Procedure.Worker.Master = new Procedure.Master(Encoding.ASCII.GetBytes(Console.ReadLine()));
            LogLine(Procedure.Worker.Master.GetSeed(Encoding.ASCII.GetBytes("Main")));
            LogLine(Procedure.Worker.Master.GetSeed(Encoding.ASCII.GetBytes("Slave")));
            LogLine(Procedure.Worker.Master.GetSeed(Encoding.ASCII.GetBytes("Depth")));
            LogLine(Procedure.Worker.Master.GetSeed(Encoding.ASCII.GetBytes("Colors")));
            LogLine(Procedure.Worker.Master.GetSeed(Encoding.ASCII.GetBytes("Test A Longer String LOL")));
            LogLine(Procedure.Worker.Master.GetSeed(Encoding.ASCII.GetBytes("¥¬")));

            Log(new Random(Procedure.Worker.Master.GetSeed(Encoding.ASCII.GetBytes("negative"))).Next());

            Console.ReadLine();
        }

        static void LogLine(object In, string From = null)
            => Log(In + "\r\n", From);

        static void Log(object In, string From = null)
            => Console.Write($"[{DateTime.Now.ToLongTimeString()}] {From ?? "Main"} - {In}");
    }
}
