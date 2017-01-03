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
                Pixelate,
                Fade
            };
        }

        internal static void Colorize(float[,,] Colors, Random Rand)
        {
            RawColor4 Color = Constants.Color;
            if (Constants.RandomColor)
            {
                var Floats = new[] { Rand.NextFloat(), 1f, Rand.NextFloat(0.25f, 0.65f) }.ToRGB();
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
            for (int X = 0; X < Colors.GetLength(0); X++)
            {
                for (int Y = 0; Y < Colors.GetLength(0); Y++)
                {
                    for (int C = 0; C < 3; C++)
                    {
                        Colors[X, Y, C] *= Rand.NextFloat(Constants.DirtSmoothness);
                    }
                }
            }
        }

        private static void Stripe(float[,,] Colors, Random Rand)
        {
            Parallel.For(0, Colors.GetLength(0), X =>
            {
                Parallel.For(0, Colors.GetLength(1), Y =>
                {
                    var Intensity = (float)Math.Sin((X + Y) * Math.PI * 2 / Constants.TextureDensity) * Constants.StripeMultiplyFactor + Constants.StripeStart;

                    for (int C = 0; C < 3; C++)
                    {
                        Colors[X, Y, C] *= Intensity;
                    }
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
                            for (int C = 0; C < 3; C++)
                            {
                                Color[C] += Colors[X * SideScale + dX, Y * SideScale + dY, C];
                            }
                        }
                    }

                    for (int dX = 0; dX < SideScale; dX++)
                    {
                        for (int dY = 0; dY < SideScale; dY++)
                        {
                            for (int C = 0; C < 3; C++)
                            {
                                Colors[X * SideScale + dX, Y * SideScale + dY, C] = Color[C] / AreaScale;
                            }
                        }
                    }
                });
            });
        }

        private static void Fade(float[,,] Colors, Random Rand)
        {
            Parallel.For(0, Colors.GetLength(1), Y =>
            {
                var Intensity = (1f - (float)Math.Sin(Y / Colors.GetLength(1) * Math.PI)) * Constants.FadeIntensity;
                var OriginalIntensity = 1f - Intensity;

                var AddColor = new[] { Intensity * Constants.FadeColor[0], Intensity * Constants.FadeColor[1], Intensity * Constants.FadeColor[2] };

                Parallel.For(0, Colors.GetLength(0), X =>
                {
                    for (int C = 0; C < 3; C++)
                    {
                        Colors[X, Y, C] = Colors[X, Y, C] * OriginalIntensity + AddColor[C];
                    }
                });
            });
        }
    }
}
