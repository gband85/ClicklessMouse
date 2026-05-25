using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;

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
            WindowEula w = new WindowEula();
            w.Show();
        }

        private void Bchangelog_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            WindowChangelog wc = new WindowChangelog();
            wc.Show();
        }
    }
}
