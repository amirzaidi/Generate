using SharpDX;

namespace Generate.Input
{
    class Camera
    {
        internal static int RotationX = 0;
        internal static short RotationY = 0;
        internal static Vector3 Position = Vector3.Zero;

        internal static Matrix Rotation
        {
            get
            {
                return Matrix.RotationYawPitchRoll(RotationX / 1000f, RotationY / 1000f, 0);
            }
        }
        
        internal static Matrix View
        {
            get
            {
                var Rotation = Camera.Rotation;
                var Up = Vector3.TransformCoordinate(Vector3.UnitY, Rotation);
                var LookAt = Vector3.TransformCoordinate(Vector3.UnitZ, Rotation);

                var View = Matrix.LookAtLH(Position, Position + LookAt, Up);
                View.Transpose();

                return View;
            }
        }

        internal static void MoveForward(float Amount)
        {
            Position += Vector3.TransformCoordinate(Vector3.UnitZ, Rotation) * Amount;
        }

        internal static void MoveRight(float Amount)
        {
            Position += Vector3.TransformCoordinate(Vector3.UnitX, Rotation) * Amount;
        }
    }
}
