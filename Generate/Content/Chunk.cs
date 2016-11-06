using Generate.Procedure;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Generate.Content
{
    class Chunk : IDisposable
    {
        private const float Size = 64f;
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