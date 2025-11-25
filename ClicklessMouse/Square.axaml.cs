using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using ClicklessMouse.Native;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using X11;
using Color = Avalonia.Media.Color;
using Window = X11.Window;

namespace ClicklessMouse
{
    public partial class Square : Avalonia.Controls.Window
    {
        int side = 1;
        int line_width = 1;
        Color c1;
        Color c2;

        public Square(int Side, int Line_width, Color color1, Color color2)
        {
            InitializeComponent();

            Background = new SolidColorBrush(Colors.Transparent);
            SystemDecorations = SystemDecorations.None;
            ShowInTaskbar = false;
            side = Side;
            line_width = Line_width;
            c1 = color1;
            c2 = color2;
            ShowActivated = false;

            //this solves blinking problem that sometimes happens when squares are regenerated
            this.PointToClient(new PixelPoint(0, 0));
            this.Position = new PixelPoint(side * -1, side * -1);
#if _LINUX 
HideSquareTaskbarIcon();
Console.Write("");
#endif
        }
     #if _LINUX
        [DllImport("libX11.so.6")]
        public static extern IntPtr XOpenDisplay(IntPtr display);

        [DllImport("libX11.so.6")]
        public static extern ulong XDefaultRootWindow(IntPtr display);

        [DllImport("libX11.so.6")]
        public static extern ulong XInternAtom(IntPtr display, string atomName, bool onlyIfExists);
     public void HideSquareTaskbarIcon()
        {
                            
//             IntPtr display;
// X11.Window w;
//             Atom net_client_list;
             long long_offset = 0L;
             long long_length = 256L;
             bool delete=false;
//             Atom req_type; 
//             
//
            IntPtr prop_return;
             Atom actual_type_return=new Atom();
             int actual_format_return=0;
             ulong nitems_return = 0UL;
                 ulong nitems_return2 = 0UL;
                 ulong bytes_after_return=0UL;
             List<Window> prop_return_list= [];


           IntPtr display = Xlib.XOpenDisplay(null);
Window w = Xlib.XDefaultRootWindow(display);
IntPtr prop_return2;
Atom net_client_list = Xlib.XInternAtom(display, "_NET_CLIENT_LIST", true);
         Atom  prop = Xlib.XInternAtom(display, "_NET_WM_NAME", true);
         Atom req_type2 = 0UL;
         string window_name="";
           // Atom.
           
           int result = InputX11.XGetWindowProperty(display, w, net_client_list, long_offset, long_length, delete, req_type2, ref actual_type_return, ref actual_format_return,
               ref nitems_return, ref bytes_after_return, out prop_return);
            //if (result && actual_type_return == XA_WINDOW) ;
            ulong count = nitems_return - 1UL;
            for (ulong i = 0; i <= count; i++)
            {
                IntPtr ptr = new IntPtr(prop_return.ToInt64() +  (int)i * 8);
               // ulong windowId = (ulong)Marshal.ReadInt64(ptr);
                // Window g = (Window)windowId;
                // Console.WriteLine($"Id:");
                prop_return_list.Add((Window)Marshal.ReadInt64(ptr));
                InputX11.XGetWindowProperty(display, (Window)Marshal.ReadInt64(ptr), prop, long_offset, long_length,
                    false, req_type2, ref actual_type_return, ref actual_format_return, ref nitems_return2,
                    ref bytes_after_return, out prop_return2);
                // var t = Marshal.ReadInt64(prop_return2, 8).ToString();
                Xlib.XFetchName(display, (Window)Marshal.ReadInt64(prop_return2,(int)i*8), ref window_name);
Console.WriteLine(window_name);
            }
            
Console.WriteLine("");
        }
#endif
        public sealed override void Render(DrawingContext context)
        {
            if (Background != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var Rectangle1 = new Rect((int)(line_width / 2), (int)(line_width / 2), side - line_width, side - line_width);
                    Avalonia.Media.Pen p = new Avalonia.Media.Pen(new SolidColorBrush(c1), line_width);
                    context.DrawRectangle(null, p, Rectangle1);

                    var Rectangle2 = new Rect((int)(line_width / 2) + line_width, (int)(line_width / 2) + line_width,
                        side - 3 * line_width, side - 3 * line_width);
                    p = new Avalonia.Media.Pen(new SolidColorBrush(c2), line_width);
                    context.DrawRectangle(null, p, Rectangle2);
                }
                    );
            }

            base.Render(context);
        }
    }
}