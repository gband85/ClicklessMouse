using System;
using Window = Avalonia.Controls.Window;
using MsBox.Avalonia;

namespace ClicklessMouse;

public partial class WindowChangelog : Window
{
    public WindowChangelog()
    {
        try
        {
            InitializeComponent();

            TB.IsReadOnly = true;

            TB.Text = """
                      All notable changes to Clickless Mouse will be documented here.
                      
                      [3.2.0] - June 26, 2026:
                      - Ported to AvaloniaUI.
                      - Now cross-platform for Windows and X11 on Linux.
                      - Other minor improvements.
                      
                      [2.2] - January 28, 2024:
                      - Added automatic check for updates.
                      - Changed recommended square size.
                      - Improved UI.
                      
                      [2.1] - December 7, 2023:
                      - Clickless Mouse from now on requires administrator rights to run.
                      - Default "Cursor time in square to register a click" is now 100ms.
                      - Improved mouse button holding and releasing.
                      - Other minor improvements.
                      """;
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    async private void ShowError(Exception ex)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Error WC001", ex.Message, MsBox.Avalonia.Enums.ButtonEnum.Ok,
            MsBox.Avalonia.Enums.Icon.Error);
        await box.ShowAsync();
    }
}