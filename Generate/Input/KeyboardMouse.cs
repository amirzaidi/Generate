using SharpDX.Multimedia;
using SharpDX.RawInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Generate.Input
{
    class KeyboardMouse
    {
        class KeyPressDuration
        {
            internal long Ticks = 0;
            internal int Presses = 0;
            internal int Pressed = 0;
        }

        internal struct MouseMovement
        {
            internal long X;
            internal long Y;
        }
        
        private static Stopwatch Timer = new Stopwatch();
        private static Dictionary<Keys, KeyPressDuration> Keys = new Dictionary<Keys, KeyPressDuration>();
        private static MouseMovement Mouse;

        static KeyboardMouse()
        {
            foreach (Keys Key in Enum.GetValues(typeof(Keys)))
            {
                Keys[Key] = new KeyPressDuration();
            }

            Timer.Start();

            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.None);
            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, DeviceFlags.None);

            Device.KeyboardInput += KeyboardEvent;
            Device.MouseInput += MouseEvent;
        }

        public static void MouseEvent(object s, MouseInputEventArgs e)
        {
            lock (Timer)
            {
                Mouse.X += e.X;
                Mouse.Y += e.Y;
            }
        }

        public static void KeyboardEvent(object s, KeyboardInputEventArgs e)
        {
            if (e.State == KeyState.KeyDown)
            {
                Down(e.Key);
            }
            else if (e.State == KeyState.KeyUp)
            {
                Up(e.Key);
            }
            /*else if (e.State == KeyState.SystemKeyDown)
            {
                System.Console.WriteLine(e.Key);
            }*/
        }

        public void MouseDown(object s, MouseEventArgs e)
        {
            var Key = Translate(e.Button);
            if (Key != System.Windows.Forms.Keys.None)
            {
                Down(Key);
            }
        }

        public void MouseUp(object s, MouseEventArgs e)
        {
            var Key = Translate(e.Button);
            if (Key != System.Windows.Forms.Keys.None)
            {
                Up(Key);
            }
        }

        private Keys Translate(MouseButtons Button)
        {
            var Key = System.Windows.Forms.Keys.None;
            if (Button == MouseButtons.Left)
            {
                Key = System.Windows.Forms.Keys.LButton;
            }
            else if (Button == MouseButtons.Right)
            {
                Key = System.Windows.Forms.Keys.RButton;
            }
            else if (Button == MouseButtons.Middle)
            {
                Key = System.Windows.Forms.Keys.MButton;
            }

            return Key;
        }

        private static void Down(Keys Key)
        {
            lock (Keys[Key])
            {
                Keys[Key].Presses++;

                if (Keys[Key].Ticks >= 0)
                {
                    Keys[Key].Ticks -= Timer.ElapsedTicks;
                }
            }
        }

        private static void Up(Keys Key)
        {
            lock (Keys[Key])
            {
                Keys[Key].Pressed++;
                Keys[Key].Ticks += Timer.ElapsedTicks;
            }
        }

        public static int Presses(Keys Key)
        {
            int Presses;
            lock (Keys[Key])
            {
                Presses = Keys[Key].Presses;
                Keys[Key].Presses = 0;
            }

            return Presses;
        }

        public static int Pressed(Keys Key)
        {
            int Pressed;
            lock (Keys[Key])
            {
                Pressed = Keys[Key].Pressed;
                Keys[Key].Pressed = 0;
            }

            return Pressed;
        }

        public static float Duration(Keys Key)
        {
            long Ticks;
            lock (Keys[Key])
            {
                if (Keys[Key].Ticks < 0)
                {
                    Ticks = Keys[Key].Ticks + Timer.ElapsedTicks;
                    Keys[Key].Ticks = -Timer.ElapsedTicks;
                }
                else
                {
                    Ticks = Keys[Key].Ticks;
                    Keys[Key].Ticks = 0;
                }
            }

            return (float)Ticks / Stopwatch.Frequency;
        }

        public static bool HasDuration(Keys Key, out float OutDuration)
        {
            OutDuration = Duration(Key);
            if (OutDuration > 0)
            {
                return true;
            }

            return false;
        }

        public static float[] Duration(params Keys[] Keys)
        {
            var Result = new float[Keys.Length];
            for (int i = 0; i < Keys.Length; i++)
            {
                Result[i] = Duration(Keys[i]);
            }

            return Result;
        }

        public static MouseMovement MouseMoved()
        {
            var Result = Mouse;
            Mouse = new MouseMovement
            {
                X = 0,
                Y = 0
            };

            return Result;
        }
    }
}
