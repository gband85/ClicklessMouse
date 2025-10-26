using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System;
using System.Configuration;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using HarfBuzzSharp;
using System.Windows;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;
using Window = Avalonia.Controls.Window;
using System.Windows.Input;
using WindowState = Avalonia.Controls.WindowState;
using Avalonia.Input;
using Avalonia;
using Avalonia.LogicalTree;
using Egorozh.ColorPicker.Dialog;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using WindowsInput;
using WindowsInput.Native;
using Color = Avalonia.Media.Color;

namespace ClicklessMouse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Main config
        bool SL_enabled;
        bool SR_enabled;
        bool SM_enabled;
        bool SLH_enabled;
        bool SRH_enabled;
        bool screen_panning;

        //Square config
        const int default_cursor_idle_time_ms = 200;
        const int lowest_cursor_idle_time_ms = 100;
        const int additional_cursor_idle_time = 300; //after click is performed it gives user more
                                                     //time to start moving mouse without squares blocking top movement path
        const int default_cursor_time_in_square_ms = 100;
        const int lowest_cursor_time_in_square_ms = 10;
        const int default_time_to_start_mouse_movement_ms = 700;
        const int lowest_time_to_start_mouse_movement_ms = 300;
        const int default_size = 50;
        const int lowest_size = 10;
        const int default_border_width = 2;
        const int lowest_border_width = 1;
        const uint default_color1_uint = 4278190335;
        const uint default_color2_uint = 4294967040;
        const int default_min_square_size_percents = 60;
        const int lowest_min_square_size_percents = 10;
        const int loop_time_ms = 10; //how often is cursor position checked (10ms recommended)
                                     //changing this requires changing default and lowest:
                                     //cursor_idle_time_ms, time_to_start_mouse_movement_ms
                                     //and cursor_time_in_square_ms

        int cursor_idle_time_ms; //idle time before squares appear
        int loops_to_show_squares_after_cursor_idle;
        int time_to_start_mouse_movement_ms; //time to start mouse movement after squares appear, 
                                             //before they disappear (700 default, lowest reasonable 500)
        int loops_to_start_mouse_movement;
        int cursor_time_in_square_ms; //cursor hover time in square needed to perform a click
        int size;
        int border_width;
        Avalonia.Media.Color color1 = Avalonia.Media.Color.FromUInt32(default_color1_uint); //square color 1
        Avalonia.Media.Color color2 = Avalonia.Media.Color.FromUInt32(default_color2_uint); //square color 2
        uint square_color1_uint;
        uint square_color2_uint;

        int min_square_size_percents = default_min_square_size_percents; //how much square size can
                                                                         //be decreased if it would be covered by left or right screen edge
                                                                         //----------------------------------

        const string prog_name = "Clickless Mouse";
        const string prog_version = "3.0";
        const string url_latest_version = "https://raw.githubusercontent.com/gband85/Clickless-Mouse/AvaloniaUI/other/latest_version.txt";
        const string url_homepage = "github.com/gband85/Clickless-Mouse";
        string latest_version = "";
        const string copyright_text = "Copyright © 2025 Garrett Anderson. All rights reserved.";
        string settings_filename = "appsettings.json";

        Square SL, SR, SM, SLH, SRH;
        DateTime last_click_time;
        CancellationTokenSource cts1, cts2;
        Thread THRmouse_monitor, THRsquares_monitor, THRmouse_monitor2;
        int displacement = 0;

        bool saving_enabled = false;
        //full path is necessary if run at startup is used (running at startup uses different current
        //directory
        private string app_folder_path =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), prog_name);

        UILanguage lang = UILanguage.en;
        bool loading_error = false;
        Process prc;

        //NotifyIcon ni = new NotifyIcon();

        InputSimulator sim = new InputSimulator();

        WindowManual wm = new WindowManual();

        public L10nResourceMgr L10nResourceMgr 
            => L10nResourceMgr.Instance;

        public MainWindow()
        {
            is_program_already_running();

            // prc = Process.GetCurrentProcess();
            // prc.PriorityClass = ProcessPriorityClass.High;
            // Thread.CurrentThread.Priority = ThreadPriority.Highest;

            InitializeComponent();
            DataContext = this;

            //Stream iconStream = System.Windows.Application.GetResourceStream(
            //    new Uri("pack://application:,,,/ClicklessMouse;component/clickless_mouse.ico")).Stream;
            //ni.Icon = new System.Drawing.Icon(iconStream);
            //iconStream.Close();
            //ni.MouseClick += new System.Windows.Forms.MouseEventHandler(ni_MouseClick);

            Wmain.Title = prog_name + " " + prog_version;

            restore_default_settings();

            //load_settings();

            fix_wrong_values();

            // saving_enabled = true;

            regenerate_squares();

            if (loading_error)
            {
                save_settings(); //save settings so loading error won't happen again (default values
                                 //will take place of unread values)
            }

            change_language(lang);

            if (CHBstart_minimized.IsChecked == true)
            {
                this.WindowState = WindowState.Minimized;

                if (CHBminimize_to_tray.IsChecked == true)
                {
                    this.Hide();
                    //ni.Visible = true;
                }
            }

            THRmouse_monitor = new Thread(new ThreadStart(monitor_mouse));
            THRmouse_monitor.Priority = ThreadPriority.Highest;
            THRmouse_monitor.Start();
        }

        async void is_program_already_running()
        {
            Process[] arr = Process.GetProcesses();
            string[] a;
            int i = 0;

            foreach (Process p in arr)
            {
                if (p.ProcessName == prog_name)
                {
                    i++;
                }
            }

            if (i > 1)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), prog_name + "is already running.",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowWindowAsync();
                Process.GetCurrentProcess().Kill();
                var box=MessageBoxManager.GetMessageBoxStandard("Error", prog_name + "is already running.", ButtonEnum.Ok,MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowWindowAsync();
            }
        }

        void fix_wrong_values()
        {
            if (cursor_idle_time_ms < lowest_cursor_idle_time_ms)
            {
                TBcursor_idle_before_squares_appear.Text = lowest_cursor_idle_time_ms.ToString();
                cursor_idle_time_ms = lowest_cursor_idle_time_ms;
            }

            if (time_to_start_mouse_movement_ms < lowest_time_to_start_mouse_movement_ms)
            {
                TBtime_to_start_mouse.Text = lowest_time_to_start_mouse_movement_ms.ToString();
                time_to_start_mouse_movement_ms = lowest_time_to_start_mouse_movement_ms;
            }

            if (cursor_time_in_square_ms < lowest_cursor_time_in_square_ms)
            {
                TBcursor_time_in_square.Text = lowest_cursor_time_in_square_ms.ToString();
                cursor_time_in_square_ms = lowest_cursor_time_in_square_ms;
            }
            if (size < lowest_size)
            {
                TBsquare_size.Text = lowest_size.ToString();
                size = lowest_size;
            }

            if (border_width < lowest_border_width)
            {
                TBsquare_border.Text = lowest_border_width.ToString();
                border_width = lowest_border_width;
            }

            if (min_square_size_percents < lowest_min_square_size_percents)
            {
                TBmin_square_size.Text = lowest_min_square_size_percents.ToString();
                min_square_size_percents = lowest_min_square_size_percents;
            }

            else if (min_square_size_percents > 100)
            {

                TBmin_square_size.Text = "100";
                min_square_size_percents = 100;
            }
        }

        void restore_default_settings()
        {
            CHBLMB.IsChecked = false;
            CHBRMB.IsChecked = false;
            CHBdoubleLMB.IsChecked = false;
            CHBholdLMB.IsChecked = false;
            CHBholdRMB.IsChecked = false;
            CHBscreen_panning.IsChecked = false;

            //Checkboxes Checked and Unchecked events work only after form is loaded
            //so they have to be called manually in order to restore settings before form is loaded
            CHBLMB_CheckedChanged(null, null);
            CHBRMB_CheckedChanged(null, null);
            CHBdoubleLMB_CheckedChanged(null, null);
            CHBholdLMB_CheckedChanged(null, null);
            CHBholdRMB_CheckedChanged(null, null);
            CHBscreen_panning_CheckedChanged(null, null);
            CHBcheck_for_updates_CheckedChanged(null, null);

            TBcursor_idle_before_squares_appear.Text = default_cursor_idle_time_ms.ToString();
            cursor_idle_time_ms = default_cursor_idle_time_ms;

            TBtime_to_start_mouse.Text = default_time_to_start_mouse_movement_ms.ToString();
            time_to_start_mouse_movement_ms = default_time_to_start_mouse_movement_ms;

            TBcursor_time_in_square.Text = default_cursor_time_in_square_ms.ToString();
            cursor_time_in_square_ms = default_cursor_time_in_square_ms;

            CHBrun_at_startup.IsChecked = false;
            CHBstart_minimized.IsChecked = false;
            CHBminimize_to_tray.IsChecked = false;
            CHBcheck_for_updates.IsChecked = false;

            TBsquare_size.Text = default_size.ToString();
            size = default_size;

            TBsquare_border.Text = default_border_width.ToString();
            border_width=default_border_width;

            square_color1_uint = default_color1_uint;
            square_color2_uint = default_color2_uint;

            Bsquare_color1.Background = new SolidColorBrush(Color.FromUInt32(square_color1_uint));
            color1 = Avalonia.Media.Color.FromUInt32(square_color1_uint);

            Bsquare_color2.Background = new SolidColorBrush(Color.FromUInt32(square_color2_uint));
            color2 = Avalonia.Media.Color.FromUInt32(square_color2_uint);

            TBmin_square_size.Text = default_min_square_size_percents.ToString();

            TBscreen_size.Text = "";
            int x = Screens.Primary.Bounds.Width;
            int y = Screens.Primary.Bounds.Height;

            TBscreen_resolution.Text = x + "x" + y;
        }

        void regenerate_squares()
        {
            regenerate_SL();
            regenerate_SR();
            regenerate_SM();
            regenerate_SLH();
            regenerate_SRH();
        }

        public int x = 0, y = 0;
        int max_x;
        int max_y;
        public bool squares_visible = false;
        int show_zone;
        int SL_start_x;
        int SL_start_y;
        int SL_end_x;
        int SL_end_y;
        int SR_start_x;
        int SR_start_y;
        int SR_end_x;
        int SR_end_y;
        int SM_start_x;
        int SM_start_y;
        int SM_end_x;
        int SM_end_y;
        int SLH_start_x;
        int SLH_start_y;
        int SLH_end_x;
        int SLH_end_y;
        int SRH_start_x;
        int SRH_start_y;
        int SRH_end_x;
        int SRH_end_y;

        void monitor_mouse2(CancellationToken token)
        {
            //Thread.Sleep(5000); //debug only

            int x1, y1;
            bool pressed_up, pressed_left, pressed_down, pressed_right;
            pressed_up = pressed_left = pressed_down = pressed_right = false;
            System.Drawing.Point one;
            int[] MouseCoords;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                //user may change screen resolution so max_x and max_y should be updated
                max_x = Screens.Primary.Bounds.Width - 1;
                max_y = Screens.Primary.Bounds.Height - 1;
MouseCoords= GetCursorPosition();
                    x1 = MouseCoords[0];
                y1 = MouseCoords[1];

                if (x1 == 0) //if (x1 == 0 && pressed_left == false) would be a mistake (we need
                             //continous pressing as well as holding)
                {
                    key_down(VirtualKeyCode.LEFT);
                    pressed_left = true;
                }
                else if (x1 != 0 && pressed_left == true)
                {
                    key_up(VirtualKeyCode.LEFT);
                    pressed_left = false;
                }

                if (x1 == max_x)
                {
                    key_down(VirtualKeyCode.RIGHT);
                    pressed_right = true;
                }
                else if (x1 != max_x && pressed_right == true)
                {
                    key_up(VirtualKeyCode.RIGHT);
                    pressed_right = false;
                }

                if (y1 == 0)
                {
                    key_down(VirtualKeyCode.UP);
                    pressed_up = true;
                }
                else if (y1 != 0 && pressed_up == true)
                {
                    key_up(VirtualKeyCode.UP);
                    pressed_up = false;
                }

                if (y1 == max_y)
                {
                    key_down(VirtualKeyCode.DOWN);
                    pressed_down = true;
                }
                else if (y1 != max_y && pressed_down == true)
                {
                    key_down(VirtualKeyCode.DOWN);
                    pressed_down = false;
                }

                real_sleep(50);
            }
        }

        void calculate_squares_start_positions()
        {
            displacement = (int)(size / 2);
            show_zone = size + displacement;

            SL_start_x = x - show_zone;
            SL_start_y = y - show_zone;
            SL_end_x = SL_start_x + size;
            SL_end_y = SL_start_y + size;

            SR_start_x = x + displacement;
            SR_start_y = y - show_zone;
            SR_end_x = SR_start_x + size;
            SR_end_y = SR_start_y + size;

            SM_start_x = x - displacement;
            SM_start_y = y - 2 * size;
            SM_end_x = SM_start_x + size;
            SM_end_y = SM_start_y + size;

            SLH_start_x = x - 2 * size;
            SLH_start_y = y - displacement;
            SLH_end_x = SLH_start_x + size;
            SLH_end_y = SLH_start_y + size;

            SRH_start_x = x + size;
            SRH_start_y = y - displacement;
            SRH_end_x = SRH_start_x + size;
            SRH_end_y = SRH_start_y + size;
        }

        int banned_x = -1;
        int banned_y = -1;
        int previous_size = 0;
        void monitor_mouse()
        {
            int i = 0;
            int x1 = 0, x2 = 0, y1 = 0, y2 = 0;
            int[] MouseCoords;

            while (true)
            {
              MouseCoords = GetCursorPosition();
                    x1 = MouseCoords[0];
                y1 = MouseCoords[1];
                    Thread.Sleep(loop_time_ms);
                MouseCoords = GetCursorPosition();
                    x2 = MouseCoords[0];
                    y2 = MouseCoords[1];
             

                //max_x and max_y are updated in monitor_mouse2 by THRmouse_monitor2 which works
                //only when screen_panning == true
                if (screen_panning && (x2 == 0 || x2 == max_x || y2 == 0 || y2 == max_y))
                {
                    banned_x = -1;
                    banned_y = -1;
                    i = 0;
                    mouse_move_detected();
                }
                else if (x1 == x2 && y1 == y2)
                {
                    if (x1 != banned_x || y1 != banned_y)
                    {
                        i++;
                    }
                }
                else
                {
                    banned_x = -1;
                    banned_y = -1;
                    i = 0;
                    mouse_move_detected();
                }
                if (i > loops_to_show_squares_after_cursor_idle && squares_visible == false)
                {
                    x = x2;
                    y = y2;

                    TimeSpan time_elapsed_since_last_click = new TimeSpan();
                    if (last_click_time != null)
                    {
                        time_elapsed_since_last_click = DateTime.Now - last_click_time;
                    }

                    if (last_click_time != null
                        && time_elapsed_since_last_click.TotalMilliseconds >
                        cursor_idle_time_ms + additional_cursor_idle_time)
                    {
                        int original_size = size;
                        int minimum_size =
                            (int)Math.Round((double)original_size * min_square_size_percents / 100);

                        calculate_squares_start_positions();

                        int screen_width = Screens.Primary.Bounds.Width;

                        //if SLH is visible when at minimum_size and 80% or more of SLH 
                        //is out of left screen edge
                        if (SLH_enabled && x > minimum_size && SLH_start_x <= -1 * size * 0.8)
                        {
                            //decrease square size so at least 25% is visible, but square size >= minimum_size
                            size = (int)(x / 1.25);

                            if (size < minimum_size)
                            {
                                size = minimum_size;
                            }
                        }
                        //if SL is visible when at minimum_size and 80% or more of SL 
                        //is out of left screen edge
                        else if (SL_enabled && x > minimum_size / 2 && SL_start_x <= -1 * size * 0.8)
                        {
                            //decrease square size so at least 25% is visible, but square size >= minimum_size
                            size = (int)(x / 0.75);

                            if (size < minimum_size)
                            {
                                size = minimum_size;
                            }
                        }
                        //if SRH is visible when at minimum_size and 80% or more of SRH 
                        //is out of left screen edge
                        else if (SRH_enabled && x < (screen_width - 1) - minimum_size
                            && SRH_start_x >= (screen_width - 1) - size * 0.2)
                        {
                            //decrease square size so at least 25% is visible, but square size >= minimum_size
                            size = (int)(((screen_width - 1) - x) / 1.25);

                            if (size < minimum_size)
                            {
                                size = minimum_size;
                            }
                        }
                        //if SR is visible when at minimum_size and 80% or more of SR
                        //is out of left screen edge
                        else if (SR_enabled && x < (screen_width - 1) - 0.5 * minimum_size
                            && SR_start_x >= (screen_width - 1) - size * 0.2)
                        {
                            //decrease square size so at least 25% is visible, but square size >= minimum_size
                            size = (int)(((screen_width - 1) - x) / 0.75);

                            if (size < minimum_size)
                            {
                                size = minimum_size;
                            }
                        }

                        if (original_size != size)
                        {
                            calculate_squares_start_positions();
                            regenerate_squares();
                        }
                        else if (previous_size != size)
                        {
                            regenerate_squares();
                        }

                        //if top screen edge would cover squares show them below mouse cursor instead
                        if (SM_enabled && SM_start_y < -1 * size * 0.75 || (SM_enabled == false
                            && (SL_enabled || SR_enabled) && SL_start_y < -1 * size * 0.75))
                        {
                            SL_start_y = y + displacement;
                            SL_end_y = SL_start_y + size;

                            SR_start_y = y + displacement;
                            SR_end_y = SR_start_y + size;

                            SM_start_y = y + size;
                            SM_end_y = SM_start_y + size;
                        }

                        bool mi_file_open = false;
                        bool mi_restore_open = false;
                        bool mi_language_open = false;
                        bool mi_help_open = false;
                        bool is_this_focused = false;
                        bool is_instructions_focused = false;

                        Dispatcher.UIThread.Invoke(
                            new Action(() => { mi_file_open = MIfile.IsSubMenuOpen; }));
                        Dispatcher.UIThread.Invoke(
                            new Action(() => { mi_restore_open = MIrestore.IsSubMenuOpen; }));
                        Dispatcher.UIThread.Invoke(
                            new Action(() => { mi_language_open = MIlanguage.IsSubMenuOpen; }));
                        Dispatcher.UIThread.Invoke(
                            new Action(() => { mi_help_open = MIhelp.IsSubMenuOpen; }));

                        Dispatcher.UIThread.Invoke(
                            new Action(() => { is_this_focused = this.IsActive; }));
                        Dispatcher.UIThread.Invoke(
                            new Action(() => { is_instructions_focused = wm.IsActive; }));

                        if (SL_enabled)
                            show_SL(true);
                        if (SR_enabled)
                            show_SR(true);
                        if (SM_enabled)
                            show_SM(true);
                        if (SLH_enabled)
                            show_SLH(true);
                        if (SRH_enabled)
                            show_SRH(true);

                        squares_visible = true;

                        //reopen submenu that was closed because squares appeared
                        if (mi_file_open)
                            Dispatcher.UIThread.Invoke(
                            new Action(() => { MIfile.IsSubMenuOpen = mi_file_open; }));
                        if (mi_restore_open)
                            Dispatcher.UIThread.Invoke(
                            new Action(() => { MIrestore.IsSubMenuOpen = mi_restore_open; }));
                        if (mi_language_open)
                            Dispatcher.UIThread.Invoke(
                            new Action(() => { MIlanguage.IsSubMenuOpen = mi_language_open; }));
                        if (mi_help_open)
                            Dispatcher.UIThread.Invoke(
                            new Action(() => { MIhelp.IsSubMenuOpen = mi_help_open; }));

                        //give back stolen focus (by squares) to a Window if it
                        //was focused before they appeared
                        if (is_this_focused)
                        {
                            Dispatcher.UIThread.Invoke(
                                new Action(() => { this.Focus(); }));
                        }
                        else if (is_instructions_focused)
                        {
                            Dispatcher.UIThread.Invoke(
                                new Action(() => { wm.Focus(); }));
                        }
                        cts1 = new CancellationTokenSource();
                        THRsquares_monitor = new Thread(() => monitor_squares(cts1.Token));
                        THRsquares_monitor.Priority = ThreadPriority.Highest;
                        THRsquares_monitor.Start();
                        i = 0;

                        previous_size = size;
                        size = original_size;
                    }
                }
                else if (i > loops_to_start_mouse_movement && squares_visible)
                {
                    cts1.Cancel();
                    cts1.Dispose();
                    squares_visible = false;
                    if (SL_enabled)
                        show_SL(false);
                    if (SR_enabled)
                        show_SR(false);
                    if (SM_enabled)
                        show_SM(false);
                    if (SLH_enabled)
                        show_SLH(false);
                    if (SRH_enabled)
                        show_SRH(false);
                    i = 0;
                    banned_x = x;
                    banned_y = y;
                }
            }
        }

        void monitor_squares(CancellationToken token)
        {
            int i_SL = 0, i_SR = 0, i_SM = 0, i_SLH = 0, i_SRH = 0;
            int i_max = cursor_time_in_square_ms / loop_time_ms;
            int pos_x, pos_y;
            int[] MouseCoords;

            while (i_SL < i_max && i_SR < i_max && i_SM < i_max
                && i_SLH < i_max && i_SRH < i_max && squares_visible)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                MouseCoords = GetCursorPosition();
                pos_x = MouseCoords[0];
                    pos_y = MouseCoords[1];
                
                if (SL_enabled)
                {
                    if (is_cursor_in_SL(pos_x, pos_y))
                    {
                        i_SL++;
                    }
                    else i_SL = 0;
                }
                if (SR_enabled)
                {
                    if (is_cursor_in_SR(pos_x, pos_y))
                    {
                        i_SR++;
                    }
                    else i_SR = 0;
                }
                if (SM_enabled)
                {
                    if (is_cursor_in_SM(pos_x, pos_y))
                    {
                        i_SM++;
                    }
                    else i_SM = 0;
                }
                if (SLH_enabled)
                {
                    if (is_cursor_in_SLH(pos_x, pos_y))
                    {
                        i_SLH++;
                    }
                    else i_SLH = 0;
                }
                if (SRH_enabled)
                {
                    if (is_cursor_in_SRH(pos_x, pos_y))
                    {
                        i_SRH++;
                    }
                    else i_SRH = 0;
                }
                Thread.Sleep(loop_time_ms);
            }
            if (i_SL >= i_max)
            {
                LMBClick(x, y, 100);
                if (SL_enabled)
                    show_SL(false);
                if (SR_enabled)
                    show_SR(false);
                if (SM_enabled)
                    show_SM(false);
                if (SLH_enabled)
                    show_SLH(false);
                if (SRH_enabled)
                    show_SRH(false);
                last_click_time = DateTime.Now;
                squares_visible = false;
            }
            else if (i_SR >= i_max)
            {
                RMBClick(x, y, 100);
                if (SL_enabled)
                    show_SL(false);
                if (SR_enabled)
                    show_SR(false);
                if (SM_enabled)
                    show_SM(false);
                if (SLH_enabled)
                    show_SLH(false);
                if (SRH_enabled)
                    show_SRH(false);
                last_click_time = DateTime.Now;
                squares_visible = false;
            }
            else if (i_SM >= i_max)
            {
                DLMBClick(x, y, 100);
                if (SL_enabled)
                    show_SL(false);
                if (SR_enabled)
                    show_SR(false);
                if (SM_enabled)
                    show_SM(false);
                if (SLH_enabled)
                    show_SLH(false);
                if (SRH_enabled)
                    show_SRH(false);
                last_click_time = DateTime.Now;
                squares_visible = false;
            }
            if (i_SLH >= i_max)
            {
                LMBHold(x, y, 100);
                if (SL_enabled)
                    show_SL(false);
                if (SR_enabled)
                    show_SR(false);
                if (SM_enabled)
                    show_SM(false);
                if (SLH_enabled)
                    show_SLH(false);
                if (SRH_enabled)
                    show_SRH(false);
                last_click_time = DateTime.Now;
                squares_visible = false;
            }
            else if (i_SRH >= i_max)
            {
                RMBHold(x, y, 100);
                if (SL_enabled)
                    show_SL(false);
                if (SR_enabled)
                    show_SR(false);
                if (SM_enabled)
                    show_SM(false);
                if (SLH_enabled)
                    show_SLH(false);
                if (SRH_enabled)
                    show_SRH(false);
                last_click_time = DateTime.Now;
                squares_visible = false;
            }
        }


        private void mouse_move_detected()
        {
            if (squares_visible)
            {
                int x1, y1;
                int[] MouseCoords;

                MouseCoords = GetCursorPosition();
                x1 = MouseCoords[0];
                y1 = MouseCoords[1];

                if (is_cursor_outside_zone(x1, y1))
                {
                    if (SL_enabled)
                        show_SL(false);
                    if (SR_enabled)
                        show_SR(false);
                    if (SM_enabled)
                        show_SM(false);
                    if (SLH_enabled)
                        show_SLH(false);
                    if (SRH_enabled)
                        show_SRH(false);
                    squares_visible = false;
                }
            }
        }

        bool is_cursor_in_SL(int x1, int y1)
        {
            if (x1 >= SL_start_x && x1 <= SL_end_x
                && y1 >= SL_start_y && y1 <= SL_end_y)
            {
                return true;
            }
            else return false;
        }

        bool is_cursor_in_SR(int x1, int y1)
        {
            if (x1 >= SR_start_x && x1 <= SR_end_x
                && y1 >= SR_start_y && y1 <= SR_end_y)
            {
                return true;
            }
            else return false;
        }

        bool is_cursor_in_SM(int x1, int y1)
        {
            if (x1 >= SM_start_x && x1 <= SM_end_x
                && y1 >= SM_start_y && y1 <= SM_end_y)
            {
                return true;
            }
            else return false;
        }

        bool is_cursor_in_SLH(int x1, int y1)
        {
            if (x1 >= SLH_start_x && x1 <= SLH_end_x
                && y1 >= SLH_start_y && y1 <= SLH_end_y)
            {
                return true;
            }
            else return false;
        }

        bool is_cursor_in_SRH(int x1, int y1)
        {
            if (x1 >= SRH_start_x && x1 <= SRH_end_x
                && y1 >= SRH_start_y && y1 <= SRH_end_y)
            {
                return true;
            }
            else return false;
        }

        bool is_cursor_outside_zone(int x1, int y1)
        {
            //if (SM_enabled == false)
            //{
            //    if (x1 > x + show_zone || x1 < x - show_zone || y1 > y + show_zone || y1 < y - show_zone)
            //    {
            //        return true;
            //    }
            //    else return false;
            //}
            //else
            //{
            //    if (x1 > x + show_zone || x1 < x - show_zone || y1 > y + show_zone || y1 < y - show_zone - (int)(size / 2))
            //    {
            //        return true;
            //    }
            //    else return false;
            //}

            //temp solution
            if (x1 > x + show_zone + displacement || x1 < x - show_zone - displacement
                || y1 > y + show_zone || y1 < y - show_zone - displacement)
            {
                return true;
            }
            else return false;
        }

        void real_sleep(int time)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            do
            {
                Thread.Sleep(10);
            }
            while (stopwatch.ElapsedMilliseconds < time);
            stopwatch.Stop();
        }

        delegate void Callback1(bool show);

        void show_SL(bool show)
        {
            if (!SL.CheckAccess())
            {
                try
                {
                    Dispatcher.UIThread.Invoke(new Action(() => show_SL(show)));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (show)
                {
                        SL.Position = new PixelPoint(SL_start_x, SL_start_y);
                        SL.Show();
                }
                else SL.Hide();
            }
        }
        void show_SR(bool show)
        {
            if (!SR.CheckAccess())
            {
                try
                {
                    Dispatcher.UIThread.Invoke(new Action(() => show_SR(show)));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (show)
                {
                    SR.Position = new PixelPoint(SR_start_x, SR_start_y);
                    SR.Show();
                }
                else SR.Hide();
            }
        }
        void show_SM(bool show)
        {
            if (!SM.CheckAccess())
            {
                try
                {
                    Dispatcher.UIThread.Invoke(new Action(() => show_SM(show)));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (show)
                {
                    SM.Position = new PixelPoint(SM_start_x, SM_start_y);
                    SM.Show();
                }
                else SM.Hide();
            }
        }
        void show_SLH(bool show)
        {
            if (!SLH.CheckAccess())
            {
                try
                {
                    Dispatcher.UIThread.Invoke(new Action(() => show_SLH(show)));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (show)
                {
                    SLH.Position = new PixelPoint(SLH_start_x, SLH_start_y);
                    SLH.Show();
                }
                else SLH.Hide();
            }
        }
        void show_SRH(bool show)
        {
            if (!SRH.CheckAccess())
            {
                try
                {
                    Dispatcher.UIThread.Invoke(new Action(() => show_SRH(show)));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (show)
                {
                    SRH.Position = new PixelPoint(SRH_start_x, SRH_start_y);
                    SRH.Show();
                }
                else SRH.Hide();
            }
        }

        delegate void Callback2();

        void regenerate_SL()
        {
            if (SL != null && !SL.CheckAccess())
            {
                try
                {
                    Dispatcher.UIThread.Invoke(new Action(() => regenerate_SL()));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (SL != null)
                    SL.Close();

                    SL = new Square(size, border_width, color1, color2);
                    SL.Topmost = true;
                    SL.Show();
                    SL.Height = size;
                    SL.Width = size;

                    SL.Hide();
            }
        }

        void regenerate_SR()
        {
            if (SR != null && !SR.CheckAccess())
            {
                try
                {
                    Dispatcher.UIThread.Invoke(new Action(() => regenerate_SR()));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (SR != null)
                    SR.Close();

                SR = new Square(size, border_width, color1, color2);
                SR.Topmost = true;
                SR.Show();
                SR.Height = size;
                SR.Width = size;

                SR.Hide();
            }
        }

        void regenerate_SM()
        {
            if (SM != null && !SM.CheckAccess())
            {
                try
                {
                    Dispatcher.UIThread.Invoke(new Action(() => regenerate_SM()));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (SM != null)
                    SM.Close();

                SM = new Square(size, border_width, color1, color2);
                SM.Topmost = true;
                SM.Show();
                SM.Height = size;
                SM.Width = size;

                SM.Hide();
            }
        }

        void regenerate_SLH()
        {
            if (SLH != null && !SLH.CheckAccess())
            {
                try
                {
                    Dispatcher.UIThread.Invoke(new Action(() => regenerate_SLH()));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (SLH != null)
                    SLH.Close();

                SLH = new Square(size, border_width, color1, color2);
                SLH.Topmost = true;
                SLH.Show();
                SLH.Height = size;
                SLH.Width = size;

                SLH.Hide();
            }
        }

        void regenerate_SRH()
        {
            if (SRH != null && !SRH.CheckAccess())
            {
                try
                {
                    Dispatcher.UIThread.Invoke(new Action(() => regenerate_SRH()));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (SRH != null)
                    SRH.Close();

                SRH = new Square(size, border_width, color1, color2);
                SRH.Topmost = true;
                SRH.Show();
                SRH.Height = size;
                SRH.Width = size;

                SRH.Hide();
            }
        }

        //----------------------------------------------------------------------------------

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private void Wmain_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized && CHBminimize_to_tray.IsChecked == true)
            {
                this.Hide();
                //ni.Visible = true;
            }
        }

        void ni_MouseClick(object sender,PointerEventArgs e)
        {
            //ni.Visible = false;
            Show();
            this.WindowState = WindowState.Normal;
            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
        }

        private void Window_Closing(object sender, WindowClosingEventArgs e)
        {
            MIexit_Click(null, null);
        }

        private void MIexit_Click(object sender, RoutedEventArgs e)
        {
            //ni.Visible = false;
            //ni.Dispose();

            release_buttons_and_keys();
            Process.GetCurrentProcess().Kill();
        }

        private void MIdefault_colors_Click(object sender, RoutedEventArgs e)
        {
            saving_enabled = false; //to avoid multiple saves

            square_color1_uint = default_color1_uint;
            square_color2_uint = default_color2_uint;

            Bsquare_color1.Background = new SolidColorBrush(Color.FromUInt32(square_color1_uint));
            color1 = Avalonia.Media.Color.FromUInt32(square_color1_uint);

            Bsquare_color2.Background = new SolidColorBrush(Color.FromUInt32(square_color2_uint));
            color2 = Avalonia.Media.Color.FromUInt32(square_color2_uint);

            regenerate_squares();

            saving_enabled = true;

            save_settings();
        }

        private void MIdefault_settings_Click(object sender, RoutedEventArgs e)
        {
            saving_enabled = false; //to avoid multiple saves

            restore_default_settings();

            regenerate_squares();

            saving_enabled = true;
            save_settings();
        }


        private void MIenglish_Click(object sender, RoutedEventArgs e)
        {
            var switchToCulture = UILanguage.en;
            change_language(switchToCulture);
            MIenglish.IsChecked = true;
            MIpolish.IsChecked = false;
            save_settings();
        }

        private void MIpolish_Click(object sender, RoutedEventArgs e)
        {
            var switchToCulture = UILanguage.pl;
            change_language(switchToCulture);
            MIenglish.IsChecked = false;
            MIpolish.IsChecked = true;
            save_settings();
        }

        private void MImanual_Click(object sender, RoutedEventArgs e)
        {
            wm.Show();
        }

        private async void MIabout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string content;
                MyWebClient wc = new MyWebClient();
                content = wc.DownloadString(url_latest_version);

                latest_version = content.Replace("\r\n", "").Trim();
            }
            catch (WebException we)
            {
                latest_version = "unknown";
            }

            try
            {
                WindowAbout w = new WindowAbout();

                w.Lprogram_name.Content = prog_name;
                w.Llatest_version.Content = "Latest version: " + latest_version;
                w.Linstalled_version.Content = "Installed version: " + prog_version;
                w.HBhomepage.Content = url_homepage;
                w.HBhomepage.NavigateUri = new Uri("http://"+url_homepage);
                w.Lcopyright.Content = copyright_text;

                w.Show();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private void CHBLMB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (saving_enabled)
            {
                show_SL(false);
                show_SR(false);
                show_SM(false);
                show_SLH(false);
                show_SRH(false);
            }

            if (CHBLMB.IsChecked == true)
                SL_enabled = true;
            else
                SL_enabled = false;

            if (saving_enabled)
            {
                save_settings();
            }
        }

        private void CHBRMB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (saving_enabled)
            {
                show_SL(false);
                show_SR(false);
                show_SM(false);
                show_SLH(false);
                show_SRH(false);
            }

            if (CHBRMB.IsChecked == true)
                SR_enabled = true;
            else
                SR_enabled = false;

            if (saving_enabled)
            {
                save_settings();
            }
        }

        private void CHBdoubleLMB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (saving_enabled)
            {
                show_SL(false);
                show_SR(false);
                show_SM(false);
                show_SLH(false);
                show_SRH(false);
            }

            if (CHBdoubleLMB.IsChecked == true)
                SM_enabled = true;
            else
                SM_enabled = false;

            if (saving_enabled)
            {
                save_settings();
            }
        }

        private void CHBholdLMB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (saving_enabled)
            {
                show_SL(false);
                show_SR(false);
                show_SM(false);
                show_SLH(false);
                show_SRH(false);
            }

            if (CHBholdLMB.IsChecked == true)
                SLH_enabled = true;
            else
                SLH_enabled = false;

            if (saving_enabled)
            {
                save_settings();
            }
        }

        private void CHBholdRMB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (saving_enabled)
            {
                show_SL(false);
                show_SR(false);
                show_SM(false);
                show_SLH(false);
                show_SRH(false);
            }

            if (CHBholdRMB.IsChecked == true)
                SRH_enabled = true;
            else
                SRH_enabled = false;

            if (saving_enabled)
            {
                save_settings();
            }
        }

        private void CHBscreen_panning_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CHBscreen_panning.IsChecked == true && THRmouse_monitor2 == null)
            {
                cts2 = new CancellationTokenSource();
                THRmouse_monitor2 = new Thread(() => monitor_mouse2(cts2.Token));
                THRmouse_monitor2.Priority = ThreadPriority.Highest;
                THRmouse_monitor2.Start();
                screen_panning = true;
            }
            else if (CHBscreen_panning.IsChecked == false && THRmouse_monitor2 != null)
            {
                screen_panning = false;
                cts2.Cancel();
                cts2.Dispose();
                THRmouse_monitor2 = null;
            }

            if (saving_enabled)
            {
                save_settings();
            }
        }

        private void CHBcheck_for_updates_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if ((bool)CHBcheck_for_updates.IsChecked)
            {
                update_app_if_necessary();
            }

            if (saving_enabled)
            {
                save_settings();
            }
        }

        private async void TBcursor_idle_before_squares_appear_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TBcursor_idle_before_squares_appear.Text.Length > 0)
                {
                    int x = int.Parse(TBcursor_idle_before_squares_appear.Text);
                    if (x < 1)
                    {
                        TBcursor_idle_before_squares_appear.Text = "1";
                        throw new Exception(cursor_idle_time_error + "1 ms");
                    }

                    cursor_idle_time_ms = x;
                    loops_to_show_squares_after_cursor_idle = (int)Math.Round(
                        (double)(cursor_idle_time_ms / loop_time_ms));

                    if (cursor_idle_time_ms < lowest_cursor_idle_time_ms)
                    {
                        cursor_idle_time_ms = lowest_cursor_idle_time_ms;
                        loops_to_show_squares_after_cursor_idle =
                            (int)Math.Round((double)(lowest_cursor_idle_time_ms / loop_time_ms));
                    }

                    if (saving_enabled)
                    {
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private async void TBtime_to_start_mouse_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TBtime_to_start_mouse.Text.Length > 0)
                {
                    int x = int.Parse(TBtime_to_start_mouse.Text);
                    if (x < 1)
                    {
                        TBtime_to_start_mouse.Text = "1";
                        throw new Exception(time_to_start_mouse_movement_error + "1 ms.");
                    }

                    time_to_start_mouse_movement_ms = x;
                    loops_to_start_mouse_movement = (int)Math.Round(
                        (double)time_to_start_mouse_movement_ms / loop_time_ms);

                    if (time_to_start_mouse_movement_ms < lowest_time_to_start_mouse_movement_ms)
                    {
                        time_to_start_mouse_movement_ms = lowest_time_to_start_mouse_movement_ms;
                        loops_to_start_mouse_movement = (int)Math.Round(
                            (double)(lowest_time_to_start_mouse_movement_ms / loop_time_ms));
                    }

                    if (saving_enabled)
                    {
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private async void TBcursor_time_in_square_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TBcursor_time_in_square.Text.Length > 0)
                {
                    int x = int.Parse(TBcursor_time_in_square.Text);
                    if (x < 1)
                    {
                        TBcursor_time_in_square.Text = "1";
                        throw new Exception(cursor_time_in_square_error + "1 ms.");
                    }

                    cursor_time_in_square_ms = x;

                    if (cursor_time_in_square_ms < lowest_cursor_time_in_square_ms)
                        cursor_time_in_square_ms = lowest_cursor_time_in_square_ms;

                    if (saving_enabled)
                    {
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private void CHBrun_at_startup_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (saving_enabled)
            {
                //need a .bat file to start an .exe file for some reasons
                Microsoft.Win32.RegistryKey rkApp = Microsoft.Win32.Registry.CurrentUser
                    .OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (CHBrun_at_startup.IsChecked == true)
                {
                    if (rkApp.GetValue(prog_name) == null)
                    {
                        rkApp.SetValue(prog_name,
                            System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".exe", ".vbs"));
                    }
                    generate_bat_file();
                }
                else if (rkApp.GetValue(prog_name) != null)
                {
                    rkApp.DeleteValue(prog_name, false);
                }

                save_settings();
            }
        }

        async void  generate_bat_file()
        {
            FileStream fs = null;
            StreamWriter sw = null;
            string file_path = System.IO.Path.Combine(
                System.Reflection.Assembly.GetExecutingAssembly().Location.
                Replace(".exe", ".vbs"));

            try
            {
                fs = new FileStream(file_path, FileMode.Create, FileAccess.Write);
                sw = new StreamWriter(fs);

                //cmd script (black window appearing for a moment is a problem)
                //sw.WriteLine("cd \"" + app_folder_path + "\"");
                //sw.WriteLine("start " 
                //    + System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName);

                //vbs script (no window appearing during execution)
                sw.WriteLine("Set objShell = CreateObject(\"Wscript.Shell\")");
                sw.WriteLine("objShell.CurrentDirectory = \"" + app_folder_path + "\"");
                sw.WriteLine("strApp = \"\"\""
                    + System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName + "\"\"\"");
                sw.WriteLine("objShell.Run(strApp)");

                sw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();

                try
                {
                    if (sw != null)
                        sw.Close();
                    if (fs != null)
                        fs.Close();
                }
                catch (Exception ex2) { }
            }
        }
        private void CHBstart_minimized_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (saving_enabled)
            {
                save_settings();
            }
        }

        private void CHBminimize_to_tray_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (saving_enabled)
            {
                save_settings();
            }
        }

        private async void TBsquare_size_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TBsquare_size.Text.Length > 0)
                {
                    int x = int.Parse(TBsquare_size.Text);
                    if (x < 1)
                    {
                        TBsquare_size.Text = "1";
                        throw new Exception(square_size_error + "1 px.");
                    }

                    size = x;

                    if (size < lowest_size)
                        size = lowest_size;

                    if (saving_enabled)
                    {
                        regenerate_squares();
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private async void TBsquare_border_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TBsquare_border.Text.Length > 0)
                {
                    int x = int.Parse(TBsquare_border.Text);
                    if (x < 1)
                    {
                        TBsquare_border.Text = "1";
                        throw new Exception(square_border_error + "1 px.");
                    }

                    border_width = x;

                    if (saving_enabled)
                    {
                        regenerate_squares();
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private async void Bsquare_color1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ColorPickerDialog colorDialog1 = new ColorPickerDialog() { Color = color1 };
                
                var dr = await colorDialog1.ShowDialog<bool>(this);

                if (dr)
                {
                    if (colorDialog1.Color == null)
                        throw new Exception("No color selected");
                    
                    Bsquare_color1.Background = new SolidColorBrush((colorDialog1.Color));
                    color1 = colorDialog1.Color;

                    square_color1_str = colorDialog1.Color.ToString();

                    regenerate_squares();

                    if (saving_enabled)
                    {
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private async void Bsquare_color2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ColorPickerDialog colorDialog2 = new ColorPickerDialog() { Color = color2 };
                
                var dr = await colorDialog2.ShowDialog<bool>(this);
                
                if (dr)
                {
                    if (colorDialog2.Color == null)
                        throw new Exception("No color selected");
                    
                    Bsquare_color2.Background = new SolidColorBrush(colorDialog2.Color);
                    color2 = colorDialog2.Color;

                    square_color2_uint = colorDialog2.Color.ToUInt32();

                    regenerate_squares();
                    
                    if (saving_enabled)
                    {
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private async void TBmin_square_size_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TBmin_square_size.Text.Length > 0)
                {
                    int x = int.Parse(TBmin_square_size.Text);
                    if (x < 1)
                    {
                        TBmin_square_size.Text = "1";
                        throw new Exception(min_square_size_too_low_error + "1%.");
                    }
                    else if (x > 100)
                    {
                        TBmin_square_size.Text = "100";
                        throw new Exception(min_square_size_too_high_error + "100%.");
                    }

                    min_square_size_percents = x;

                    if (min_square_size_percents < lowest_min_square_size_percents)
                    {
                        min_square_size_percents = lowest_min_square_size_percents;
                    }

                    if (saving_enabled)
                    {
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private async void Click_Bset_recommended_square(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TBscreen_size.Text.Length == 0)
                {
                    throw new Exception(screen_size_error2);
                }
                else
                {
                    int d = int.Parse(TBscreen_size.Text);
                    if (d < 1)
                        throw new Exception(screen_size_error1 + ".");

                    int x = Screens.Primary.Bounds.Width;
                    int y = Screens.Primary.Bounds.Height;

                    TBscreen_resolution.Text = x + "x" + y;

                    double b = Math.Sqrt(Math.Pow(d, 2) / (Math.Pow(x, 2) / Math.Pow(y, 2) + 1));
                    double a = b * x / y;

                    double area = a * b;
                    double pixel_size_mm = area / (x * y) * Math.Pow(25.4, 2);

                    TBsquare_size.Text = Math.Round(50 * 0.0771 / pixel_size_mm).ToString();
                    TBsquare_border.Text = Math.Round(2 * 0.06939 / pixel_size_mm).ToString();

                    regenerate_squares();
                    save_settings();
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private void save_settings()
        {

            foreach (ILogical control in Wmain.GetLogicalDescendants())
            {
                if (control is CheckBox cb)
                    AddUpdateAppSetting(cb.Name, cb.IsChecked.ToString());

                else if (control is TextBox tb)
                {

                    if (tb.Name == TBscreen_size.Name && tb.Text == "")
                        AddUpdateAppSetting(tb.Name, "0");
                    else if (tb.Name == Bsquare_color1.Name)                    
                        AddUpdateAppSetting("square_color1_str", square_color1_str);                    
                    else if (tb.Name == Bsquare_color2.Name)
                        AddUpdateAppSetting("square_color2_str", square_color2_str);
                    else
                        AddUpdateAppSetting(tb.Name, tb.Text);
                }
            }
            AddUpdateAppSetting("lang", lang.ToString());

            Console.WriteLine($"{settings_filename} updated successfully.");
        }

        private async void load_settings()
        {
            string settings_file_path = System.IO.Path.Combine(app_folder_path, settings_filename);
            
            try
            {
                if (File.Exists(settings_file_path))
                {
                    //Checkboxes Checked and Unchecked events work only after form is loaded
                    //so they have to be called manually in order to load save data properly
                    CHBLMB_CheckedChanged(null, null);
                    CHBRMB_CheckedChanged(null, null);
                    CHBdoubleLMB_CheckedChanged(null, null);
                    CHBholdLMB_CheckedChanged(null, null);
                    CHBholdRMB_CheckedChanged(null, null);
                    CHBscreen_panning_CheckedChanged(null, null);
                    CHBcheck_for_updates_CheckedChanged(null, null);

                    foreach (ILogical control in Wmain.GetLogicalDescendants())
                    {
                        if (control is CheckBox cb)
                            cb.IsChecked = bool.Parse(ReadAppSetting(cb.Name));

                        else if (control is TextBox tb)
                        {
                            if (tb.Name == "TBsquare_color1")
                                square_color1_str = ReadAppSetting("square_color1_str");
                            else if (tb.Name == "TBsquare_color2")
                                square_color2_str = ReadAppSetting("square_color2_str");
                            else
                                tb.Text = ReadAppSetting(tb.Name);
                        }
                    }

                    Bsquare_color1.Background = new SolidColorBrush(Color.FromUInt32(square_color1_uint));
                    color1 = Avalonia.Media.Color.FromUInt32(square_color1_uint);

                    Bsquare_color2.Background = new SolidColorBrush(Color.FromUInt32(square_color2_uint));
                    color2 = Avalonia.Media.Color.FromUInt32(square_color2_uint);

                    Enum.TryParse(ReadAppSetting("lang"), out lang);
                }
            }
            catch (Exception ex)
            {
                loading_error = true;
                var box = MessageBoxManager.GetMessageBoxStandard(L10nResourceMgr["error_title"].ToString(), ex.Message + loading_error_msg,
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();

                // try
                // {
                //     if (sr != null)
                //         sr.Close();
                //     if (fs != null)
                //         fs.Close();
                // }
                // catch (Exception ex2) { }
            }
        }

        async void update_app_if_necessary()
        {
            try
            {
                string content;
                MyWebClient wc = new MyWebClient();
                content = wc.DownloadString(url_latest_version);

                latest_version = content.Replace("\r\n", "").Trim();
            }
            catch (WebException we)
            {
                latest_version = "unknown";
            }

            bool update_available = false;

            if (latest_version != "unknown" &&
                int.Parse(latest_version.Replace(".", "")) > int.Parse(prog_version.Replace(".", "")))
            {
                update_available = true;
            }

            if ((bool)CHBcheck_for_updates.IsChecked && update_available)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("New Version Available", "A new program version" +
                    " is available. Do you want to download it now?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                var dialogResult = await box.ShowWindowAsync();
                if (dialogResult == ButtonResult.Yes)
                {
                    //Open download page
                    Process.Start("https://" + url_homepage);
                }
            }
        }

        private void AddUpdateAppSetting(string key, string value)
        {
            try
            {
                string settings_file_path = System.IO.Path.Combine(app_folder_path, settings_filename);

                // Load the JSON file
                string json = File.ReadAllText(settings_file_path);

                // Parse JSON as JsonNode
                JsonNode root = JsonNode.Parse(json);

                if (root == null)
                {
                    Console.WriteLine("Failed to load JSON.");
                    return;
                }

                root[key] = value;

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(settings_file_path, root.ToJsonString(options));
            }
            catch (IOException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }
        private string ReadAppSetting(string key)
        {
            try
            {

                string settings_file_path = System.IO.Path.Combine(app_folder_path, settings_filename);

                // Load the JSON file
                string json = File.ReadAllText(settings_file_path);

                // Parse JSON as JsonNode
                JsonNode root = JsonNode.Parse(json);

                if (root == null)
                {
                    Console.WriteLine("Failed to load JSON.");
                    return "0";
                }

                return root[key].ToString();

            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
                return "[]";
            }
        }


        private class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 3000;
                return w;
            }
        }
    }
}
