using System;
using Avalonia.Controls;
using System.Globalization;
using System.IO;
using Avalonia.Platform;

namespace ClicklessMouse
{
    public partial class MainWindow : Window
    {
        public enum UiLanguage
        {
            En,
            Pl
        }

        void change_language(UiLanguage lang)
        {
            if (lang == UiLanguage.En)
            {
                L10NResourceMgr.Instance.Culture = new CultureInfo("en-US");
                MIenglish.IsChecked = true;
                MIpolish.IsChecked = false;
            }

            else if (lang == UiLanguage.Pl)
            {
                L10NResourceMgr.Instance.Culture = new CultureInfo("pl-PL");
                MIenglish.IsChecked = false;
                MIpolish.IsChecked = true;
            }

        }
    }
}