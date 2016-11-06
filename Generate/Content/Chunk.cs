using Generate.Procedure;
using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Generate.Content
{
    class Chunk : IDisposable
    {
        internal const float Size = 64f;

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

        internal void Render(int dX, int dZ)
        {
            foreach (var Model in Models)
            {
                Model.Render(new Vector2(dX * Size, dZ * Size));
            }
        }

        internal void FixPosition(ref Vector3 Position, float Height)
        {
            if (Heights != null)
            {
                var ViewpointX = Position.X / Size + 0.5f;
                var ViewpointZ = Position.Z / Size + 0.5f;

                float TopHeight = (1 - ViewpointX) * Heights[0, 0] + ViewpointX * Heights[1, 0];
                float BottomHeight = (1 - ViewpointX) * Heights[0, 1] + ViewpointX * Heights[1, 1];

                float MidHeight = (1 - ViewpointZ) * TopHeight + ViewpointZ * BottomHeight;

                Position.Y = MidHeight + Height;
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