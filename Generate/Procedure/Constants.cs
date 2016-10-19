using SharpDX.Mathematics.Interop;
using System;

namespace Generate.Procedure
{
    static class Constants
    {
        internal static float Hue;
        internal static float Saturation;
        internal static float Brightness;

        internal static RawColor4 BG;

        internal static int AvgTexDensity;
        private static float HeightIntensity;

        internal static void Load()
        {
            var Rand = new Worker("constants");

            Hue = (float)Rand.NextDouble();
            Saturation = (float)Rand.NextDouble();
            Brightness = (float)(Rand.NextDouble() / 2f + 0.5f);

            var BGFloat = new[] { Hue, Saturation, Brightness }.ToRGB();
            BG = new RawColor4(BGFloat[0], BGFloat[1], BGFloat[2], 1);

            AvgTexDensity = Rand.Next(1, 6);
            HeightIntensity = (float)Math.Pow(Rand.NextDouble(), 3) * 64f;
        }

        internal static float[,] GetHeights(int X, int Z)
        {
            var Places = new float[2, 2];

            for (int dX = 0; dX < 2; dX++)
            {
                for (int dY = 0; dY < 2; dY++)
                {
                    Places[dX, dY] = (float)(Math.Sin(X + dX) * Math.Cos(Z + dY) * HeightIntensity) - HeightIntensity;
                }
            }

            return Places;
        }
    }
}
