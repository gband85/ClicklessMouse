using System;
using Avalonia.Controls;
using System.Globalization;
using System.IO;
using Avalonia.Platform;

namespace ClicklessMouse
{
    public partial class MainWindow : Window
    {
        public enum UILanguage
        {
            en,
            pl
        }

        void change_language(UILanguage lang)
        {
            if (lang == UILanguage.en)
            {
                L10nResourceMgr.Instance.Culture = new CultureInfo("en-US");
                MIenglish.IsChecked = true;
                MIpolish.IsChecked = false;
            }

            else if (lang == UILanguage.pl)
            {
                L10nResourceMgr.Instance.Culture = new CultureInfo("pl-PL");
                MIenglish.IsChecked = false;
                MIpolish.IsChecked = true;
            }

        }
    }
}