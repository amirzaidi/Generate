using Generate.Content;
using System;
using System.Windows.Forms;

namespace Generate.Input
{
    class Processor
    {
        private static bool FixPosition = true;

        public static void Process()
        {
            var Speed = 40;

            float Duration;
            if (KeyboardMouse.HasDuration(Keys.W, out Duration))
                Camera.MoveForward(Duration * Speed);

            if (KeyboardMouse.HasDuration(Keys.A, out Duration))
                Camera.MoveRight(-Duration * Speed);

            if (KeyboardMouse.HasDuration(Keys.S, out Duration))
                Camera.MoveForward(-Duration * Speed);

            if (KeyboardMouse.HasDuration(Keys.D, out Duration))
                Camera.MoveRight(Duration * Speed);

            if (KeyboardMouse.HasDuration(Keys.ShiftKey, out Duration) && !FixPosition)
                Camera.MoveUp(-Duration * 25);

            if (KeyboardMouse.HasDuration(Keys.Space, out Duration) && !FixPosition)
                Camera.MoveUp(Duration * 25);

            if (Camera.Position.X > Chunk.Size / 2)
            {
                Camera.Position.X -= Chunk.Size;
                Program.Chunks.UpX();
            }
            else if (Camera.Position.X < -Chunk.Size / 2)
            {
                Camera.Position.X += Chunk.Size;
                Program.Chunks.DownX();
            }
            
            if (Camera.Position.Z > Chunk.Size / 2)
            {
                Camera.Position.Z -= Chunk.Size;
                Program.Chunks.UpZ();
            }
            else if (Camera.Position.Z < -Chunk.Size / 2)
            {
                Camera.Position.Z += Chunk.Size;
                Program.Chunks.DownZ();
            }

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

            if (KeyboardMouse.Pressed(Keys.Escape) > 0)
                Program.Close = true;

            if (KeyboardMouse.Pressed(Keys.F9) > 0)
                Program.VSync = 1 - Program.VSync;

            if (KeyboardMouse.Pressed(Keys.F10) > 0)
                FixPosition = !FixPosition;

            if (FixPosition)
            {
                var Height = Program.Chunks.Mid.Height(Camera.Position, 10f);
                if (Math.Abs(Height) < 1000f)
                {
                    Camera.Position.Y += Height - 5f;
                    Camera.Position.Y /= 2;
                    Camera.Position.Y += 5f;
                }
            }
        }
    }
}
