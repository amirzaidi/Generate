using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Linq;

namespace Generate.Procedure
{
    static class Constants
    {
        internal static RawColor4 Color;
        internal static RawColor4 Background;
        internal static Vector4 Light;

        internal static int TextureDensityPower;
        internal static int TextureDensity;
        internal static float HeightIntensity;

        internal static Vector3 BaseLightDirection;
        private static double SinCos1;
        private static double SinCos2;

        internal static float StripeStart;
        internal static float StripeMultiplyFactor;

        internal static float DirtSmoothness;
        internal static bool RandomColor;

        internal static int BuildingDensity = 0;
        internal static int BuildingHeight;

        internal static void Load()
        {
            var Rand = new Worker("constants");

            var Hue = Rand.NextFloat();
            var Saturation = Rand.NextFloat();
            var Brightness = Rand.NextFloat();

            var Float = new[] { Hue, Saturation, Brightness }.ToRGB();
            Color = new RawColor4(Float[0], Float[1], Float[2], 1);

            Float = new[] { Hue, Saturation, Brightness * 0.5f + 0.5f }.ToRGB();
            Background = new RawColor4(Float[0], Float[1], Float[2], 1);

            Float = new[] { Hue, Saturation * 0.5f, Brightness * 0.5f + 0.5f }.ToRGB();
            Light = new Vector4(Float[0], Float[1], Float[2], 1);

            TextureDensityPower = Rand.Next(3, 8);
            TextureDensity = (int)Math.Pow(2, TextureDensityPower);
            HeightIntensity = (float)Math.Pow(Rand.NextDouble(), 5) * 128f;

            BaseLightDirection = new Vector3(
                Rand.NextFloat(-1f, 1f),
                Rand.NextFloat(-1f, -0.5f),
                Rand.NextFloat(-1f, 1f)
            );

            BaseLightDirection.Normalize();

            var Scale = (float)Math.Pow(2, Rand.Next(8, 17));
            SinCos1 = Rand.NextDouble() * Scale;
            SinCos2 = Rand.NextDouble() * Scale;

            Texture.Handlers = Texture.Handlers.Where(x => Rand.Next(0, 2) == 1).ToArray();

            StripeStart = Rand.NextFloat(0.4f, 1f);
            StripeMultiplyFactor = 1f - StripeStart;

            DirtSmoothness = Rand.NextFloat(0.5f, 1f);
            RandomColor = Rand.Next(0, 2) == 1;

            if (HeightIntensity < 10f)
            {
                BuildingDensity = (int)Math.Pow(2, Rand.Next(1, 5));
                BuildingHeight = (int)Math.Pow(2, Rand.Next(1, 6));
            }

        }

        internal static float[,] GetHeights(int X, int Z)
        {
            var Places = new float[2, 2];

            for (int dX = 0; dX < 2; dX++)
            {
                for (int dZ = 0; dZ < 2; dZ++)
                {
                    Places[dX, dZ] = GetHeight(X + dX, Z + dZ);
                }
            }

            return Places;
        }

        private static float GetHeight(int X, int Z)
        {
            return (float)((Math.Sin(X * SinCos1 + SinCos2) - 0.5f) * (Math.Cos(Math.Pow(Z, 3) * SinCos2 + SinCos1) + 0.5f) * HeightIntensity) - HeightIntensity;
        }
    }
}
