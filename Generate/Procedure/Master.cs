using System;

namespace Generate.Procedure
{
    class Master
    {
        private byte[] Seed;

        internal Master(byte[] Seed)
        {
            this.Seed = new byte[Seed.Length + 4];
            Seed.CopyTo(this.Seed, 4);

            this.Seed[0] = (byte)(Seed.Length & 0xff);
            this.Seed[1] = (byte)((Seed.Length >> 8) & 0xff);
            this.Seed[2] = (byte)((Seed.Length >> 16) & 0xff);
            this.Seed[3] = (byte)((Seed.Length >> 24) & 0xff);
        }

        internal int GetSeed(byte[] For)
        {
            int Stride = 0;
            for (int i = 0; i < For.Length; i++)
            {
                Stride += For[i];
            }

            Stride %= Seed.Length;

            var Result = new byte[4];
            for (int i = 0; i < For.Length; i++)
            {
                int Place = For[i] * Stride % Seed.Length;
                Result[i % 4] ^= Seed[Place];
            }
            
            return (2 * (Stride % 2) - 1) * BitConverter.ToInt32(Result, 0);
        }
    }
}
