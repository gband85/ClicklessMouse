using System;
using System.Runtime.InteropServices;
using WindowsInput.Native;
using X11;

namespace ClicklessMouse.Native
{
#if _LINUX
    public partial class InputX11
    {
        public static (Window window_return,Window child_return,int root_x,int root_y,int win_x,int win_y,uint mask_return) GetCursorPos()
        {
            //int number_of_screens;
            IntPtr display = Xlib.XOpenDisplay(null);
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

            return (window_return,child_return,root_x,root_y,win_x,win_y,mask_return);
        }
     
        public static void SetCursorPos(int x, int y)
        {
            IntPtr display = Xlib.XOpenDisplay(null);
            Window w = Xlib.XDefaultRootWindow(display);
            Xlib.XWarpPointer(display, Window.None, w, 0, 0, 0, 0, x, y);
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
                XTest.XTestFakeKeyEvent(display, Xlib.XKeysymToKeycode(display, KeySym.XK_Left), true, 0);
            else if (code == VirtualKeyCode.RIGHT)
                XTest.XTestFakeKeyEvent(display, Xlib.XKeysymToKeycode(display, KeySym.XK_Right), true, 0);
            else if (code == VirtualKeyCode.UP)
                XTest.XTestFakeKeyEvent(display, Xlib.XKeysymToKeycode(display, KeySym.XK_Up), true, 0);
            else if (code == VirtualKeyCode.DOWN)
                XTest.XTestFakeKeyEvent(display, Xlib.XKeysymToKeycode(display, KeySym.XK_Down), true, 0);
            Xlib.XCloseDisplay(display);
        }

        public static void KeyUp(VirtualKeyCode code)
        {
            IntPtr display = Xlib.XOpenDisplay(null);
            if (code == VirtualKeyCode.LEFT)
                XTest.XTestFakeKeyEvent(display, Xlib.XKeysymToKeycode(display, KeySym.XK_Left), false, 0);
            else if (code == VirtualKeyCode.RIGHT)
                XTest.XTestFakeKeyEvent(display, Xlib.XKeysymToKeycode(display, KeySym.XK_Right), false, 0);
            else if (code == VirtualKeyCode.UP)
                XTest.XTestFakeKeyEvent(display, Xlib.XKeysymToKeycode(display, KeySym.XK_Up), false, 0);
            else if (code == VirtualKeyCode.DOWN)
                XTest.XTestFakeKeyEvent(display, Xlib.XKeysymToKeycode(display, KeySym.XK_Down), false, 0);
            Xlib.XCloseDisplay(display);
        }
    }
#endif
}