using Generate.Procedure;
using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Generate.Content
{
    class Chunk : IDisposable
    {
        private static Matrix RotateLeft;
        private static Matrix RotateRight;

        static Chunk()
        {
            Matrix.RotationZ((float)(Math.PI / 4), out RotateLeft);
            Matrix.RotationZ(-(float)(Math.PI / 4), out RotateRight);
        }

        internal const float Size = 64f;

        private ConcurrentBag<Model> Models = new ConcurrentBag<Model>();
        private Worker Random;
        private float[,] Heights;
        private Task Init;
        private float BuildingHeight = 0f;
        private Vector2 BuildingScale;

        internal Chunk(long X, long Z)
        {
            Init = Task.Run(() =>
            {
                Random = new Worker($"{X}.{Z}");
                Heights = Constants.GetHeights(X, Z);

                Models.Add(DefaultModels.Ground(Random.Next(), Heights));
                
                if (Constants.BuildingDensity != 0 && (((X + 1) % Constants.BuildingDensity) | ((Z + 1) % Constants.BuildingDensity)) == 0 && Random.NextFloat() < Constants.BuildingChance)
                {
                    Program.LogLine("Has building", $"{X},{Z}");

                    var Building = DefaultModels.Building(Random);
                    BuildingHeight = Building.MoveWorld.Y + Building.ScaleVector.Y;
                    BuildingScale = new Vector2(Building.ScaleVector.X, Building.ScaleVector.Z);

                    Models.Add(Building);
                }

                Program.LogLine($"Heights {Heights[0, 0]} {Heights[0, 1]} {Heights[1, 0]} {Heights[1, 1]}", $"{X},{Z}");
            });
        }

        internal void Render(int dX, int dZ)
        {
            foreach (var Model in Models)
            {
                Model.Render(new Vector2(dX * Size, dZ * Size));
            }
        }

        private static float Sqrt2 = (float)Math.Sqrt(2);
        private static float HalfSqrt2 = Sqrt2 / 2;

        internal float Height(Vector3 Position)
        {
            if (Heights == null)
            {
                return 1000f;
            }

            if (BuildingHeight != 0f && Position.X > -BuildingScale.X && Position.X < BuildingScale.X && Position.Z > -BuildingScale.Y && Position.Z < BuildingScale.Y)
            {
                return BuildingHeight;
            }

            var DistanceLeft = Position.X / Size + 0.5f;
            var DistanceBottom = Position.Z / Size + 0.5f;

            var TransformedPlace = Vector3.TransformCoordinate(new Vector3(DistanceLeft, DistanceBottom, 0), RotateLeft);

            var AnchorHeight = Heights[1, 0];
            if (TransformedPlace.X < 0)
            {
                TransformedPlace.X *= -1;
                AnchorHeight = Heights[0, 1];
            }

            var DistanceAnchor = 1 - Sqrt2 * TransformedPlace.X;
            var WeightedAnchorHeight = (1 - DistanceAnchor) * AnchorHeight;

            var HeightBottomLeft = DistanceAnchor * Heights[0, 0] + WeightedAnchorHeight;
            var HeightTopRight = DistanceAnchor * Heights[1, 1] + WeightedAnchorHeight;

            var ScaleMovedUp = (TransformedPlace.Y - TransformedPlace.X) / (HalfSqrt2 - TransformedPlace.X) / 2;

            return ScaleMovedUp * HeightTopRight + (1 - ScaleMovedUp) * HeightBottomLeft;
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