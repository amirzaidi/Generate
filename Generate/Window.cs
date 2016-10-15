using SharpDX.Windows;
using System;
using System.Windows.Forms;

namespace Generate
{
    class Window : IDisposable
    {
        private Form Form;

        public Window()
        {
            Form = new RenderForm();
            Form.MouseEnter += (s, e) => Cursor.Hide();
            Form.MouseLeave += (s, e) => Cursor.Show();
        }

        public void Show()
        {
            Form.Show();
        }

        public void Dispose()
        {
            Form.Dispose();
        }
    }
}
