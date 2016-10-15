using System;
using System.Text;

namespace Generate.Procedure
{
    class Worker : Random
    {
        internal static Master Master;

        public Worker(string For) : this(Encoding.ASCII.GetBytes(For))
        {
        }

        public Worker(byte[] For) : base(Master.GetSeed(For))
        {
        }
    }
}
