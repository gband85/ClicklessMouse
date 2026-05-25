using Avalonia.Controls;

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
            Hide();
        }
    }
}