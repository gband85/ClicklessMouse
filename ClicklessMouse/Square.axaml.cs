using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using ClicklessMouse.Native;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using X11;
using Color = Avalonia.Media.Color;

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
HideSquare();
#endif
        }
     #if _LINUX
     public void HideSquare()
        {
                            
            nint display;
X11.Window w;
            Atom net_client_list;
            long long_offset = 0;
            long long_length = ~0L;
            bool delete=false;
            Atom req_type; 
            

            Atom actual_type_return=new Atom();
            int actual_format_return=new int();
            ulong nitems_return =new ulong();
                ulong bytes_after_return=new ulong();
            string prop_return=new("");


            display = Xlib.XOpenDisplay(null);
 w = Xlib.XDefaultRootWindow(display);
net_client_list = Xlib.XInternAtom(display, "_NET_CLIENT_LIST", false);
           req_type = Xlib.XInternAtom(display, "AnyPropertyType", false);
           
           
            int result = InputX11.XGetWindowProperty(display,w,net_client_list, long_offset,long_length, delete,req_type, ref actual_type_return,ref actual_format_return,ref nitems_return, ref bytes_after_return,ref prop_return );
            //if (result && actual_type_return == XA_WINDOW) ;

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