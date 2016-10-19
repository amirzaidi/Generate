using Generate.Procedure;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Generate.Content
{
    class Chunk : IDisposable
    {
        private static Chunk[,] Chunks = new Chunk[7, 7];
        private static int MovedX = 0, MovedZ = 0;
        private const float Size = 64f;

        static Chunk()
        {
            for (int X = 0; X < 7; X++)
            {
                for (int Z = 0; Z < 7; Z++)
                {
                    Chunks[X, Z] = new Chunk(X, Z);
                }
            }
        }

        internal static void RenderVisible()
        {
            for (int X = 0; X < 7; X++)
            {
                for (int Z = 0; Z < 7; Z++)
                {
                    Chunks[X, Z].Render(X - 3, Z - 3);
                }
            }
        }

        internal static void UpX()
        {
            MovedX++;
            Parallel.For(0, 7, Z =>
            {
                Chunks[0, Z].Dispose();
                for (int X = 0; X < 6; X++)
                {
                    Chunks[X, Z] = Chunks[X + 1, Z];
                }

                Chunks[6, Z] = new Chunk(6 + MovedX, Z + MovedZ);
            });
        }

        internal static void DownX()
        {
            MovedX--;
            Parallel.For(0, 7, Z =>
            {
                Chunks[6, Z].Dispose();
                for (int X = 6; X > 0; X--)
                {
                    Chunks[X, Z] = Chunks[X - 1, Z];
                }

                Chunks[0, Z] = new Chunk(MovedX, Z + MovedZ);
            });
        }

        internal static void UpZ()
        {
            MovedZ++;
            Parallel.For(0, 7, X =>
            {
                Chunks[X, 0].Dispose();
                for (int Z = 0; Z < 6; Z++)
                {
                    Chunks[X, Z] = Chunks[X, Z + 1];
                }

                Chunks[X, 6] = new Chunk(X + MovedX, 6 + MovedZ);
            });
        }

        internal static void DownZ()
        {
            MovedZ--;
            Parallel.For(0, 7, X =>
            {
                Chunks[X, 6].Dispose();
                for (int Z = 6; Z > 0; Z--)
                {
                    Chunks[X, Z] = Chunks[X, Z - 1];
                }

                Chunks[X, 0] = new Chunk(X + MovedX, MovedZ);
            });
        }

        internal static void DisposeAll()
        {
            foreach (var Chunk in Chunks)
            {
                Chunk.Dispose();
            }
        }

        private ConcurrentBag<Model> Models = new ConcurrentBag<Model>();
        private bool Loaded = false;
        private Worker Random;
        private float[,] Heights;

        internal Chunk(int X, int Z)
        {
            Task.Run(async delegate
            {
                Random = new Worker($"{X}.{Z}.chunk");
                Heights = Constants.GetHeights(X, Z);

                await Task.Delay(50);
                Models.Add(DefaultModels.Ground(Random.Next(), Heights));
                await Task.Delay(50);
                Models.Add(DefaultModels.Sphere(Random.Next()));
                await Task.Delay(50);
                Models.Add(DefaultModels.Triangle(Random.Next()));

                Loaded = true;
                Console.WriteLine($"Created {X}, {Z} at {Heights[0,0]} {Heights[0, 1]} {Heights[1, 0]} {Heights[1, 1]}");
            });
        }

        public void Render(int dX, int dY)
        {
            if (Loaded)
            {
                foreach (var Model in Models)
                {
                    Model.Render(new SharpDX.Vector2(dX * Size, dY * Size));
                }
            }
        }

        public void Dispose()
        {
            Task.Run(delegate
            {
                Parallel.ForEach(Models, Model =>
                {
                    Model.Dispose();
                });
            });
        }
    }
}