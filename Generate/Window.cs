using SharpDX.Windows;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Generate
{
    class Window : IDisposable
    {
        private Form Form;
        internal IntPtr Handle
        {
            get
            {
                return Form.Handle;
            }
        }

        internal Window()
        {
            Form = new RenderForm();
            Form.MouseEnter += (s, e) => Cursor.Hide();
            Form.MouseLeave += (s, e) => Cursor.Show();
        }

        internal void Show()
        {
            Form.Show();
        }

        internal void Borderless(int Width, int Height)
        {
            Form.FormBorderStyle = FormBorderStyle.None;
            Form.Location = new Point(0, 0);
            Form.Size = new Size(Width, Height);
        }

        public void Dispose()
        {
            Form.Dispose();
        }
    }
}
