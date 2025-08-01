using System;
using System.Windows;
using System.Windows;


using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Platform;
//Documents;
//using System.Windows.Forms;
//using System.Windows.Media.Imaging;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Controls.Documents;
using Avalonia.Platform;
//using Screen = System.Windows.Forms.Screen;
using Window = Avalonia.Controls.Window;
using System.Security.Policy;

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
                //int x = Screen.PrimaryScreen.Bounds.Width;
                //int y = Screen.PrimaryScreen.Bounds.Height;
                //TBLscreen_resolution.Text = x + "x" + y;

                about_content = "Version: " + prog_version + "\nAuthor: Mikołaj Magowski";

            }
            else if(lang == language.pl)
            {
                //int x = Screen.PrimaryScreen.Bounds.Width;
                //int y = Screen.PrimaryScreen.Bounds.Height;
                //TBLscreen_resolution.Text = x + "x" + y;
                about_content = "Wersja: " + prog_version + "\nAutor: Mikołaj Magowski";
            }
        }

        void set_instructions(language lang)
        {
            if(lang == language.en)
            {
                wm.Title = "User Guide";
                wm.RTBinstructions.Source = new Uri("avares://ClicklessMouse/Assets/1en.md");
            }
            else if (lang == language.pl)
                {
                wm.Title = "Instrukcja obsługi";
                wm.RTBinstructions.Source = new Uri("avares://ClicklessMouse/Assets/1pl.md");
            }
            //else if(lang == language.pl)
            //{
            //    wm.Title = "Instrukcja obsługi";

            //    FlowDocument fd = new FlowDocument();
            //    Paragraph p1 = new Paragraph();
            //    //Paragraph p2 = new Paragraph();
            //    p1.Inlines.Add(new Run("Clickless Mouse ułatwia użytkowanie komputera osobą z RSI " +
            //        "(repetitive strain injury),"
            //        + " zespołem cieśni nadgarstka, niektórymi niepełnosprawnościami ruchowymi oraz"
            //        + " innymi problemami zdrowotnymi."
            //        + "\n\nTa aplikacja umożliwia używanie myszki bez klikania - jedynie poprzez"
            //        + " jej przesuwanie."
            //        + "\n\nPoprzez reagowanie na poruszanie myszą przez użytkownika program symuluje"
            //        + " lewy/prawy przycisk myszy, podwójne kliknięcie lewym przyciskiem myszy "
            //        + "oraz przytrzymywanie lewego/prawego przycisku myszy."
            //        + "\n\nClickless Mouse można używać z klawiaturą wirtualną do pisania poprzez " +
            //        "poruszanie myszką (np. Free Virtual Keyboard: "));

            //    Hyperlink link = new Hyperlink();
            //    link.IsEnabled = true;
            //    link.Inlines.Add("https://freevirtualkeyboard.com/");
            //    link.NavigateUri = new Uri("https://freevirtualkeyboard.com/");
            //    link.RequestNavigate += (sender, args) => System.Diagnostics.Process.Start(args.Uri.ToString());
            //    p1.Inlines.Add(link);

            //    p1.Inlines.Add(new Run(").\n\nUżywając Clickless Mouse wraz z wirtualną klawiaturą"
            //        + " użytkownik może w pełni kontrolować komputer poprzez poruszanie myszką."
            //        + "\n\nKiedy chcesz kliknąć/przytrzymać przycisk myszy:"
            //        + " zatrzymaj myszkę, poczekaj aż pojawią się kwadraty, a następnie przesuń"
            //        + " kursor do wybranego kwadratu:"));

            //    List a = new List();
            //    a.MarkerStyle = TextMarkerStyle.Disc;
            //    a.Padding = new Thickness(25, 0, 0, 0);
            //    a.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Górny środkowy kwadrat = podwójne kliknięcie lewym przyciskiem myszy"))));
            //    a.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Górny lewy kwadrat = kliknięcie lewym przyciskiem myszy"))));
            //    a.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Górny prawy kwadrat = kliknięcie prawym przyciskiem myszy"))));
            //    a.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Lewy kwadrat = włącz/wyłącz przytrzymanie lewego przycisku myszy"))));
            //    a.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Prawy kwadrat = włącz/wyłącz przytrzymanie prawego przycisku myszy"))));

            //    Bitmap bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://ClicklessMouse/Assets/1pl.jpg")));
            //    Image image = new Image();
            //    image.Source = bitmap;
            //    image.Width = 521;
            //    //p2.Inlines.Add(image);

            //    //p2.Inlines.Add(new Run("\nKiedy kursor myszy znajduje się wystarczająco długo"
            //    //    + " w kwadracie (w zależności od ustawienia czasu kursora w kwadracie do rejestracji"
            //    //    + " kliknięcia)"
            //    //    + ", przesunie się automatycznie do poprzedniej pozycji, aby zasymulować"
            //    //    + " akcję na podstawie ostatnio odwiedzonego kwadratu."
            //    //    + "\n\nJeśli kursor myszy jest zbyt blisko górnej krawędzi ekranu, kwadraty"
            //    //    + " dla kliknięcia LPM, kliknięcia PPM i dwukliknięcia LPM są pokazywane"
            //    //    + " poniżej kursora myszy."
            //    //    + "\n\nGdy kursor myszy jest zbyt blisko lewej lub prawej krawędzi ekranu"
            //    //    + " rozmiar wszystkich kwadratów zostaje zmniejszony tak aby przynajmniej 25%"
            //    //    + " kwadratu było widoczne. Maksymalne domyślne zmniejszenie rozmiaru kwadratu to"
            //    //    + " 60% oryginalnej wielkości."
            //    //    + "\nWielkość kwadratów nie zmniejsza się, jeśli kursor jest na tyle blisko"
            //    //    + " krawędzi ekranu, że najmniejszy rozmiar nie wystarczy do pokazania kwadratu."
            //    //    + "\n\nIstnieje możliwość wyłączenia niepotrzebnych kwadratów."
            //    //    + "\n\nPrzesuwanie ekranu - gdy tryb ten jest włączony, przesunięcie kursora do" +
            //    //    " krawędzi ekranu powoduje naciśnięcie klawiszy: góra dla górnej krawędzi, dół" +
            //    //    " dla dolnej krawędzi, lewo dla lewej krawędzi i prawo dla prawej krawędzi." +
            //    //    " Gdy przesuwanie ekranu jest aktywne, kwadraty nie są pokazywane kiedy kursor" +
            //    //    " myszy znajduje się na krawędzi ekranu."
            //    //    + "\n\nClickless Mouse działa tylko w programach i grach, które zostały"
            //    //    + " uruchomione w trybie bezramkowym lub okienkowym (tryb pełnoekranowy"
            //    //    + " nie jest wspierany)."));

            //    Paragraph p3 = new Paragraph();
            //    Bold b = new Bold();
            //    b.Inlines.Add(new Run("Pierwsze kroki:"));
            //    p3.Inlines.Add(b);

            //    List a2 = new List();
            //    a2.MarkerStyle = TextMarkerStyle.Decimal;
            //    a2.Padding = new Thickness(25, 0, 0, 0);
            //    a2.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Podaj przekątną ekranu, a następnie naciśnij przycisk 'Ustaw zalecaną wielkość " +
            //        "kwadratu'."))));
            //    a2.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Zdecyduj z jakich funkcji myszki chcesz skorzystać. Większość użytkowników wybiera"
            //        + " 'Klik LPM', 'Klik PPM', 'Dwuklik LPM' oraz 'Przytrzymywanie LPM'."))));
            //    a2.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Jeśli posiadasz niepełnosprawność ruchową rozważ zwiększenie czasu bezczynności" +
            //        " kursora zanim pojawią się kwadraty, czasu na rozpoczęcie " +
            //        "ruchu myszą po pojawieniu się kwadratów oraz rozmiaru kwadratów."))));

            //    Paragraph p4 = new Paragraph();
            //    b = new Bold();
            //    b.Inlines.Add(new Run("Najniższe możliwe wartości (program ignoruje niższe wartości"
            //        + " i używa poniższe zamiast nich):"));
            //    p4.Inlines.Add(b);

            //    List a3 = new List();
            //    a3.MarkerStyle = TextMarkerStyle.Disc;
            //    a3.Padding = new Thickness(25, 0, 0, 0);
            //    a3.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Czas bezczynności kursora zanim pojawią się kwadraty [ms]: "
            //        + lowest_cursor_idle_time_ms))));
            //    a3.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Czas na rozpoczęcie ruchu myszą po pojawieniu się kwadratów [ms]: "
            //        + lowest_time_to_start_mouse_movement_ms))));
            //    a3.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Czas kursora w kwadracie do zarejestrowania kliknięcia [ms]: "
            //        + lowest_cursor_time_in_square_ms))));
            //    a3.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Rozmiar [px]: " + lowest_size))));
            //    a3.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Szerokość krawędzi [px]: " + lowest_border_width))));
            //    a3.ListItems.Add(new ListItem(new Paragraph(new Run(
            //        "Minimalny rozmiar [%]: " + lowest_min_square_size_percents))));

            //    fd.Blocks.Add(p1);
            //    fd.Blocks.Add(a);
            //    //fd.Blocks.Add(p2);
            //    fd.Blocks.Add(p3);
            //    fd.Blocks.Add(a2);
            //    fd.Blocks.Add(p4);
            //    fd.Blocks.Add(a3);
            //    //wm.RTBinstructions.Document = fd;
            //}
        }
    }
}
