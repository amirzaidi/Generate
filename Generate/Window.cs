using SharpDX.Windows;
using System;
using System.Collections;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Generate
{
    class LoopWindow : IDisposable
    {
        private Form Form;
        internal IntPtr Handle
        {
            get
            {
                return Form.Handle;
            }
        }

        internal LoopWindow()
        {
            Form = new RenderForm();
            Form.MouseEnter += (s, e) => Cursor.Hide();
            Form.MouseLeave += (s, e) => Cursor.Show();
            Form.MouseDown += Input.KeyboardMouse.MouseDown;
            Form.MouseUp += Input.KeyboardMouse.MouseUp;
        }

        internal RenderLoop Loop()
        {
            var RenderLoop = new RenderLoop(Form);
            Form.Show();
            Form.Activate();
            return RenderLoop;
        }

        internal void Borderless(int Width, int Height)
        {
            Form.FormBorderStyle = FormBorderStyle.None;
            Form.Location = new Point(0, 0);
            Form.Size = new Size(Width, Height);
        }

        public void Dispose()
        {
            Form.Close();
            Form.Dispose();
        }
    }
}
