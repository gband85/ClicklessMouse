using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ClicklessMouse;

public partial class Square : Window
{
    public Square(int Side, int Line_width, System.Drawing.Color color1, System.Drawing.Color color2)
    {
        InitializeComponent();
    }
}