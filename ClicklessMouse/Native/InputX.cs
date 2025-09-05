using Avalonia;
using DocumentFormat.OpenXml.Drawing.Charts;
using DynamicData.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;
using X11;

namespace ClicklessMouse.Native
{
#if _LINUX
    public partial class InputX11
    {
        [DllImport("libX11.so.6")]
        public static extern int XWarpPointer(IntPtr display, Window src_w, Window dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y);

        [DllImport("libXtst.so.6")]
        public static extern int XTestFakeKeyEvent(IntPtr display, uint keycode, bool is_press, ulong delay);

        [DllImport("libX11.so.6")]
        public static extern int XGetWindowProperty(IntPtr display, Window w, Atom property, long long_offset, long long_length, bool delete, Atom req_type,
                        ref Atom actual_type_return, ref int actual_format_return, ref ulong nitems_return, ref ulong bytes_after_return,
                        ref string prop_return);

        public static int[] GetCursorPos()
        {
            //int number_of_screens;
            IntPtr display = Xlib.XOpenDisplay(null);
            //number_of_screens = Xlib.XScreenCount(display);
            //IntPtr root_windows;
            //root_windows = sizeof(Window) * number_of_screens;
            Window w = Xlib.XDefaultRootWindow(display);

            Window window_return = new();
            Window child_return = new();

            int root_x = new();
            int root_y = new();
            int win_x = new();
            int win_y = new();
            uint mask_return = new uint();
            Xlib.XQueryPointer(display, w, ref window_return, ref child_return, ref root_x, ref root_y, ref win_x, ref win_y, ref mask_return);
            Xlib.XCloseDisplay(display);
            int[] coords = new int[2];
            coords[0] = root_x;
            coords[1] = root_y;
            return coords;
        }

        public static void SetCursorPos(int x, int y)
        {
            IntPtr display = Xlib.XOpenDisplay(null);
            X11.Window w = Xlib.XDefaultRootWindow(display);
            XWarpPointer(display, X11.Window.None, w, 0, 0, 0, 0, x, y);
            Xlib.XCloseDisplay(display);
        }

        public static void LeftButtonDown()
        {
                    IntPtr display = Xlib.XOpenDisplay(null);
                    XTest.XTestFakeButtonEvent(display, Button.LEFT, 1, 0);
                    Xlib.XCloseDisplay(display);
        }

        public static void LeftButtonUp()
        {
            IntPtr display = Xlib.XOpenDisplay(null);
            XTest.XTestFakeButtonEvent(display, Button.LEFT, 0, 0);
            Xlib.XCloseDisplay(display);
        }
        
        public static void RightButtonDown()
        {
            IntPtr display = Xlib.XOpenDisplay(null);
            XTest.XTestFakeButtonEvent(display, Button.RIGHT, 1, 0);
            Xlib.XCloseDisplay(display);
        }
        
        public static void RightButtonUp()
        {
            IntPtr display = Xlib.XOpenDisplay(null);
            XTest.XTestFakeButtonEvent(display, Button.RIGHT, 0, 0);
            Xlib.XCloseDisplay(display);
        }

        public static void KeyDown(VirtualKeyCode code)
        {
            IntPtr display = Xlib.XOpenDisplay(null);
            if (code == VirtualKeyCode.LEFT)
                XTestFakeKeyEvent(display, 113, true, 0);
            else if (code == VirtualKeyCode.RIGHT)
                XTestFakeKeyEvent(display, 114, true, 0);
            else if (code == VirtualKeyCode.UP)
                XTestFakeKeyEvent(display, 111, true, 0);
            else if (code == VirtualKeyCode.DOWN)
                XTestFakeKeyEvent(display, 116, true, 0);
            Xlib.XCloseDisplay(display);
        }

        public static void KeyUp(VirtualKeyCode code)
        {
            IntPtr display = Xlib.XOpenDisplay(null);
            if (code == VirtualKeyCode.LEFT)
                XTestFakeKeyEvent(display, 113, false, 0);
            else if (code == VirtualKeyCode.RIGHT)
                XTestFakeKeyEvent(display, 114, false, 0);
            else if (code == VirtualKeyCode.UP)
                XTestFakeKeyEvent(display, 111, false, 0);
            else if (code == VirtualKeyCode.DOWN)
                XTestFakeKeyEvent(display, 116, false, 0);
            Xlib.XCloseDisplay(display);
        }
    }
#endif
}
