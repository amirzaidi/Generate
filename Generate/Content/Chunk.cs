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
                // ToDo: Fix separate triangle positioning

                var DistanceLeft = Position.X / Size + 0.5f;
                var DistanceBottom = Position.Z / Size + 0.5f;

                Console.WriteLine(DistanceBottom);

                float MidHeight;

                //+X,0 = right
                //0,+Z = down

                Console.WriteLine(DistanceLeft + " " + DistanceBottom);

                if (DistanceLeft < DistanceBottom)
                {
                    //Top Left

                    float TopHeight = (1 - DistanceLeft) * Heights[0, 0] + DistanceLeft * Heights[1, 0];
                    float BottomHeight = (1 - DistanceLeft) * Heights[0, 1] + DistanceLeft * Heights[1, 1];

                    MidHeight = (1 - DistanceBottom) * TopHeight + DistanceBottom * BottomHeight;
                }
                else
                {
                    //Bottom Right

                    float TopHeight = (1 - DistanceLeft) * Heights[0, 0] + DistanceLeft * Heights[1, 0];
                    float BottomHeight = (1 - DistanceLeft) * Heights[0, 1] + DistanceLeft * Heights[1, 1];

                    MidHeight = (1 - DistanceBottom) * TopHeight + DistanceBottom * BottomHeight;
                }

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