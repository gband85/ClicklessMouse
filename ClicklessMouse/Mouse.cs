using ClicklessMouse.Native;
using System.Diagnostics;
using System.Threading;
using Window = Avalonia.Controls.Window;

namespace ClicklessMouse
{
    public partial class MainWindow : Window
    {
        private void left_down()
        {
#if _LINUX
            InputX11.LeftButtonDown();
#elif _WINDOWS
            _sim.Mouse.LeftButtonDown();
#endif
        }

        private void left_up()
        {
#if _LINUX
            InputX11.LeftButtonUp();
#elif _WINDOWS
            _sim.Mouse.LeftButtonUp();
#endif
        }

        private void right_down()
        {
#if _LINUX
            InputX11.RightButtonDown();
#elif _WINDOWS
            _sim.Mouse.RightButtonDown();
#endif
        }

        private void right_up()
        {
#if _LINUX
            InputX11.RightButtonUp();
#elif _WINDOWS
            _sim.Mouse.RightButtonUp();
#endif
        }

        private void freeze_mouse(int x, int y, int time)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
#if _LINUX
                InputX11.SetCursorPos(X, Y);
#elif _WINDOWS
                ClicklessMouse.Native.InputWin.SetCursorPos(x, y);
#endif
                Thread.Sleep(1);
            }
            while (stopwatch.ElapsedMilliseconds < time);
        }

        public void LmbClick(int x, int y, int time)
        {
            ///user may forget that right button is pressed or press it by mistake without noticing
            //(holding RMB prevents LMB clicking)
            #if _LINUX
            if (InputX11.GetCursorPos().mask_return == 1024)
            {
                right_up();
            }
#elif WINDOWS

            if (sim.InputDeviceState.IsKeyDown(VirtualKeyCode.RBUTTON))
            {
                right_up();
            }
#endif
            freeze_mouse(x, y, 50);
            left_down();
            freeze_mouse(x, y, time);
            left_up();
            freeze_mouse(x, y, 10);
        }

        public void RmbClick(int x, int y, int time)
        {
            freeze_mouse(x, y, 50);
            right_down();
            freeze_mouse(x, y, time);
            right_up();
            freeze_mouse(x, y, 10);
        }

        public void DlmbClick(int x, int y, int time)
        {
            //user may forget that right button is pressed or press it by mistake without noticing
            //(holding RMB prevents LMB clicking)
#if _LINUX
            if (InputX11.GetCursorPos().mask_return == 1024)
            {
                right_up();
            }
#elif WINDOWS

            if (sim.InputDeviceState.IsKeyDown(VirtualKeyCode.RBUTTON))
            {
                right_up();
            }
#endif
            LmbClick(x, y, 100);
            LmbClick(x, y, 100);
        }

        public void LmbHold(int x, int y, int time)
        {
            freeze_mouse(x, y, 50);
#if _LINUX
            if (InputX11.GetCursorPos().mask_return != 256 || InputX11.GetCursorPos().mask_return != 1280)
            {
                left_down();
            }
            else
            {
                left_up();
            }
#elif _WINDOWS
            if (_sim.InputDeviceState.IsKeyDown(VirtualKeyCode.LBUTTON) == false)
            {
                left_down();
            }
            else
            {
                left_up();
            }
#endif
            freeze_mouse(x, y, time);
        }

        public void RmbHold(int x, int y, int time)
        {
            freeze_mouse(x, y, 50);
#if _LINUX

            if (InputX11.GetCursorPos().mask_return != 1024 || InputX11.GetCursorPos().mask_return != 1280)
            {
                right_down();
            }
            else
            {
                right_up();
            }

#elif _WINDOWS
            if (_sim.InputDeviceState.IsKeyDown(VirtualKeyCode.RBUTTON) == false)
            {
                right_down();
            }
            else
            {
                right_up();
            }
#endif

            freeze_mouse(x, y, time);
        }
        public int[] GetCursorPosition()
        {
            int x;
            int y;

#if _WINDOWS
            {
                Point mousePoint;
                ClicklessMouse.Native.InputWin.GetCursorPos(out mousePoint);
                x = mousePoint.X;
                y = mousePoint.Y;
            }

#elif _LINUX
            {
                var CursorPos = InputX11.GetCursorPos();

                x = CursorPos.root_x;
                y = CursorPos.root_y;
            }

#endif

            return [x, y];
        }
        public void SetCursorPosition(int x, int y)
        {
#if _WINDOWS
            ClicklessMouse.Native.InputWin.SetCursorPos(x, y);
#elif _LINUX
            InputX11.SetCursorPos(x, y);

#endif
        }
    }
}