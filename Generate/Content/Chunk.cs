using System;
using System.Collections.Concurrent;

namespace Generate.Content
{
    class Chunk : IDisposable
    {
        private static Chunk[,] Chunks = new Chunk[5, 5];
        private static int MovedX = 0, MovedZ = 0;

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

        internal static void Render()
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
            for (int Z = 0; Z < 5; Z++)
            {
                Chunks[0, Z].Dispose();
                Chunks[0, Z] = Chunks[1, Z];
                Chunks[1, Z] = Chunks[2, Z];
                Chunks[2, Z] = Chunks[3, Z];
                Chunks[3, Z] = Chunks[4, Z];
                Chunks[4, Z] = new Chunk(4 + MovedX, Z + MovedZ);
            }
        }

        internal static void DownX()
        {
            MovedX--;
            for (int Z = 0; Z < 5; Z++)
            {
                Chunks[4, Z].Dispose();
                Chunks[4, Z] = Chunks[3, Z];
                Chunks[3, Z] = Chunks[2, Z];
                Chunks[2, Z] = Chunks[1, Z];
                Chunks[1, Z] = Chunks[0, Z];
                Chunks[0, Z] = new Chunk(MovedX, Z + MovedZ);
            }
        }

        internal static void UpZ()
        {
            MovedZ++;
            for (int X = 0; X < 5; X++)
            {
                Chunks[X, 0].Dispose();
                for (int Z = 0; Z < 4; Z++)
                {
                    Chunks[X, Z] = Chunks[X, Z + 1];
                }

                Chunks[X, 4] = new Chunk(X + MovedX, 4 + MovedZ);
            }
        }

        internal static void DownZ()
        {
            MovedZ--;
            for (int X = 0; X < 5; X++)
            {
                Chunks[X, 4].Dispose();
                for (int Z = 4; Z > 0; Z--)
                {
                    Chunks[X, Z] = Chunks[X, Z - 1];
                }

                Chunks[X, 0] = new Chunk(X + MovedX, MovedZ);
            }
        }

        public static void DisposeAll()
        {
            foreach (var Chunk in Chunks)
            {
                Chunk.Dispose();
            }
        }

        private ConcurrentBag<IDisposable> Models = new ConcurrentBag<IDisposable>();

        internal Chunk(int X, int Z)
        {
            Console.WriteLine($"Created {X}, {Z}");
        }

        public void Render(int DeltaX, int DeltaY)
        {
            foreach (var Model in Models)
            {
                
            }
        }

        public void Dispose()
        {
            foreach (var Model in Models)
            {
                Model.Dispose();
            }
        }
    }
}