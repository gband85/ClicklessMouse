using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvRichTextBox;
using DocumentFormat.OpenXml.Bibliography;
using HarfBuzzSharp;
using System;
using System.Windows;
//using System.Windows.Documents;
//using System.Windows.Forms;
//using System.Windows.Media.Imaging;

namespace ClicklessMouse
{
    public partial class MainWindow : Window
    {
        string error_title, about_title, about_content, contentLscreen_resolution,
            argb_error, screen_size_error1, screen_size_error2, time_to_start_mouse_movement_error,
            cursor_time_in_square_error, min_square_size_too_low_error, min_square_size_too_high_error,
            square_size_error, square_border_error, cursor_idle_time_error, loading_error_msg;

        enum language
        {
            en,
            pl
        }

        void change_language(language lang)
        {
            set_instructions(lang);

            if (lang == language.en)
            {
                MIfile.Header = "File";
                MIexit.Header = "Exit";
                MIrestore.Header = "Restore";
                MIdefault_colors.Header = "Default Square Colors";
                MIdefault_settings.Header = "All Default Settings";
                MIlanguage.Header = "Language";
                MIenglish.Header = "English";
                MIpolish.Header = "Polish";
                MIhelp.Header = "Help";
                MImanual.Header = "User Guide";
                MIabout.Header = "About";
                GBmain_settings.Header = "Main Settings";
                CHBLMB.Content = "LMB click";
                CHBRMB.Content = "RMB click";
                CHBdoubleLMB.Content = "LMB double click";
                CHBholdLMB.Content = "LMB holding";
                CHBholdRMB.Content = "RMB holding";
                CHBscreen_panning.Content = "Screen panning";
                Lcursor_idle_before_squares_appear.Text = "Cursor idle time before squares appear [ms]:";
                Ltime_to_start_mouse.Text = "Time to start mouse movement after squares appear [ms]:";
                Lcursor_time_in_square.Text = "Cursor time in square to register click [ms]:";
                GBother_settings.Header = "Other Settings";
                CHBrun_at_startup.Content = "Run when computer starts";
                CHBstart_minimized.Content = "Start minimized";
                CHBminimize_to_tray.Content = "Minimize to system tray";
                CHBcheck_for_updates.Content = "Check for updates automatically";
                GBsquare_settings.Header = "Squares Settings";
                Lsquare_size.Content = "Size [px]:";
                Lsquare_border.Content = "Border width [px]:";
                Lsquare_color1.Content = "Color 1:";
                Lsquare_color2.Content = "Color 2:";
                Lmin_square_size.Content = "Minimum size [%]:";
                GBrecommended_square_size.Header = "Recommended Square Size";
                Lscreen_size.Content = "Diagonal screen size [in]:";
                contentLscreen_resolution = "Screen resolution: ";
                //int x = Screen.PrimaryScreen.Bounds.Width;
                //int y = Screen.PrimaryScreen.Bounds.Height;
                Lscreen_resolution.Content = "Screen resolution: "; 
                TBscreen_resolution.Text = x + "x" + y;
                //contentLscreen_resolution + x + "x" + y;
                Bset_recommended_square.Content = "Set recommended square size";

                error_title = "Error";
                about_title = "About";
                about_content = "Version: " + prog_version + "\nAuthor: Mikołaj Magowski";
                argb_error = "ARGB color value must be lower than 0";
                screen_size_error1 = "Diagonal screen size must be higher than 0";
                screen_size_error2 = "Diagonal screen size can't be empty";
                cursor_idle_time_error = "Cursor idle time before squares appear cannot be lower than ";
                time_to_start_mouse_movement_error = "Time to start mouse movement cannot be lower than ";
                cursor_time_in_square_error = "Cursor time in square to register click cannot be "
                    + "lower than ";
                square_size_error = "Square size cannot be lower than ";
                square_border_error = "Square border width cannot be lower than ";
                min_square_size_too_low_error = "Minimum square size cannot be lower than ";
                min_square_size_too_high_error = "Minimum square size cannot be higher than ";
                loading_error_msg = " Settings loading error - values that weren't loaded will be " +
                    "restored to default and saved.";
            }
            else if (lang == language.pl)
            {
                MIfile.Header = "Plik";
                MIexit.Header = "Zakończ";
                MIrestore.Header = "Przywróć";
                MIdefault_colors.Header = "Domyślne kolory kwadratu";
                MIdefault_settings.Header = "Wszystkie ustawienia domyślne";
                MIlanguage.Header = "Język";
                MIenglish.Header = "Angielski";
                MIpolish.Header = "Polski";
                MIhelp.Header = "Pomoc";
                MImanual.Header = "Instrukcja obsługi";
                MIabout.Header = "O programie";
                GBmain_settings.Header = "Ustawienia główne";
                CHBLMB.Content = "Klik LPM";
                CHBRMB.Content = "Klik PPM";
                CHBdoubleLMB.Content = "Dwuklik LPM";
                CHBholdLMB.Content = "Przytrzymanie LPM";
                CHBholdRMB.Content = "Przytrzymanie PPM";
                CHBscreen_panning.Content = "Przesuwanie ekranu";
                Lcursor_idle_before_squares_appear.Text = "Czas bezczynności kursora zanim pojawią" +
                    " się kwadraty [ms]:";
                Ltime_to_start_mouse.Text = "Czas na rozpoczęcie ruchu myszą po pojawieniu się "
                    + "kwadratów [ms]:";
                Lcursor_time_in_square.Text = "Czas kursora w kwadracie do zarejestrowania kliknięcia [ms]:";
                GBother_settings.Header = "Pozostałe ustawienia";
                CHBrun_at_startup.Content = "Uruchom przy starcie komputera";
                CHBstart_minimized.Content = "Uruchom zminimalizowany";
                CHBminimize_to_tray.Content = "Minimalizuj do zasobnika systemowego";
                CHBcheck_for_updates.Content = "Automatycznie sprawdzaj aktualizacje";
                GBsquare_settings.Header = "Ustawienia kwadratów";
                Lsquare_size.Content = "Rozmiar [px]:";
                Lsquare_border.Content = "Szerokość krawędzi [px]:";
                Lsquare_color1.Content = "Kolor 1:";
                Lsquare_color2.Content = "Kolor 2:";
                Lmin_square_size.Content = "Minimalny rozmiar [%]:";
                GBrecommended_square_size.Header = "Zalecany rozmiar kwadratu";
                Lscreen_size.Content = "Przekątna ekranu [cale]:";
                contentLscreen_resolution = "Rozdzielczość ekranu: ";
                //int x = Screen.PrimaryScreen.Bounds.Width;
                //int y = Screen.PrimaryScreen.Bounds.Height;
                Lscreen_resolution.Content = contentLscreen_resolution + x + "x" + y;
                Bset_recommended_square.Content = "Ustaw zalecaną wielkość kwadratu";

                error_title = "Błąd";
                about_title = "O programie";
                about_content = "Wersja: " + prog_version + "\nAutor: Mikołaj Magowski";
                argb_error = "Wartość ARGB koloru musi być mniejsza od 0";
                screen_size_error1 = "Przekątna ekranu musi być większa od 0";
                screen_size_error2 = "Przekątna ekranu nie może być pusta";
                cursor_idle_time_error = "Czas bezczynności kursora zanim pojawią się kwadraty nie może" +
                    " być niższy niż ";
                time_to_start_mouse_movement_error = "Czas na rozpoczęcie ruchu myszą nie może być "
                    + "niższy niż ";
                cursor_time_in_square_error = "Czas kursora w kwadracie do zarejestrowania kliknięcia"
                    + " nie może byc niższy niż ";
                square_size_error = "Rozmiar kwadratu nie może być niższy niż ";
                square_border_error = "Rozmiar krawędzi kwadratu nie może być niższy niż ";
                min_square_size_too_low_error = "Minimalny rozmiar kwadratu nie może być niższy niż ";
                min_square_size_too_high_error = "Minimalny rozmiar kwadratu nie może być wyższy niż ";
                loading_error_msg = " Błąd odczytu ustawień - wartości, które nie zostały wczytane " +
                    "zostaną przywrócone do domyślnych i zapisane.";
            }
        }

        void set_instructions(language lang)
        {
            if (lang == language.en)
            {
                wm.Title = "User Guide";
                wm.RTBinstructions.Source = new Uri("avares://ClicklessMouse/Assets/1en.md");
            }
            else if (lang == language.pl)
            {
                wm.Title = "Instrukcja obsługi";
                wm.RTBinstructions.Source = new Uri("avares://ClicklessMouse/Assets/1pl.md");
            }
        }
    }
}