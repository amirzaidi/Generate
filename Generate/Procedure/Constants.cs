using SharpDX;
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

        private static System.Diagnostics.Stopwatch RotateWatch = new System.Diagnostics.Stopwatch();
        private static Vector3 BaseLightDirection;
        internal static Vector3 LightDirection
        {
            get
            {
                if (!RotateWatch.IsRunning)
                {
                    RotateWatch.Start();
                }

                Matrix Rotation;
                Matrix.RotationY(RotateWatch.ElapsedMilliseconds / 1000f, out Rotation);
                return Vector3.TransformCoordinate(BaseLightDirection, Rotation);
            }
        }

        internal static void Load()
        {
            var Rand = new Worker("constants");

            Hue = (float)Rand.NextDouble();
            Saturation = (float)Rand.NextDouble();
            Brightness = (float)(Rand.NextDouble() / 2f + 0.5f);

            var BGFloat = new[] { Hue, Saturation, Brightness }.ToRGB();
            BG = new RawColor4(BGFloat[0], BGFloat[1], BGFloat[2], 1);

            AvgTexDensity = Rand.Next(1, 6);
            HeightIntensity = (float)Math.Pow(Rand.NextDouble(), 5) * 128f;

            BaseLightDirection = new Vector3(
                (float)Rand.NextDouble() * 2f - 1f, 
                -(float)Rand.NextDouble() * 0.5f - 0.5f,
                (float)Rand.NextDouble() * 2f - 1f
            );

            BaseLightDirection.Normalize();
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
            return (float)(Math.Sin(X) * Math.Cos(Z) * HeightIntensity) - HeightIntensity;
        }
    }
}
