using System;

namespace Generate.Procedure
{
    class Worker : Random
    {
        internal static Master Master;

        public Worker(string For) : this(For.ASCIIBytes())
        {
        }

        public Worker(byte[] For) : base(Master.GetSeed(For))
        {
        }
    }
}
