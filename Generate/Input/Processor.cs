using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Generate.Input
{
    class Processor
    {
        public static Task[] Process()
        {
            return new[]
            {
                Task.Factory.StartNew(RotationUpdate),
                Task.Factory.StartNew(MovementUpdate),
                Task.Factory.StartNew(ConstantsUpdate)
            };
        }

        private static void MovementUpdate()
        {
            float Duration;
            if (KeyboardMouse.HasDuration(Keys.W, out Duration))
                Camera.MoveForward(Duration * 3);

            if (KeyboardMouse.HasDuration(Keys.A, out Duration))
                Camera.MoveRight(-Duration * 3);

            if (KeyboardMouse.HasDuration(Keys.S, out Duration))
                Camera.MoveForward(-Duration * 3);

            if (KeyboardMouse.HasDuration(Keys.D, out Duration))
                Camera.MoveRight(Duration * 3);

            if (KeyboardMouse.HasDuration(Keys.ShiftKey, out Duration))
                Camera.MoveForward(Duration * 30);

            if (Camera.Position.X > 32)
            {
                Camera.Position.X -= 64;
                Content.Chunk.UpX();
            }
            else if (Camera.Position.X < -32)
            {
                Camera.Position.X += 64;
                Content.Chunk.DownX();
            }

            if (Camera.Position.Z > 32)
            {
                Camera.Position.Z -= 64;
                Content.Chunk.UpZ();
            }
            else if (Camera.Position.Z < -32)
            {
                Camera.Position.Z += 64;
                Content.Chunk.DownZ();
            }
        }

        private static void RotationUpdate()
        {
            var Moved = KeyboardMouse.MouseMoved();
            Camera.RotationX += Moved.X * 0.003f;
            Camera.RotationY += Moved.Y * 0.003f;

            if (Camera.RotationY < -Math.PI * 0.5)
            {
                Camera.RotationY = (float)(-Math.PI * 0.5);
            }
            else if (Camera.RotationY > Math.PI * 0.5)
            {
                Camera.RotationY = (float)(Math.PI * 0.5);
            }
        }

        private static void ConstantsUpdate()
        {
            if (KeyboardMouse.Pressed(Keys.Escape) > 0)
                Program.Close = true;

            if (KeyboardMouse.Pressed(Keys.F9) > 0)
                Program.VSync = 1 - Program.VSync;
        }
    }
}
