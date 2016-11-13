using SharpDX.Mathematics.Interop;
using System;
using System.Threading.Tasks;

namespace Generate.Procedure
{
    class Texture
    {
        internal static Action<float[,,], Random>[] Handlers;

        static Texture()
        {
            Handlers = new Action<float[,,], Random>[]
            {
                Colorize,
                Dirt,
                Stripe,
                Pixelate
            };
        }

        internal static void Colorize(float[,,] Colors, Random Rand)
        {
            RawColor4 Color = Constants.Color;
            if (Constants.RandomColor)
            {
                var Floats = new[] { 1f, 1f, 1f }.ToRGB();
                Color = new RawColor4(Floats[0], Floats[1], Floats[2], 1);
            }

            Parallel.For(0, Colors.GetLength(0), X =>
            {
                Parallel.For(0, Colors.GetLength(1), Y =>
                {
                    Colors[X, Y, 0] *= Color.R;
                    Colors[X, Y, 1] *= Color.G;
                    Colors[X, Y, 2] *= Color.B;
                });
            });
        }

        internal static void Dirt(float[,,] Colors, Random Rand)
        {
            Parallel.For(0, Colors.GetLength(0), X =>
            {
                Parallel.For(0, Colors.GetLength(1), Y =>
                {
                    Colors[X, Y, 0] *= Rand.NextFloat(Constants.DirtSmoothness);
                    Colors[X, Y, 1] *= Rand.NextFloat(Constants.DirtSmoothness);
                    Colors[X, Y, 2] *= Rand.NextFloat(Constants.DirtSmoothness);
                });
            });
        }

        private static void Stripe(float[,,] Colors, Random Rand)
        {
            Parallel.For(0, Colors.GetLength(0), X =>
            {
                Parallel.For(0, Colors.GetLength(1), Y =>
                {
                    var Intensity = (float)Math.Sin((X + Y) * Math.PI * 2 / Constants.TextureDensity) * Constants.StripeMultiplyFactor + Constants.StripeStart;

                    Colors[X, Y, 0] *= Intensity;
                    Colors[X, Y, 1] *= Intensity;
                    Colors[X, Y, 2] *= Intensity;
                });
            });
        }

        private static void Pixelate(float[,,] Colors, Random Rand)
        {
            int Side = (int)Math.Pow(2, Constants.TextureDensityPower - Rand.Next(0, 3));
            int SideScale = Colors.GetLength(0) / Side;
            int AreaScale = SideScale * SideScale;
            
            Parallel.For(0, Side, X =>
            {
                Parallel.For(0, Side, Y =>
                {
                    var Color = new[] { 0f, 0f, 0f };

                    for (int dX = 0; dX < SideScale; dX++)
                    {
                        for (int dY = 0; dY < SideScale; dY++)
                        {
                            Color[0] += Colors[X * SideScale + dX, Y * SideScale + dY, 0];
                            Color[1] += Colors[X * SideScale + dX, Y * SideScale + dY, 1];
                            Color[2] += Colors[X * SideScale + dX, Y * SideScale + dY, 2];
                        }
                    }

                    for (int dX = 0; dX < SideScale; dX++)
                    {
                        for (int dY = 0; dY < SideScale; dY++)
                        {
                            Colors[X * SideScale + dX, Y * SideScale + dY, 0] = Color[0] / AreaScale;
                            Colors[X * SideScale + dX, Y * SideScale + dY, 1] = Color[1] / AreaScale;
                            Colors[X * SideScale + dX, Y * SideScale + dY, 2] = Color[2] / AreaScale;
                        }
                    }
                });
            });
        }
    }
}
