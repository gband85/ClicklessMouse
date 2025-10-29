using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace ClicklessMouse
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class WindowManual : Window
    {
        public WindowManual()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, WindowClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}