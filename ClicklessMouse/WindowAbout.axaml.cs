using System.Diagnostics;
using System.Windows.Input;
using System.Windows;
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ClicklessMouse
{
    /// <summary>
    /// Interaction logic for WindowAbout.xaml
    /// </summary>
    public partial class WindowAbout : Avalonia.Controls.Window
    {
        public WindowAbout()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error WC001", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Beula_Click(object sender, RoutedEventArgs e)
        {
            WindowEULA w = new WindowEULA();
            w.Show();
        }

        private void Lhomepage_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://" + Lhomepage.Content.ToString());
        }

        private void Lhomepage_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Lhomepage_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void Bchangelog_Click(object sender, RoutedEventArgs e)
        {
            WindowChangelog wc = new WindowChangelog();
            wc.Show();
        }
    }
}
