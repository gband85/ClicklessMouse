using Avalonia.Controls;
using System.Globalization;

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
                L10nResourceMgr.Instance.Culture = new CultureInfo("en-US");
            else if (lang == UILanguage.pl)
                L10nResourceMgr.Instance.Culture = new CultureInfo("pl-PL");
        }
    }
}