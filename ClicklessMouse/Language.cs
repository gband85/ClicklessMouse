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
            Uri asset_uri;
            Stream stream;
            StreamReader reader;
            if (lang == UILanguage.en)
            {
                L10nResourceMgr.Instance.Culture = new CultureInfo("en-US");
                asset_uri=new Uri("avares://ClicklessMouse/Assets/1en.md");
                stream =AssetLoader.Open(asset_uri);
                reader = new StreamReader(stream);
                Wmanual.RTBinstructions.Markdown=reader.ReadToEnd();
            }


            else if (lang == UILanguage.pl)
            {
                L10nResourceMgr.Instance.Culture = new CultureInfo("pl-PL");
                asset_uri = new Uri("avares://ClicklessMouse/Assets/1pl.md");
                stream = AssetLoader.Open(asset_uri);
                reader = new StreamReader(stream);
Wmanual.RTBinstructions.Markdown = reader.ReadToEnd();
            }
            
        }
    }
}