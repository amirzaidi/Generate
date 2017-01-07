using Generate.Procedure;
using SharpDX;
using System;

namespace Generate.Content
{
    class Skybox : Model
    {
        internal static Skybox Main;

        internal Skybox(int Seed) : base(Vector3.Zero, null, null, Seed, false)
        {
            int Tesselation = 16;

            int VerticalSegments = Tesselation;
            int HorizontalSegments = Tesselation * 2;
            
            BallVertices(VerticalSegments, HorizontalSegments, 15 * 1024f);

            // Fill the index buffer with triangles joining each pair of latitude rings.

            Indices = new int[(VerticalSegments) * (HorizontalSegments + 1) * 6];
            int Stride = HorizontalSegments + 1;

            int Index = 0;
            for (int i = 0; i < VerticalSegments; i++)
            {
                for (int j = 0; j <= HorizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % Stride;

                    //Inside Out
                    Indices[Index++] = (i * Stride + j);
                    Indices[Index++] = (nextI * Stride + j);
                    Indices[Index++] = (i * Stride + nextJ);

                    Indices[Index++] = (i * Stride + nextJ);
                    Indices[Index++] = (nextI * Stride + j);
                    Indices[Index++] = (nextI * Stride + nextJ);
                }
            }

            Load();
        }

        protected override void LoadTexture()
        {
            var Colors = new float[3200, 3200, 3];

            for (int Y = 0; Y < Colors.GetLength(1); Y++)
            {
                var Intensity = (float)Math.Pow((float)Y / (Colors.GetLength(1) - 1), 2);

                for (int X = 0; X < Colors.GetLength(0); X++)
                {
                    Colors[X, Y, 0] = Constants.Background.R * Intensity;
                    Colors[X, Y, 1] = Constants.Background.G * Intensity;
                    Colors[X, Y, 2] = Constants.Background.B * Intensity;
                }
            }

            var Rand = new Random(Seed);
            
            if (Constants.StarDensity > 0)
            {
                int Count = (int)Math.Pow(2, Constants.StarDensity);

                for (int i = 0; i < Count; i++)
                {
                    int X = Rand.Next(10, 3190);
                    int Y = Rand.Next(1000, 3190);
                    int MaxdX = Rand.Next(0, 7);
                    if (MaxdX > 3)
                    {
                        MaxdX = 0;
                    }

                    int MaxdY = MaxdX * 2;

                    if (Y > 2900)
                    {
                        var Scale = Math.Pow((double)(Y - 2700) / 200, 1.25);
                        MaxdX *= (int)Math.Ceiling(Scale);

                        if (Y > 3150)
                        {
                            MaxdY = (int)Math.Ceiling(MaxdY / Math.Pow(Scale, 8));
                        }
                        else
                        {
                            MaxdY = (int)Math.Ceiling(MaxdY / Math.Pow(Scale, 3));
                        }
                    }

                    for (int dX = -MaxdX; dX <= MaxdX; dX++)
                    {
                        Colors[X + dX, Y, 0] = 1f;
                        Colors[X + dX, Y, 1] = 1f;
                        Colors[X + dX, Y, 2] = 1f;
                    }

                    for (int dY = -MaxdY; dY <= MaxdY; dY++)
                    {
                        Colors[X, Y + dY, 0] = 1f;
                        Colors[X, Y + dY, 1] = 1f;
                        Colors[X, Y + dY, 2] = 1f;
                    }
                }
            }
            
            TextureFromFloatArray(Colors);
        }
    }
}
