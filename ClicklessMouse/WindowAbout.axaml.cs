using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
                ShowError(ex);
            }
        }

        async private void ShowError(Exception ex)
        {
var box = MessageBoxManager.GetMessageBoxStandard("Error WC001", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
        }
        private void Beula_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            WindowEULA w = new WindowEULA();
            w.Show();
        }

        private void Bchangelog_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            WindowChangelog wc = new WindowChangelog();
            wc.Show();
        }
    }
}
