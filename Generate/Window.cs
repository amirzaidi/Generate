using SharpDX.Windows;
using System;
using System.Drawing;
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
            Form = new RenderForm("Generate");
            Form.MouseEnter += (s, e) => Cursor.Hide();
            Form.MouseLeave += (s, e) => Cursor.Show();
            Form.MouseDown += Input.KeyboardMouse.MouseDown;
            Form.MouseUp += Input.KeyboardMouse.MouseUp;
            Form.Deactivate += (s, e) => Program.VSync = 1;

            Form.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
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
            Form.WindowState = FormWindowState.Maximized;
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
