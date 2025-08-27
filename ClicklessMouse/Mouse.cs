using Avalonia.Automation.Peers;
using Avalonia.Controls;
using ClicklessMouse.Native;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using WindowsInput;
using WindowsInput.Native;
using X11;
using Window = Avalonia.Controls.Window;

namespace ClicklessMouse
{
    public partial class MainWindow : Window
    {
        void left_down()
        {
#if _LINUX
                ClicklessMouse.Native.InputX11.LeftButtonDown();
#elif _WINDOWS
            sim.Mouse.LeftButtonDown();
#endif
        }

        void left_up()
        {
#if _LINUX
                ClicklessMouse.Native.InputX11.LeftButtonUp();
#elif _WINDOWS
            sim.Mouse.LeftButtonUp();
#endif
        }

        void right_down()
        {
#if _LINUX
                ClicklessMouse.Native.InputX11.RightButtonDown();
#elif _WINDOWS
            sim.Mouse.RightButtonDown();
#endif
        }

        void right_up()
        {
#if _LINUX
            ClicklessMouse.Native.InputX11.RightButtonUp();
#elif _WINDOWS
            sim.Mouse.RightButtonUp();
#endif
        }

        void freeze_mouse(int X, int Y, int time)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
#if _LINUX
                    ClicklessMouse.Native.InputX11.SetCursorPos(X,Y);
#elif _WINDOWS
                ClicklessMouse.Native.InputWin.SetCursorPos(X, Y);
#endif
                Thread.Sleep(1);
            }
            while (stopwatch.ElapsedMilliseconds < time);
        }

        public void LMBClick(int X, int Y, int time)
        {
            ///user may forget that right button is pressed or press it by mistake without noticing
            //(holding RMB prevents LMB clicking)
            // if (sim.InputDeviceState.IsKeyDown(VirtualKeyCode.RBUTTON))
            // {
            //     right_up();
            // }
            freeze_mouse(X, Y, 50);
            left_down();
            freeze_mouse(X, Y, time);
            left_up();
            freeze_mouse(X, Y, 10);
        }

        public void RMBClick(int X, int Y, int time)
        {
            freeze_mouse(X, Y, 50);
            right_down();
            freeze_mouse(X, Y, time);
            right_up();
            freeze_mouse(X, Y, 10);
        }

        public void DLMBClick(int X, int Y, int time)
        {
            //user may forget that right button is pressed or press it by mistake without noticing
            //(holding RMB prevents LMB clicking)
            if (sim.InputDeviceState.IsKeyDown(VirtualKeyCode.RBUTTON))
            {
                right_up();
            }
            LMBClick(X, Y, 100);
            LMBClick(X, Y, 100);
        }

        public void LMBHold(int X, int Y, int time)
        {
            freeze_mouse(X, Y, 50);
            if (sim.InputDeviceState.IsKeyDown(VirtualKeyCode.LBUTTON) == false)
            {
                left_down();
            }
            else
            {
                left_up();
            }
            freeze_mouse(X, Y, time);
        }

        public void RMBHold(int X, int Y, int time)
        {
            freeze_mouse(X, Y, 50);
            if (sim.InputDeviceState.IsKeyDown(VirtualKeyCode.RBUTTON) == false)
            {
                right_down();
            }
            else
            {
                right_up();
            }
            freeze_mouse(X, Y, time);
        }
        public int[] GetCursorPosition()
        {
            int x;
            int y;

#if _WINDOWS
            {
                Point MousePoint;
                ClicklessMouse.Native.InputWin.GetCursorPos(out MousePoint);
                x = MousePoint.X;
                y = MousePoint.Y;
            }

#elif _LINUX
            {
            int[] MouseCoords;
                MouseCoords = ClicklessMouse.Native.InputX11.GetCursorPos();
                
                x = MouseCoords[0];
                y = MouseCoords[1];
            }

#endif

            return [x,y];
        }
        public void SetCursorPosition(int x, int y)
        {
#if _WINDOWS
            ClicklessMouse.Native.InputWin.SetCursorPos(x, y);
#elif _LINUX
            ClicklessMouse.Native.InputX11.SetCursorPos(x, y);

#endif
        }
    }
}