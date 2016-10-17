using SharpDX;

namespace Generate.Input
{
    class Camera
    {
        internal static long RotationX = 0;
        internal static long RotationY = 0;
        internal static Matrix Rotation
        {
            get
            {
                return Matrix.RotationYawPitchRoll(RotationX / 1000f, RotationY / 1000f, 0);
            }
        }

        internal static Vector3 Position = new Vector3(0, 0, 0);

        internal static Matrix View()
        {
            // Create the rotation matrix from the yaw, pitch, and roll values.
            var rotationMatrix = Rotation;
            var up = Vector3.TransformCoordinate(Vector3.UnitY, rotationMatrix);

            // Transform the lookAt and up vector by the rotation matrix so the view is correctly rotated at the origin.
            var lookAt = Vector3.TransformCoordinate(new Vector3(0, 0, 1), rotationMatrix);

            // Translate the rotated camera position to the location of the viewer.
            // Finally create the view matrix from the three updated vectors.
            var View = Matrix.LookAtLH(Position, Position + lookAt, up);

            // Transpose the matrix to prepare it for shader.
            View.Transpose();

            return View;
        }

        internal static void MoveForward(float Amount)
        {
            Position += Vector3.TransformCoordinate(new Vector3(0, 0, 1), Rotation) * Amount;
        }

        internal static void MoveRight(float Amount)
        {
            Position += Vector3.TransformCoordinate(new Vector3(1, 0, 0), Rotation) * Amount;
        }
    }
}
