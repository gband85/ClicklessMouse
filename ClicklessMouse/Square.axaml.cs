using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using System.Drawing;
using System.Windows.Forms;
using Color = Avalonia.Media.Color;

namespace ClicklessMouse
{
    public partial class Square : Window
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

            //this solves blinking problem that sometimes happens when squares are regenerated
            this.PointToClient(new PixelPoint(0, 0));
            this.Position = new PixelPoint(side * -1, side * -1);
        }

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