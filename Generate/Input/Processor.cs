using Generate.Content;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Generate.Input
{
    class Processor
    {
        public static Task[] Process()
        {
            MovementUpdate();
            ChangeChunkSize();

            return new[]
            {
                Task.Factory.StartNew(ConstantsUpdate)
            };
        }

        private static void MovementUpdate()
        {
            float Duration;
            if (KeyboardMouse.HasDuration(Keys.W, out Duration))
                Camera.MoveForward(Duration * 40);

            if (KeyboardMouse.HasDuration(Keys.A, out Duration))
                Camera.MoveRight(-Duration * 20);

            if (KeyboardMouse.HasDuration(Keys.S, out Duration))
                Camera.MoveForward(-Duration * 15);

            if (KeyboardMouse.HasDuration(Keys.D, out Duration))
                Camera.MoveRight(Duration * 20);

            if (KeyboardMouse.HasDuration(Keys.ShiftKey, out Duration))
                Camera.MoveUp(-Duration * 25);

            if (KeyboardMouse.HasDuration(Keys.Space, out Duration))
                Camera.MoveUp(Duration * 25);


            if (Camera.Position.X > 32f)
            {
                Camera.Position.X -= 64f;
                Program.Chunks.UpX();
            }
            else if (Camera.Position.X < -32f)
            {
                Camera.Position.X += 64f;
                Program.Chunks.DownX();
            }
            
            if (Camera.Position.Z > 32f)
            {
                Camera.Position.Z -= 64f;
                Program.Chunks.UpZ();
            }
            else if (Camera.Position.Z < -32f)
            {
                Camera.Position.Z += 64f;
                Program.Chunks.DownZ();
            }
        }

        private static void ChangeChunkSize()
        {
            if (KeyboardMouse.Pressed(Keys.F7) > 0 && ChunkLoader.ChunkCountSide > 1)
            {
                Program.Chunks.Dispose();
                ChunkLoader.ChunkCountSide--;
                Program.Chunks = new ChunkLoader();
            }

            if (KeyboardMouse.Pressed(Keys.F8) > 0)
            {
                Program.Chunks.Dispose();
                ChunkLoader.ChunkCountSide++;
                Program.Chunks = new ChunkLoader();
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
