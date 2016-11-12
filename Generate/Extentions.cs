using SharpDX.Mathematics.Interop;
using System;
using System.Text;

namespace Generate
{
    static class Extentions
    {
        internal static byte[] AsciiBytes(this string In)
        {
            return Encoding.ASCII.GetBytes(In);
        }

        internal static float[] ToRGB(this float[] hsb)
        {
            var hue = hsb[0] % 1.0f;
            var saturation = hsb[1];
            var brightness = hsb[2];

            float r = 0, g = 0, b = 0;
            if (saturation == 0)
            {
                r = g = b = brightness;
            }
            else
            {
                float h = (hue - (float)Math.Floor(hue)) * 6.0f;
                float f = h - (float)Math.Floor(h);
                float p = brightness * (1.0f - saturation);
                float q = brightness * (1.0f - saturation * f);
                float t = brightness * (1.0f - (saturation * (1.0f - f)));
                switch ((int)h)
                {
                    case 0:
                        r = brightness;
                        g = t;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = brightness;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = brightness;
                        b = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = brightness;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        b = brightness;
                        break;
                    case 5:
                        r = brightness;
                        g = p;
                        b = q;
                        break;
                }
            }

            return new[] { r, g, b, 1f };
        }

        internal static float NextFloat(this Random Random, float Start = 0f, float End = 1f)
        {
            return (float)Random.NextDouble() * (End - Start) + Start;
        }
    }
}
