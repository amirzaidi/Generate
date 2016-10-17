using SharpDX.Mathematics.Interop;

namespace Generate.Procedure
{
    static class Constants
    {
        internal static float Hue;
        internal static float Saturation;
        internal static float Brightness;

        internal static RawColor4 BG;

        internal static int AvgTexDensity;

        internal static void Load()
        {
            var Rand = new Worker("constants");

            Hue = (float)Rand.NextDouble();
            Saturation = (float)Rand.NextDouble();
            Brightness = (float)Rand.NextDouble();
            BG = new[] { Hue, Saturation, Brightness }.ToRGB();

            AvgTexDensity = Rand.Next(1, 6);
        }
    }
}
