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
        int _side = 1;
        int _lineWidth = 1;
        Color _c1;
        Color _c2;

        public Square(int side, int lineWidth, Color color1, Color color2)
        {
            InitializeComponent();

            Background = new SolidColorBrush(Colors.Transparent);
            SystemDecorations = SystemDecorations.BorderOnly;
            ShowInTaskbar = false;
            _side = side;
            _lineWidth = lineWidth;
            _c1 = color1;
            _c2 = color2;
            ShowActivated = false;
            CanResize=false;

            //this solves blinking problem that sometimes happens when squares are regenerated
            this.PointToClient(new PixelPoint(0, 0));
            this.Position = new PixelPoint(_side * -1, _side * -1);
#if _LINUX 
Avalonia.Controls.X11Properties.SetNetWmWindowType(this,Avalonia.Controls.Platform.X11NetWmWindowType.Utility);
#endif
        }
     
        public sealed override void Render(DrawingContext context)
        {
            if (Background != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var rectangle1 = new Rect((int)(_lineWidth / 2), (int)(_lineWidth / 2), _side - _lineWidth, _side - _lineWidth);
                    Avalonia.Media.Pen p = new Avalonia.Media.Pen(new SolidColorBrush(_c1), _lineWidth);
                    context.DrawRectangle(null, p, rectangle1);

                    var rectangle2 = new Rect((int)(_lineWidth / 2) + _lineWidth, (int)(_lineWidth / 2) + _lineWidth,
                        _side - 3 * _lineWidth, _side - 3 * _lineWidth);
                    p = new Avalonia.Media.Pen(new SolidColorBrush(_c2), _lineWidth);
                    context.DrawRectangle(null, p, rectangle2);
                }
                    );
            }

            base.Render(context);
        }
    }
}