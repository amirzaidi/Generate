using Generate.Procedure;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Generate.Content
{
    class Chunk : IDisposable
    {
        internal static int MovedX = 0, MovedZ = 0;
        private const float Size = 64f;

        private const int ChunkCountSide = 3;
        private const int ChunkCountMaxKey = 2 * ChunkCountSide;
        private const int ChunkCount = ChunkCountMaxKey + 1;
        private static Chunk[,] Chunks = new Chunk[ChunkCount, ChunkCount];

        static Chunk()
        {
            for (int X = 0; X < ChunkCount; X++)
            {
                for (int Z = 0; Z < ChunkCount; Z++)
                {
                    Chunks[X, Z] = new Chunk(X, Z);
                }
            }
        }

        internal static void RenderVisible()
        {
            for (int X = 0; X < ChunkCount; X++)
            {
                for (int Z = 0; Z < ChunkCount; Z++)
                {
                    Chunks[X, Z].Render(X - ChunkCountSide, Z - ChunkCountSide);
                }
            }
        }

        internal static void UpX()
        {
            MovedX++;
            Parallel.For(0, ChunkCount, Z =>
            {
                Chunks[0, Z].Dispose();
                for (int X = 0; X < ChunkCountMaxKey; X++)
                {
                    Chunks[X, Z] = Chunks[X + 1, Z];
                }

                Chunks[ChunkCount - 1, Z] = new Chunk(ChunkCountMaxKey + MovedX, Z + MovedZ);
            });
        }

        internal static void DownX()
        {
            MovedX--;
            Parallel.For(0, ChunkCount, Z =>
            {
                Chunks[ChunkCountMaxKey, Z].Dispose();
                for (int X = ChunkCount - 1; X > 0; X--)
                {
                    Chunks[X, Z] = Chunks[X - 1, Z];
                }

                Chunks[0, Z] = new Chunk(MovedX, Z + MovedZ);
            });
        }

        internal static void UpZ()
        {
            MovedZ++;
            Parallel.For(0, ChunkCount, X =>
            {
                Chunks[X, 0].Dispose();
                for (int Z = 0; Z < ChunkCountMaxKey; Z++)
                {
                    Chunks[X, Z] = Chunks[X, Z + 1];
                }

                Chunks[X, ChunkCount - 1] = new Chunk(X + MovedX, ChunkCountMaxKey + MovedZ);
            });
        }

        internal static void DownZ()
        {
            MovedZ--;
            Parallel.For(0, ChunkCount, X =>
            {
                Chunks[X, ChunkCountMaxKey].Dispose();
                for (int Z = ChunkCountMaxKey; Z > 0; Z--)
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
        private Worker Random;
        private float[,] Heights;
        private Task Init;

        internal Chunk(int X, int Z)
        {
            Init = Task.Run(() =>
            {
                Random = new Worker($"{X}.{Z}.chunk");
                Heights = Constants.GetHeights(X, Z);

                Models.Add(DefaultModels.Ground(Random.Next(), Heights));
                Models.Add(DefaultModels.Sphere(Random.Next()));
                Models.Add(DefaultModels.Triangle(Random.Next()));

                Console.WriteLine($"Created {X}, {Z} at {Heights[0, 0]} {Heights[0, 1]} {Heights[1, 0]} {Heights[1, 1]}");
            });
        }

        public void Render(int dX, int dZ)
        {
            foreach (var Model in Models)
            {
                Model.Render(new SharpDX.Vector2(dX * Size, dZ * Size));
            }
        }

        public void Dispose()
        {
            Init.ContinueWith(Result =>
            {
                Model Model;
                while (Models.TryTake(out Model))
                {
                    Model.Dispose();
                }
            });
        }
    }
}