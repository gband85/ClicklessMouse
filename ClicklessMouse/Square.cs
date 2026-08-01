using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Media;
using Avalonia.Threading;
using Color = Avalonia.Media.Color;

namespace ClicklessMouse
{
    public partial class Square : Window
    {
        private int _side = 1;
        private int _lineWidth = 1;
        private Color _c1;
        private Color _c2;
        public int StartX, StartY, EndX, EndY;

        public Square(int side, int lineWidth, Color color1, Color color2)
        {
            // InitializeComponent();

            Background = new SolidColorBrush(Colors.Transparent);
            WindowDecorations = WindowDecorations.None;
            ShowInTaskbar = false;
            _side = side;
            _lineWidth = lineWidth;
            _c1 = color1;
            _c2 = color2;
            ShowActivated = false;
            CanResize=false;

            //this solves blinking problem that sometimes happens when squares are regenerated
            this.PointToClient(new PixelPoint(0, 0));
            Position = new PixelPoint(_side * -1, _side * -1);
#if _LINUX 
X11Properties.SetNetWmWindowType(this,X11NetWmWindowType.Dock);
#endif
        }
     
        public sealed override void Render(DrawingContext context)
        {
            if (Background != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var rectangle1 = new Rect((int)(_lineWidth / 2), (int)(_lineWidth / 2), _side - _lineWidth, _side - _lineWidth);
                    Pen p = new Pen(new SolidColorBrush(_c1), _lineWidth);
                    context.DrawRectangle(null, p, rectangle1);

                    var rectangle2 = new Rect((int)(_lineWidth / 2) + _lineWidth, (int)(_lineWidth / 2) + _lineWidth,
                        _side - 3 * _lineWidth, _side - 3 * _lineWidth);
                    p = new Pen(new SolidColorBrush(_c2), _lineWidth);
                    context.DrawRectangle(null, p, rectangle2);
                }
                    );
            }

            base.Render(context);
        }
    }
}