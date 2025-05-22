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
                //FlowDocument fd = new FlowDocument();
                //Paragraph p1 = new Paragraph();
                TextBlock p1 = new TextBlock();
                //ScrollViewer scrollViewer = new ScrollViewer();
                //scrollViewer.VerticalScrollBarVisibility = "Visible";
                p1.Inlines.Add(new Run(
                    "Clickless Mouse makes using a computer easier for people with repetitive strain injury, carpal tunnel syndrome, some motor disabilities and other health problems.\n\n"));
                p1.Inlines.Add(new Run(
                    "This application allows using a mouse without clicking - by moving it only. By reacting to user mouse movements this program simulates left/right mouse button click, double left mouse button click and left/right mouse button holding.\n\n"));
                p1.Inlines.Add(new Run("Clickless Mouse can be used with a virtual keyboard to type by moving the mouse (e.g. Free Virtual Keyboard: "));

                //Hyperlink link = new Hyperlink();
                //link.IsEnabled = true;
                //link.Inlines.Add("https://freevirtualkeyboard.com/");
                //link.NavigateUri = new Uri("https://freevirtualkeyboard.com/");
                //link.RequestNavigate += (sender, args) => System.Diagnostics.Process.Start(args.Uri.ToString());
                //p1.Inlines.Add(link);

                p1.Inlines.Add(new Run(").\n\nBy using Clickless Mouse with a virtual keyboard a user can fully control a computer by moving a mouse. \n\nWhen you want to click/hold a mouse button: stop moving the mouse, wait for the squares to appear and move the mouse cursor to the chosen square:\n\n"));

                p1.Inlines.Add(new Run {
                        Text = "•  Top center square = double left mouse button click\n",
                        Classes = { "bullet" }
                });
                p1.Inlines.Add(new Run
                {
                    Text = "•  Top left square = left mouse button click\n",
                    Classes = { "bullet" }
                });
                p1.Inlines.Add(new Run
                {
                    Text = "•  Top right square = right mouse button click\n",
                    Classes = { "bullet" }
                });
                p1.Inlines.Add(new Run
                {
                    Text = "•  Left square = left mouse button holding on/off\n",
                    Classes = { "bullet" }

                });
                p1.Inlines.Add(new Run{
                        Text = "•  Right square = right mouse button holding on/off",
                        Classes = { "bullet" }
                });

                p1.Inlines.Add(new LineBreak());

                p1.Inlines.Add(new Image
                {
                    Source = new Bitmap(AssetLoader.Open(new Uri("avares://ClicklessMouse/Assets/1en.jpg")))
                }
);

                p1.Inlines.Add(new Run(
                    "\nWhen the mouse cursor is located inside a square long enough (depending on cursor time in square to register click setting), it automatically moves back to the previous position to simulate an action based on the square that was previously entered.\n\n"));
                p1.Inlines.Add(new Run(
                    "If the mouse cursor is too close to the top edge of the screen, squares for LMB click, RMB click, and LMB double click are shown below the mouse cursor.\n\n"));
                p1.Inlines.Add(new Run(
                    "If the mouse cursor is too close to the left/right edge of the screen all squares size decrease so at least 25% of a square is visible. By default decreased square size cannot be lower than 60% of the normal size.\nSquares size doesn't decrease if the mouse cursor is so close to the edge of the screen that the smallest size isn't enough to show a square.\n\n"));
                p1.Inlines.Add(new Run("The squares that aren't needed can be disabled.\n\n"));
                p1.Inlines.Add(new Run("Screen panning - when this mode is on, moving the mouse cursor to the edges of the screen presses keys: up at top edge, down at bottom edge, left at left edge, right at right edge. While screen panning is enabled the squares don't show when the mouse cursor is located at the  screen edge.\n\n"));
                p1.Inlines.Add(new Run("Clickless Mouse works only in programs and games that are run in borderless or windowed mode (fullscreen mode is not supported).\n\n"));
                Bold b = new Bold();
                b.Inlines.Add(new Run("First steps:\n\n"));
                p1.Inlines.Add(b);
                p1.Inlines.Add(new Run("1.  Type your diagonal screen size and press 'Set recommended square size button'.\n"));
                p1.Inlines.Add(new Run("2.  Decide which mouse functionality you need. Most users need 'LMB Click', 'RMB click', 'LMB click', 'LMB double click', and 'LMB holding'.\n"));
                p1.Inlines.Add(new Run("3.  If you have a motor disability consider increasing cursor idle time before squares appear, time to start mouse movement after squares appear, and square size.\n\n"));
                b = new Bold();
                b.Inlines.Add(new Run("Lowest possible values (the program ignores lower values and uses following values instead):\n\n"));
                p1.Inlines.Add(b);
                p1.Inlines.Add(new Run("Cursor idle time before squares appear [ms]: " + lowest_cursor_idle_time_ms));
                p1.Inlines.Add(new LineBreak());
                p1.Inlines.Add(new Run("Time to start mouse movement after squares appear [ms]: " + lowest_time_to_start_mouse_movement_ms));
                p1.Inlines.Add(new LineBreak());
                p1.Inlines.Add(new Run("Cursor time in square to register click [ms]: " + lowest_cursor_time_in_square_ms));
                p1.Inlines.Add(new LineBreak());
                p1.Inlines.Add(new Run("Size [px]: " + lowest_size));
                p1.Inlines.Add(new LineBreak());
                p1.Inlines.Add(new Run("Border width [px]: " + lowest_border_width));
                p1.Inlines.Add(new LineBreak());
                p1.Inlines.Add(new Run("Minimum size [%]: " + lowest_min_square_size_percents));

                p1.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
                p1.LineSpacing = 4;
                wm.RTBinstructions.Content = p1;
                //wm.RTBinstructions.Children.Add(new InlineUIContainer
                //{
                //    Child = new Image
                //    {
                //        Source = new Bitmap(AssetLoader.Open(new Uri("avares://ClicklessMouse/Assets/1en.jpg")))
                //    }
                //});
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
