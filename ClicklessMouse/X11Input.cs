using Avalonia;
using DynamicData.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using X11;

namespace ClicklessMouse
{
    public partial class X11Input
    {

        public static int[] GetCursorPos()
        {
            int number_of_screens;
            IntPtr display = Xlib.XOpenDisplay(null);
            number_of_screens = Xlib.XScreenCount(display);
            IntPtr root_windows;
            root_windows = sizeof(Window) * number_of_screens;
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
            coords.Prepend(root_x);
            coords.Append(root_y);
            return coords;
        }
        //XWarpPointer(display, src_w, dest_w, src_x, src_y, src_width, src_height, dest_x,
        //        dest_y)
        //Display* display;
        //Window src_w, dest_w;
        //int src_x, src_y;
        //unsigned int src_width, src_height;
        //int dest_x, dest_y;



        //static bool CheckKey()
        //{
        //    X11.XKeyEvent
        //    return true;
        //}
    }
}
