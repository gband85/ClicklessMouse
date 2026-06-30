using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using Avalonia;

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

            HBhomepage.PropertyChanged += HBhomepage_StateChanged;
        }

        async private void ShowError(Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error WC001", ex.Message, ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error);
            var result = await box.ShowAsync();
        }
        private void Beula_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            WindowEula windowEula = new WindowEula();
            windowEula.ShowDialog(this);
        }

        private void Bchangelog_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            WindowChangelog windowChangelog = new WindowChangelog();
            windowChangelog.ShowDialog(this);
        }
        private void HBhomepage_StateChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.ToString() == "IsVisited" && HBhomepage.IsVisited == true)
            {
                HBhomepage.IsVisited = false;
            }
        }
    }
}