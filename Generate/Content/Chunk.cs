using Generate.Procedure;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Generate.Content
{
    class Chunk : IDisposable
    {
        private static Chunk[,] Chunks = new Chunk[5, 5];
        private static int MovedX = 0, MovedZ = 0;
        private const float Size = 64f;

        static Chunk()
        {
            for (int X = 0; X < 5; X++)
            {
                for (int Z = 0; Z < 5; Z++)
                {
                    Chunks[X, Z] = new Chunk(X, Z);
                }
            }
        }

        internal static void RenderVisible()
        {
            for (int X = 0; X < 5; X++)
            {
                for (int Z = 0; Z < 5; Z++)
                {
                    Chunks[X, Z].Render(X - 2, Z - 2);
                }
            }
        }

        internal static void UpX()
        {
            MovedX++;
            Parallel.For(0, 5, Z =>
            {
                Chunks[0, Z].Dispose();
                for (int X = 0; X < 4; X++)
                {
                    Chunks[X, Z] = Chunks[X + 1, Z];
                }

                Chunks[4, Z] = new Chunk(4 + MovedX, Z + MovedZ);
            });
        }

        internal static void DownX()
        {
            MovedX--;
            Parallel.For(0, 5, Z =>
            {
                Chunks[4, Z].Dispose();
                for (int X = 4; X > 0; X--)
                {
                    Chunks[X, Z] = Chunks[X - 1, Z];
                }

                Chunks[0, Z] = new Chunk(MovedX, Z + MovedZ);
            });
        }

        internal static void UpZ()
        {
            MovedZ++;
            Parallel.For(0, 5, X =>
            {
                Chunks[X, 0].Dispose();
                for (int Z = 0; Z < 4; Z++)
                {
                    Chunks[X, Z] = Chunks[X, Z + 1];
                }

                Chunks[X, 4] = new Chunk(X + MovedX, 4 + MovedZ);
            });
        }

        internal static void DownZ()
        {
            MovedZ--;
            Parallel.For(0, 5, X =>
            {
                Chunks[X, 4].Dispose();
                for (int Z = 4; Z > 0; Z--)
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

        internal Chunk(int X, int Z)
        {
            Task.Run(async delegate
            {
                Random = new Worker($"{X}.{Z}.chunk");

                await Task.Delay(50);
                Models.Add(DefaultModels.Ground(Random.Next()));

                await Task.Delay(50);
                Models.Add(DefaultModels.Sphere(Random.Next()));

                await Task.Delay(50);
                Models.Add(DefaultModels.Triangle(Random.Next()));

                Loaded = true;
                Console.WriteLine($"Created {X}, {Z}");
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