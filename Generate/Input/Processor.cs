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
                Task.Factory.StartNew(MovementUpdate),
                Task.Factory.StartNew(ConstantsUpdate)
            };
        }

        private static void MovementUpdate()
        {
            float Duration;
            if (KeyboardMouse.HasDuration(Keys.W, out Duration))
                Camera.MoveForward(Duration * 10);

            if (KeyboardMouse.HasDuration(Keys.A, out Duration))
                Camera.MoveRight(-Duration * 8);

            if (KeyboardMouse.HasDuration(Keys.S, out Duration))
                Camera.MoveForward(-Duration * 10);

            if (KeyboardMouse.HasDuration(Keys.D, out Duration))
                Camera.MoveRight(Duration * 8);

            if (KeyboardMouse.HasDuration(Keys.ShiftKey, out Duration))
                Camera.MoveForward(Duration * 30);

            if (Camera.Position.X > 32f)
            {
                Camera.Position.X -= 64f;
                Content.Chunk.UpX();
            }
            else if (Camera.Position.X < -32f)
            {
                Camera.Position.X += 64f;
                Content.Chunk.DownX();
            }
            
            if (Camera.Position.Z > 32f)
            {
                Camera.Position.Z -= 64f;
                Content.Chunk.UpZ();
            }
            else if (Camera.Position.Z < -32f)
            {
                Camera.Position.Z += 64f;
                Content.Chunk.DownZ();
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
