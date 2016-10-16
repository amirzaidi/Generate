using SharpDX;
using System;

namespace Generate.Input
{
    class Camera
    {
        internal static float RotationX = 0; //0 to pi
        internal static float RotationY = 0; //0 to 2pi
        internal static Vector3 Position = new Vector3(0, 0, 0);

        internal static void Reset()
        {
            RotationX = 0;
            RotationY = 0;
        }

        internal static Matrix View()
        {
            // Create the rotation matrix from the yaw, pitch, and roll values.
            var rotationMatrix = Matrix.RotationYawPitchRoll(RotationX, RotationY, 0);
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
            // Create the rotation matrix from the yaw, pitch, and roll values.
            var rotationMatrix = Matrix.RotationYawPitchRoll(RotationX, RotationY, 0);

            // Transform the lookAt and up vector by the rotation matrix so the view is correctly rotated at the origin.
            var moveTo = Vector3.TransformCoordinate(new Vector3(0, 0, 1), rotationMatrix);

            Position.X += moveTo.X * Amount;
            Position.Y += moveTo.Y * Amount;
            Position.Z += moveTo.Z * Amount;
        }

        internal static void MoveRight(float Amount)
        {
            // Create the rotation matrix from the yaw, pitch, and roll values.
            var rotationMatrix = Matrix.RotationYawPitchRoll(RotationX + (float)Math.PI / 2, 0, 0);

            // Transform the lookAt and up vector by the rotation matrix so the view is correctly rotated at the origin.
            var moveTo = Vector3.TransformCoordinate(new Vector3(0, 0, 1), rotationMatrix);

            Position.X += moveTo.X * Amount;
            Position.Y += moveTo.Y * Amount;
            Position.Z += moveTo.Z * Amount;
        }
    }
}
