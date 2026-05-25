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
using ClicklessMouse.Native;
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
        private bool _slEnabled;
        private bool _srEnabled;
        private bool _smEnabled;
        private bool _slhEnabled;
        private bool _srhEnabled;
        private bool _screenPanning;

        //Square config
        private const int default_cursor_idle_time_ms = 200;
        private const int lowest_cursor_idle_time_ms = 100;

        private const int additional_cursor_idle_time = 300; //after click is performed it gives user more
                                                     //time to start moving mouse without squares blocking top movement path
                                                     private const int default_cursor_time_in_square_ms = 100;
                                                     private const int lowest_cursor_time_in_square_ms = 10;
                                                     private const int default_time_to_start_mouse_movement_ms = 700;
                                                     private const int lowest_time_to_start_mouse_movement_ms = 300;
                                                     private const int default_size = 50;
                                                     private const int lowest_size = 10;
                                                     private const int default_border_width = 2;
                                                     private const int lowest_border_width = 1;
                                                     private const uint default_color1_uint = 4278190335;
                                                     private const uint default_color2_uint = 4294967040;
                                                     private const int default_min_square_size_percents = 60;
                                                     private const int lowest_min_square_size_percents = 10;

                                                     private const int loop_time_ms = 10; //how often is cursor position checked (10ms recommended)
                                     //changing this requires changing default and lowest:
                                     //cursor_idle_time_ms, time_to_start_mouse_movement_ms
                                     //and cursor_time_in_square_ms

                                     private int _cursorIdleTimeMs; //idle time before squares appear
                                     private int _loopsToShowSquaresAfterCursorIdle;

                                     private int _timeToStartMouseMovementMs; //time to start mouse movement after squares appear, 
                                             //before they disappear (700 default, lowest reasonable 500)
                                             private int _loopsToStartMouseMovement;
                                             private int _cursorTimeInSquareMs; //cursor hover time in square needed to perform a click
                                             private int _size;
                                             private int _borderWidth;
                                             private Avalonia.Media.Color _color1 = Avalonia.Media.Color.FromUInt32(default_color1_uint); //square color 1
                                             private Avalonia.Media.Color _color2 = Avalonia.Media.Color.FromUInt32(default_color2_uint); //square color 2
                                             private uint _squareColor1Uint;
                                             private uint _squareColor2Uint;

                                             private int _minSquareSizePercents = default_min_square_size_percents; //how much square size can
                                                                         //be decreased if it would be covered by left or right screen edge
                                                                         //----------------------------------

                                                                         private const string prog_name = "Clickless Mouse";
                                                                         private const string prog_version = "3.0";
                                                                         private const string url_latest_version = "https://raw.githubusercontent.com/gband85/Clickless-Mouse/AvaloniaUI/other/latest_version.txt";
                                                                         private const string url_homepage = "github.com/gband85/Clickless-Mouse";
                                                                         private string _latestVersion = "";
                                                                         private const string copyright_text = "Copyright © 2025 Garrett Anderson. All rights reserved.";
                                                                         private string _settingsFilename = "appsettings.json";
                                                                         private string _defaultSettingsFilename = "defaults.json";
                                                                         private Square _sl, _sr, _sm, _slh, _srh;
                                                                         private DateTime _lastClickTime;
                                                                         private CancellationTokenSource _cts1, _cts2;
                                                                         private Thread _thRmouseMonitor, _thRsquaresMonitor, _thRmouseMonitor2;
                                                                         private int _displacement = 0;

                                                                         private bool _savingEnabled = true;
        //full path is necessary if run at startup is used (running at startup uses different current
        //directory
        private string _appFolderPath = "";

        private string _settingsPath = "";
        private string _defaultSettingsPath = "";

        private UiLanguage _lang = UiLanguage.En;
        private bool _loadingError = false;
        private Process _prc;

        //NotifyIcon ni = new NotifyIcon();

        private InputSimulator _sim = new InputSimulator();

        public L10NResourceMgr L10NResourceMgr
            => L10NResourceMgr.Instance;

        public MainWindow()
        {
            is_program_already_running();

            // prc = Process.GetCurrentProcess();
            // prc.PriorityClass = ProcessPriorityClass.High;
            // Thread.CurrentThread.Priority = ThreadPriority.Highest;

            InitializeComponent();
            DataContext = this;

#if _WINDOWS
           _appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), prog_name); 
           _defaultSettingsPath=Path.Combine(AppContext.BaseDirectory,_defaultSettingsFilename);

#elif _LINUX
            _appFolderPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".config", prog_name.Replace(" ", String.Empty));
            _defaultSettingsPath = Path.Combine("/usr/share", prog_name.Replace(" ", String.Empty), _defaultSettingsFilename);

#endif

            _settingsPath = Path.Combine(_appFolderPath, _settingsFilename);
            //Stream iconStream = System.Windows.Application.GetResourceStream(
            //    new Uri("pack://application:,,,/ClicklessMouse;component/clickless_mouse.ico")).Stream;
            //ni.Icon = new System.Drawing.Icon(iconStream);
            //iconStream.Close();
            //ni.MouseClick += new System.Windows.Forms.MouseEventHandler(ni_MouseClick);

            Wmain.Title = prog_name + " " + prog_version;

            restore_default_settings();

            load_settings();

            fix_wrong_values();

            // regenerate_squares();

            // if (loading_error)
            // {
            //     save_settings(); //save settings so loading error won't happen again (default values
            //                      //will take place of unread values)
            // }

            change_language(_lang);

            if (CHBstart_minimized.IsChecked == true)
            {
                this.WindowState = WindowState.Minimized;

                if (CHBminimize_to_tray.IsChecked == true)
                {
                    this.Hide();
                    //ni.Visible = true;
                }
            }

            _thRmouseMonitor = new Thread(new ThreadStart(monitor_mouse));
            _thRmouseMonitor.Priority = ThreadPriority.Highest;
            _thRmouseMonitor.Start();
        }

        private async void is_program_already_running()
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
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString(), prog_name + "is already running.",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowWindowAsync();
                Process.GetCurrentProcess().Kill();
            }
        }

        private void fix_wrong_values()
        {
            if (_cursorIdleTimeMs < lowest_cursor_idle_time_ms)
            {
                TBcursor_idle_before_squares_appear.Text = lowest_cursor_idle_time_ms.ToString();
                _cursorIdleTimeMs = lowest_cursor_idle_time_ms;
            }

            if (_timeToStartMouseMovementMs < lowest_time_to_start_mouse_movement_ms)
            {
                TBtime_to_start_mouse.Text = lowest_time_to_start_mouse_movement_ms.ToString();
                _timeToStartMouseMovementMs = lowest_time_to_start_mouse_movement_ms;
            }

            if (_cursorTimeInSquareMs < lowest_cursor_time_in_square_ms)
            {
                TBcursor_time_in_square.Text = lowest_cursor_time_in_square_ms.ToString();
                _cursorTimeInSquareMs = lowest_cursor_time_in_square_ms;
            }
            if (_size < lowest_size)
            {
                TBsquare_size.Text = lowest_size.ToString();
                _size = lowest_size;
            }

            if (_borderWidth < lowest_border_width)
            {
                TBsquare_border.Text = lowest_border_width.ToString();
                _borderWidth = lowest_border_width;
            }

            if (_minSquareSizePercents < lowest_min_square_size_percents)
            {
                TBmin_square_size.Text = lowest_min_square_size_percents.ToString();
                _minSquareSizePercents = lowest_min_square_size_percents;
            }

            else if (_minSquareSizePercents > 100)
            {

                TBmin_square_size.Text = "100";
                _minSquareSizePercents = 100;
            }
        }

        private void restore_default_settings()
        {
            CHBLMB.IsChecked = false;
            CHBRMB.IsChecked = false;
            CHBdoubleLMB.IsChecked = false;
            CHBholdLMB.IsChecked = false;
            CHBholdRMB.IsChecked = false;
            CHBscreen_panning.IsChecked = false;


            TBcursor_idle_before_squares_appear.Text = default_cursor_idle_time_ms.ToString();
            _cursorIdleTimeMs = default_cursor_idle_time_ms;

            TBtime_to_start_mouse.Text = default_time_to_start_mouse_movement_ms.ToString();
            _timeToStartMouseMovementMs = default_time_to_start_mouse_movement_ms;

            TBcursor_time_in_square.Text = default_cursor_time_in_square_ms.ToString();
            _cursorTimeInSquareMs = default_cursor_time_in_square_ms;

            CHBrun_at_startup.IsChecked = false;
            CHBstart_minimized.IsChecked = false;
            CHBminimize_to_tray.IsChecked = false;
            CHBcheck_for_updates.IsChecked = false;

            TBsquare_size.Text = default_size.ToString();
            _size = default_size;

            TBsquare_border.Text = default_border_width.ToString();
            _borderWidth = default_border_width;

            _squareColor1Uint = default_color1_uint;
            _squareColor2Uint = default_color2_uint;

            Bsquare_color1.Background = new SolidColorBrush(Color.FromUInt32(_squareColor1Uint));
            _color1 = Avalonia.Media.Color.FromUInt32(_squareColor1Uint);

            Bsquare_color2.Background = new SolidColorBrush(Color.FromUInt32(_squareColor2Uint));
            _color2 = Avalonia.Media.Color.FromUInt32(_squareColor2Uint);

            TBmin_square_size.Text = default_min_square_size_percents.ToString();

            TBscreen_size.Text = "";
            int x = Screens.Primary.Bounds.Width;
            int y = Screens.Primary.Bounds.Height;

            // TBscreen_resolution.Text = x + "x" + y;
        }

        private void regenerate_squares()
        {
        //     regenerate_SL();
        //     regenerate_SR();
        //     regenerate_SM();
        //     regenerate_SLH();
        //     regenerate_SRH();
        }

        public int X = 0, Y = 0;
        private int _maxX;
        private int _maxY;
        public bool SquaresVisible = false;
        private int _showZone;
        private int _slStartX;
        private int _slStartY;
        private int _slEndX;
        private int _slEndY;
        private int _srStartX;
        private int _srStartY;
        private int _srEndX;
        private int _srEndY;
        private int _smStartX;
        private int _smStartY;
        private int _smEndX;
        private int _smEndY;
        private int _slhStartX;
        private int _slhStartY;
        private int _slhEndX;
        private int _slhEndY;
        private int _srhStartX;
        private int _srhStartY;
        private int _srhEndX;
        private int _srhEndY;

        private void monitor_mouse2(CancellationToken token)
        {
            //Thread.Sleep(5000); //debug only

            int x1, y1;
            bool pressedUp, pressedLeft, pressedDown, pressedRight;
            pressedUp = pressedLeft = pressedDown = pressedRight = false;
            System.Drawing.Point one;
            int[] mouseCoords;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                //user may change screen resolution so max_x and max_y should be updated
                _maxX = Screens.Primary.Bounds.Width - 1;
                _maxY = Screens.Primary.Bounds.Height - 1;
                mouseCoords = GetCursorPosition();
                x1 = mouseCoords[0];
                y1 = mouseCoords[1];

                if (x1 == 0) //if (x1 == 0 && pressed_left == false) would be a mistake (we need
                             //continous pressing as well as holding)
                {
                    key_down(VirtualKeyCode.LEFT);
                    pressedLeft = true;
                }
                else if (x1 != 0 && pressedLeft == true)
                {
                    key_up(VirtualKeyCode.LEFT);
                    pressedLeft = false;
                }

                if (x1 == _maxX)
                {
                    key_down(VirtualKeyCode.RIGHT);
                    pressedRight = true;
                }
                else if (x1 != _maxX && pressedRight == true)
                {
                    key_up(VirtualKeyCode.RIGHT);
                    pressedRight = false;
                }

                if (y1 == 0)
                {
                    key_down(VirtualKeyCode.UP);
                    pressedUp = true;
                }
                else if (y1 != 0 && pressedUp == true)
                {
                    key_up(VirtualKeyCode.UP);
                    pressedUp = false;
                }

                if (y1 == _maxY)
                {
                    key_down(VirtualKeyCode.DOWN);
                    pressedDown = true;
                }
                else if (y1 != _maxY && pressedDown == true)
                {
                    key_down(VirtualKeyCode.DOWN);
                    pressedDown = false;
                }

                real_sleep(50);
            }
        }

        private void calculate_squares_start_positions()
        {
            _displacement = (int)(_size / 2);
            _showZone = _size + _displacement;

            _slStartX = X - _showZone;
            _slStartY = Y - _showZone;
            _slEndX = _slStartX + _size;
            _slEndY = _slStartY + _size;

            _srStartX = X + _displacement;
            _srStartY = Y - _showZone;
            _srEndX = _srStartX + _size;
            _srEndY = _srStartY + _size;

            _smStartX = X - _displacement;
            _smStartY = Y - 2 * _size;
            _smEndX = _smStartX + _size;
            _smEndY = _smStartY + _size;

            _slhStartX = X - 2 * _size;
            _slhStartY = Y - _displacement;
            _slhEndX = _slhStartX + _size;
            _slhEndY = _slhStartY + _size;

            _srhStartX = X + _size;
            _srhStartY = Y - _displacement;
            _srhEndX = _srhStartX + _size;
            _srhEndY = _srhStartY + _size;
        }

        private int _bannedX = -1;
        private int _bannedY = -1;
        private int _previousSize = 0;

        private void monitor_mouse()
        {
            int i = 0;
            int x1 = 0, x2 = 0, y1 = 0, y2 = 0;
            int[] mouseCoords;

            while (true)
            {
                mouseCoords = GetCursorPosition();
                x1 = mouseCoords[0];
                y1 = mouseCoords[1];
                Thread.Sleep(loop_time_ms);
                mouseCoords = GetCursorPosition();
                x2 = mouseCoords[0];
                y2 = mouseCoords[1];


                //max_x and max_y are updated in monitor_mouse2 by THRmouse_monitor2 which works
                //only when screen_panning == true
                if (_screenPanning && (x2 == 0 || x2 == _maxX || y2 == 0 || y2 == _maxY))
                {
                    _bannedX = -1;
                    _bannedY = -1;
                    i = 0;
                    mouse_move_detected();
                }
                else if (x1 == x2 && y1 == y2)
                {
                    if (x1 != _bannedX || y1 != _bannedY)
                    {
                        i++;
                    }
                }
                else
                {
                    _bannedX = -1;
                    _bannedY = -1;
                    i = 0;
                    mouse_move_detected();
                }
                if (i > _loopsToShowSquaresAfterCursorIdle && SquaresVisible == false)
                {
                    X = x2;
                    Y = y2;

                    TimeSpan timeElapsedSinceLastClick = new TimeSpan();
                    if (_lastClickTime != null)
                    {
                        timeElapsedSinceLastClick = DateTime.Now - _lastClickTime;
                    }

                    if (_lastClickTime != null
                        && timeElapsedSinceLastClick.TotalMilliseconds >
                        _cursorIdleTimeMs + additional_cursor_idle_time)
                    {
                        int originalSize = _size;
                        int minimumSize =
                            (int)Math.Round((double)originalSize * _minSquareSizePercents / 100);

                        calculate_squares_start_positions();

                        int screenWidth = Screens.Primary.Bounds.Width;

                        //if SLH is visible when at minimum_size and 80% or more of SLH 
                        //is out of left screen edge
                        if (_slhEnabled && X > minimumSize && _slhStartX <= -1 * _size * 0.8)
                        {
                            //decrease square size so at least 25% is visible, but square size >= minimum_size
                            _size = (int)(X / 1.25);

                            if (_size < minimumSize)
                            {
                                _size = minimumSize;
                            }
                        }
                        //if SL is visible when at minimum_size and 80% or more of SL 
                        //is out of left screen edge
                        else if (_slEnabled && X > minimumSize / 2 && _slStartX <= -1 * _size * 0.8)
                        {
                            //decrease square size so at least 25% is visible, but square size >= minimum_size
                            _size = (int)(X / 0.75);

                            if (_size < minimumSize)
                            {
                                _size = minimumSize;
                            }
                        }
                        //if SRH is visible when at minimum_size and 80% or more of SRH 
                        //is out of left screen edge
                        else if (_srhEnabled && X < (screenWidth - 1) - minimumSize
                            && _srhStartX >= (screenWidth - 1) - _size * 0.2)
                        {
                            //decrease square size so at least 25% is visible, but square size >= minimum_size
                            _size = (int)(((screenWidth - 1) - X) / 1.25);

                            if (_size < minimumSize)
                            {
                                _size = minimumSize;
                            }
                        }
                        //if SR is visible when at minimum_size and 80% or more of SR
                        //is out of left screen edge
                        else if (_srEnabled && X < (screenWidth - 1) - 0.5 * minimumSize
                            && _srStartX >= (screenWidth - 1) - _size * 0.2)
                        {
                            //decrease square size so at least 25% is visible, but square size >= minimum_size
                            _size = (int)(((screenWidth - 1) - X) / 0.75);

                            if (_size < minimumSize)
                            {
                                _size = minimumSize;
                            }
                        }

                        if (originalSize != _size)
                        {
                            calculate_squares_start_positions();
                            // regenerate_squares();
                        }
                        else if (_previousSize != _size)
                        {
                            // regenerate_squares();
                        }

                        //if top screen edge would cover squares show them below mouse cursor instead
                        if (_smEnabled && _smStartY < -1 * _size * 0.75 || (_smEnabled == false
                            && (_slEnabled || _srEnabled) && _slStartY < -1 * _size * 0.75))
                        {
                            _slStartY = Y + _displacement;
                            _slEndY = _slStartY + _size;

                            _srStartY = Y + _displacement;
                            _srEndY = _srStartY + _size;

                            _smStartY = Y + _size;
                            _smEndY = _smStartY + _size;
                        }

                        if (_slEnabled)
                            show_SL(true);
                        if (_srEnabled)
                            show_SR(true);
                        if (_smEnabled)
                            show_SM(true);
                        if (_slhEnabled)
                            show_SLH(true);
                        if (_srhEnabled)
                            show_SRH(true);

                        SquaresVisible = true;

                        _cts1 = new CancellationTokenSource();
                        _thRsquaresMonitor = new Thread(() => monitor_squares(_cts1.Token));
                        _thRsquaresMonitor.Priority = ThreadPriority.Highest;
                        _thRsquaresMonitor.Start();
                        i = 0;

                        _previousSize = _size;
                        _size = originalSize;
                    }
                }
                else if (i > _loopsToStartMouseMovement && SquaresVisible)
                {
                    _cts1.Cancel();
                    _cts1.Dispose();
                    SquaresVisible = false;
                    if (_slEnabled)
                        show_SL(false);
                    if (_srEnabled)
                        show_SR(false);
                    if (_smEnabled)
                        show_SM(false);
                    if (_slhEnabled)
                        show_SLH(false);
                    if (_srhEnabled)
                        show_SRH(false);
                    i = 0;
                    _bannedX = X;
                    _bannedY = Y;
                }
            }
        }

        private void monitor_squares(CancellationToken token)
        {
            int iSl = 0, iSr = 0, iSm = 0, iSlh = 0, iSrh = 0;
            int iMax = _cursorTimeInSquareMs / loop_time_ms;
            int posX, posY;
            int[] mouseCoords;

            while (iSl < iMax && iSr < iMax && iSm < iMax
                && iSlh < iMax && iSrh < iMax && SquaresVisible)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                mouseCoords = GetCursorPosition();
                posX = mouseCoords[0];
                posY = mouseCoords[1];

                if (_slEnabled)
                {
                    if (is_cursor_in_SL(posX, posY))
                    {
                        iSl++;
                    }
                    else iSl = 0;
                }
                if (_srEnabled)
                {
                    if (is_cursor_in_SR(posX, posY))
                    {
                        iSr++;
                    }
                    else iSr = 0;
                }
                if (_smEnabled)
                {
                    if (is_cursor_in_SM(posX, posY))
                    {
                        iSm++;
                    }
                    else iSm = 0;
                }
                if (_slhEnabled)
                {
                    if (is_cursor_in_SLH(posX, posY))
                    {
                        iSlh++;
                    }
                    else iSlh = 0;
                }
                if (_srhEnabled)
                {
                    if (is_cursor_in_SRH(posX, posY))
                    {
                        iSrh++;
                    }
                    else iSrh = 0;
                }
                Thread.Sleep(loop_time_ms);
            }
            if (iSl >= iMax)
            {
                LmbClick(X, Y, 100);
                if (_slEnabled)
                    show_SL(false);
                if (_srEnabled)
                    show_SR(false);
                if (_smEnabled)
                    show_SM(false);
                if (_slhEnabled)
                    show_SLH(false);
                if (_srhEnabled)
                    show_SRH(false);
                _lastClickTime = DateTime.Now;
                SquaresVisible = false;
            }
            else if (iSr >= iMax)
            {
                RmbClick(X, Y, 100);
                if (_slEnabled)
                    show_SL(false);
                if (_srEnabled)
                    show_SR(false);
                if (_smEnabled)
                    show_SM(false);
                if (_slhEnabled)
                    show_SLH(false);
                if (_srhEnabled)
                    show_SRH(false);
                _lastClickTime = DateTime.Now;
                SquaresVisible = false;
            }
            else if (iSm >= iMax)
            {
                DlmbClick(X, Y, 100);
                if (_slEnabled)
                    show_SL(false);
                if (_srEnabled)
                    show_SR(false);
                if (_smEnabled)
                    show_SM(false);
                if (_slhEnabled)
                    show_SLH(false);
                if (_srhEnabled)
                    show_SRH(false);
                _lastClickTime = DateTime.Now;
                SquaresVisible = false;
            }
            if (iSlh >= iMax)
            {
                LmbHold(X, Y, 100);
                if (_slEnabled)
                    show_SL(false);
                if (_srEnabled)
                    show_SR(false);
                if (_smEnabled)
                    show_SM(false);
                if (_slhEnabled)
                    show_SLH(false);
                if (_srhEnabled)
                    show_SRH(false);
                _lastClickTime = DateTime.Now;
                SquaresVisible = false;
            }
            else if (iSrh >= iMax)
            {
                RmbHold(X, Y, 100);
                if (_slEnabled)
                    show_SL(false);
                if (_srEnabled)
                    show_SR(false);
                if (_smEnabled)
                    show_SM(false);
                if (_slhEnabled)
                    show_SLH(false);
                if (_srhEnabled)
                    show_SRH(false);
                _lastClickTime = DateTime.Now;
                SquaresVisible = false;
            }
        }


        private void mouse_move_detected()
        {
            if (SquaresVisible)
            {
                int x1, y1;
                int[] mouseCoords;

                mouseCoords = GetCursorPosition();
                x1 = mouseCoords[0];
                y1 = mouseCoords[1];

                if (is_cursor_outside_zone(x1, y1))
                {
                    if (_slEnabled)
                        show_SL(false);
                    if (_srEnabled)
                        show_SR(false);
                    if (_smEnabled)
                        show_SM(false);
                    if (_slhEnabled)
                        show_SLH(false);
                    if (_srhEnabled)
                        show_SRH(false);
                    SquaresVisible = false;
                }
            }
        }

        private bool is_cursor_in_SL(int x1, int y1)
        {
            if (x1 >= _slStartX && x1 <= _slEndX
                && y1 >= _slStartY && y1 <= _slEndY)
            {
                return true;
            }
            else return false;
        }

        private bool is_cursor_in_SR(int x1, int y1)
        {
            if (x1 >= _srStartX && x1 <= _srEndX
                && y1 >= _srStartY && y1 <= _srEndY)
            {
                return true;
            }
            else return false;
        }

        private bool is_cursor_in_SM(int x1, int y1)
        {
            if (x1 >= _smStartX && x1 <= _smEndX
                && y1 >= _smStartY && y1 <= _smEndY)
            {
                return true;
            }
            else return false;
        }

        private bool is_cursor_in_SLH(int x1, int y1)
        {
            if (x1 >= _slhStartX && x1 <= _slhEndX
                && y1 >= _slhStartY && y1 <= _slhEndY)
            {
                return true;
            }
            else return false;
        }

        private bool is_cursor_in_SRH(int x1, int y1)
        {
            if (x1 >= _srhStartX && x1 <= _srhEndX
                && y1 >= _srhStartY && y1 <= _srhEndY)
            {
                return true;
            }
            else return false;
        }

        private bool is_cursor_outside_zone(int x1, int y1)
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
            if (x1 > X + _showZone + _displacement || x1 < X - _showZone - _displacement
                || y1 > Y + _showZone || y1 < Y - _showZone - _displacement)
            {
                return true;
            }
            else return false;
        }

        private void real_sleep(int time)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            do
            {
                Thread.Sleep(10);
            }
            while (stopwatch.ElapsedMilliseconds < time);
            stopwatch.Stop();
        }

        private delegate void Callback1(bool show);

        private void show_SL(bool show)
        {
            if (_sl == null)
                return;
            if (!_sl.CheckAccess())
            {
                try
                {
                    Callback1 d = new Callback1(show_SL);
                    Dispatcher.UIThread.Invoke(new Action(() => d(show)));
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
                    _sl.Position = new PixelPoint(_slStartX, _slStartY);
                    _sl.Show();
                    // InputX11.HideSquareTaskbarIcon();
                }
                else _sl.Hide();
            }
        }

        private void show_SR(bool show)
        {
            if (_sr == null)
                return;
            if (!_sr.CheckAccess())
            {
                try
                {
                    Callback1 d = new Callback1(show_SR);
                    Dispatcher.UIThread.Invoke(new Action(() => d(show)));
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
                    _sr.Position = new PixelPoint(_srStartX, _srStartY);
                    _sr.Show();
                }
                else _sr.Hide();
            }
        }

        private void show_SM(bool show)
        {
            if (_sm == null)
                return;
            if (!_sm.CheckAccess())
            {
                try
                {
                    Callback1 d = new Callback1(show_SM);
                    Dispatcher.UIThread.Invoke(new Action(() => d(show)));
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
                    _sm.Position = new PixelPoint(_smStartX, _smStartY);
                    _sm.Show();
                }
                else _sm.Hide();
            }
        }

        private void show_SLH(bool show)
        {
            if (_slh == null)
                return;
            if (!_slh.CheckAccess())
            {
                try
                {
                    Callback1 d = new Callback1(show_SLH);
                    Dispatcher.UIThread.Invoke(new Action(() => d(show)));
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
                    _slh.Position = new PixelPoint(_slhStartX, _slhStartY);
                    _slh.Show();
                }
                else _slh.Hide();
            }
        }

        private void show_SRH(bool show)
        {
            if (_srh == null)
                return;
            if (!_srh.CheckAccess())
            {
                try
                {
                    Callback1 d = new Callback1(show_SRH);
                    Dispatcher.UIThread.Invoke(new Action(() => d(show)));
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
                    _srh.Position = new PixelPoint(_srhStartX, _srhStartY);
                    _srh.Show();
                }
                else _srh.Hide();
            }
        }

        private delegate void Callback2();

        private void create_SL()
        {
            if (_sl != null && !_sl.CheckAccess())
            {
                try
                {
                    Callback2 d = new Callback2(create_SL);
                    Dispatcher.UIThread.Invoke(new Action(() => d()));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (_sl != null)
                    _sl.Close();

                _sl = new Square(_size, _borderWidth, _color1, _color2);

                _sl.Title = "Square SL";

                _sl.Topmost = true;
                _sl.Show();

                _sl.Height = _size;
                _sl.Width = _size;

                _sl.Hide();
            }
        }

        private void destroy_SL()
        {
            if (_sl != null)
            {
                _sl.Close();
            }

            Console.WriteLine("");
        }

        private void create_SR()
        {
            if (_sr != null && !_sr.CheckAccess())
            {
                try
                {
                    Callback2 d = new Callback2(create_SR);
                    Dispatcher.UIThread.Invoke(new Action(() => d()));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (_sr != null)
                    _sr.Close();

                _sr = new Square(_size, _borderWidth, _color1, _color2);
                _sr.Topmost = true;
                _sr.Show();
                _sr.Height = _size;
                _sr.Width = _size;

                _sr.Hide();
            }
        }

        private void destroy_SR()
        {
            if (_sr != null)
            {
                _sr.Close();
            }

            Console.WriteLine("");
        }

        private void create_SM()
        {
            if (_sm != null && !_sm.CheckAccess())
            {
                try
                {
                    Callback2 d = new Callback2(create_SM);
                    Dispatcher.UIThread.Invoke(new Action(() => d()));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (_sm != null)
                    _sm.Close();

                _sm = new Square(_size, _borderWidth, _color1, _color2);
                _sm.Topmost = true;
                _sm.Show();
                _sm.Height = _size;
                _sm.Width = _size;

                _sm.Hide();
            }
        }

        private void destroy_SM()
        {
            if (_sm != null)
            {
                _sm.Close();
            }

            Console.WriteLine("");
        }

        private void create_SLH()
        {
            if (_slh != null && !_slh.CheckAccess())
            {
                try
                {
                    Callback2 d = new Callback2(create_SLH);
                    Dispatcher.UIThread.Invoke(new Action(() => d()));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (_slh != null)
                    _slh.Close();

                _slh = new Square(_size, _borderWidth, _color1, _color2);
                _slh.Topmost = true;
                _slh.Show();
                _slh.Height = _size;
                _slh.Width = _size;

                _slh.Hide();
            }
        }

        private void destroy_SLH()
        {
            if (_slh != null)
            {
                _slh.Close();
            }

            Console.WriteLine("");
        }

        private void create_SRH()
        {
            if (_srh != null && !_srh.CheckAccess())
            {
                try
                {
                    Callback2 d = new Callback2(create_SRH);
                    Dispatcher.UIThread.Invoke(new Action(() => d()));
                }
                catch (ObjectDisposedException ex)
                {
                    //
                }
            }
            else
            {
                if (_srh != null)
                    _srh.Close();

                _srh = new Square(_size, _borderWidth, _color1, _color2);
                _srh.Topmost = true;
                _srh.Show();
                _srh.Height = _size;
                _srh.Width = _size;

                _srh.Hide();
            }
        }

        private void destroy_SRH()
        {
            if (_srh != null)
            {
                _srh.Close();
            }

            Console.WriteLine("");
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

        private void ni_MouseClick(object sender, PointerEventArgs e)
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

            // release_buttons_and_keys();
            Process.GetCurrentProcess().Kill();
        }

        private void MIdefault_colors_Click(object sender, RoutedEventArgs e)
        {
            _savingEnabled = false; //to avoid multiple saves

            _squareColor1Uint = default_color1_uint;
            _squareColor2Uint = default_color2_uint;

            Bsquare_color1.Background = new SolidColorBrush(Color.FromUInt32(_squareColor1Uint));
            _color1 = Avalonia.Media.Color.FromUInt32(_squareColor1Uint);

            Bsquare_color2.Background = new SolidColorBrush(Color.FromUInt32(_squareColor2Uint));
            _color2 = Avalonia.Media.Color.FromUInt32(_squareColor2Uint);

            // regenerate_squares();

            _savingEnabled = true;

            save_settings();
        }

        private void MIdefault_settings_Click(object sender, RoutedEventArgs e)
        {
            _savingEnabled = false; //to avoid multiple saves

            restore_default_settings();

            regenerate_squares();

            _savingEnabled = true;
            save_settings();
        }


        private void MIenglish_Click(object sender, RoutedEventArgs e)
        {
            _lang = UiLanguage.En;
            change_language(_lang);
            MIenglish.IsChecked = true;
            MIpolish.IsChecked = false;
            save_settings();
        }

        private void MIpolish_Click(object sender, RoutedEventArgs e)
        {
            _lang = UiLanguage.Pl;
            change_language(_lang);
            MIenglish.IsChecked = false;
            MIpolish.IsChecked = true;
            save_settings();
        }

        private void MImanual_Click(object sender, RoutedEventArgs e)
        {
        WindowManual wmanual = new WindowManual();
            Uri assetUri;
            Stream stream;
            StreamReader reader;

            if (_lang == UiLanguage.En)
            {
                assetUri = new Uri("avares://ClicklessMouse/Assets/1en.md");
                stream = AssetLoader.Open(assetUri);
                reader = new StreamReader(stream);
                wmanual.RTBinstructions.Markdown = reader.ReadToEnd();
            }

            else if (_lang == UiLanguage.Pl)
            {
                assetUri = new Uri("avares://ClicklessMouse/Assets/1pl.md");
                stream = AssetLoader.Open(assetUri);
                reader = new StreamReader(stream);
                wmanual.RTBinstructions.Markdown = reader.ReadToEnd();
            }

            wmanual.DataContext = this;
            wmanual.Show();
        }

        private async void MIabout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string content;
                MyWebClient wc = new MyWebClient();
                content = wc.DownloadString(url_latest_version);

                _latestVersion = content.Replace("\r\n", "").Trim();
            }
            catch (WebException we)
            {
                _latestVersion = "unknown";
            }

            try
            {
                WindowAbout w = new WindowAbout();

                w.Lprogram_name.Content = prog_name;
                w.Llatest_version.Content = "Latest version: " + _latestVersion;
                w.Linstalled_version.Content = "Installed version: " + prog_version;
                w.HBhomepage.Content = url_homepage;
                w.HBhomepage.NavigateUri = new Uri("http://" + url_homepage);
                w.Lcopyright.Content = copyright_text;

                w.Show();
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private void CHBLMB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_savingEnabled)
            {
                show_SL(false);
                show_SR(false);
                show_SM(false);
                show_SLH(false);
                show_SRH(false);
            }

            if (CHBLMB.IsChecked == true)
            {
                _slEnabled = true;
                create_SL();
            }
            else
            {
                _slEnabled = false;
                destroy_SL();
            }

            if (_savingEnabled)
            {
                save_settings();
            }
        }

        private void CHBRMB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_savingEnabled)
            {
                show_SL(false);
                show_SR(false);
                show_SM(false);
                show_SLH(false);
                show_SRH(false);
            }

            if (CHBRMB.IsChecked == true)
            {
                _srEnabled = true;
                create_SR();
            }
            else
            {
                _srEnabled = false;
                destroy_SR();
            }

            if (_savingEnabled)
            {
                save_settings();
            }
        }

        private void CHBdoubleLMB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_savingEnabled)
            {
                show_SL(false);
                show_SR(false);
                show_SM(false);
                show_SLH(false);
                show_SRH(false);
            }

            if (CHBdoubleLMB.IsChecked == true)
            {
                _smEnabled = true;
                create_SM();
            }
            else
            {
                _smEnabled = false;
                destroy_SM();
            }

            if (_savingEnabled)
            {
                save_settings();
            }
        }

        private void CHBholdLMB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_savingEnabled)
            {
                show_SL(false);
                show_SR(false);
                show_SM(false);
                show_SLH(false);
                show_SRH(false);
            }

            if (CHBholdLMB.IsChecked == true)
            {
                _slhEnabled = true;
                create_SLH();
            }
            else
            {
                _slhEnabled = false;
                destroy_SLH();
            }

            if (_savingEnabled)
            {
                save_settings();
            }
        }

        private void CHBholdRMB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_savingEnabled)
            {
                show_SL(false);
                show_SR(false);
                show_SM(false);
                show_SLH(false);
                show_SRH(false);
            }

            if (CHBholdRMB.IsChecked == true)
            {
                _srhEnabled = true;
                create_SRH();
            }
            else
            {
                _srhEnabled = false;
                destroy_SRH();
            }

            if (_savingEnabled)
            {
                save_settings();
            }
        }

        private void CHBscreen_panning_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CHBscreen_panning.IsChecked == true && _thRmouseMonitor2 == null)
            {
                _cts2 = new CancellationTokenSource();
                _thRmouseMonitor2 = new Thread(() => monitor_mouse2(_cts2.Token));
                _thRmouseMonitor2.Priority = ThreadPriority.Highest;
                _thRmouseMonitor2.Start();
                _screenPanning = true;
            }
            else if (CHBscreen_panning.IsChecked == false && _thRmouseMonitor2 != null)
            {
                _screenPanning = false;
                _cts2.Cancel();
                _cts2.Dispose();
                _thRmouseMonitor2 = null;
            }

            if (_savingEnabled)
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

            if (_savingEnabled)
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
                        throw new Exception(L10NResourceMgr["cursor_idle_time_error"] + " 1 ms");
                    }

                    _cursorIdleTimeMs = x;
                    _loopsToShowSquaresAfterCursorIdle = (int)Math.Round(
                        (double)(_cursorIdleTimeMs / loop_time_ms));

                    if (_cursorIdleTimeMs < lowest_cursor_idle_time_ms)
                    {
                        _cursorIdleTimeMs = lowest_cursor_idle_time_ms;
                        _loopsToShowSquaresAfterCursorIdle =
                            (int)Math.Round((double)(lowest_cursor_idle_time_ms / loop_time_ms));
                    }

                    if (_savingEnabled)
                    {
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
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
                        throw new Exception(L10NResourceMgr["time_to_start_mouse_movement_error"] + " 1 ms.");
                    }

                    _timeToStartMouseMovementMs = x;
                    _loopsToStartMouseMovement = (int)Math.Round(
                        (double)_timeToStartMouseMovementMs / loop_time_ms);

                    if (_timeToStartMouseMovementMs < lowest_time_to_start_mouse_movement_ms)
                    {
                        _timeToStartMouseMovementMs = lowest_time_to_start_mouse_movement_ms;
                        _loopsToStartMouseMovement = (int)Math.Round(
                            (double)(lowest_time_to_start_mouse_movement_ms / loop_time_ms));
                    }

                    if (_savingEnabled)
                    {
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
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
                        throw new Exception(L10NResourceMgr["cursor_time_in_square_error"] + " 1 ms.");
                    }

                    _cursorTimeInSquareMs = x;

                    if (_cursorTimeInSquareMs < lowest_cursor_time_in_square_ms)
                        _cursorTimeInSquareMs = lowest_cursor_time_in_square_ms;

                    if (_savingEnabled)
                    {
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private void CHBrun_at_startup_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_savingEnabled)
            {
                //need a .bat file to start an .exe file for some reasons
#if _WINDOWS
                Microsoft.Win32.RegistryKey rkApp = Microsoft.Win32.Registry.CurrentUser
                    .OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
#endif
                if (CHBrun_at_startup.IsChecked == true)
                {
#if _WINDOWS
                    if (rkApp.GetValue(prog_name) == null)
                    {
                        rkApp.SetValue(prog_name,
                            System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".exe", ".vbs"));
                    }
#elif _LINUX
                    if (!File.Exists(Path.Combine(_appFolderPath, "clicklessmouse.desktop")))
                    {
                        File.Copy(Path.Combine("/usr/share", prog_name.Replace(" ", String.Empty), "clicklessmouse.desktop"), Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".config/autostart", "clicklessmouse.desktop"));
                }
#endif
                }
#if _WINDOWS
                else if (rkApp.GetValue(prog_name) != null)
                {
                    rkApp.DeleteValue(prog_name, false);
                }
#elif _LINUX
                else if (File.Exists(Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".config/autostart", "clicklessmouse.desktop")))
                {
                    File.Delete(Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".config/autostart", "clicklessmouse.desktop"));
                }
#endif
                save_settings();
            }
        }

        private void CHBstart_minimized_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_savingEnabled)
            {
                save_settings();
            }
        }

        private void CHBminimize_to_tray_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_savingEnabled)
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
                        throw new Exception(L10NResourceMgr["square_size_error"] + " 1 px.");
                    }

                    _size = x;

                    if (_size < lowest_size)
                        _size = lowest_size;

                    if (_savingEnabled)
                    {
                        regenerate_squares();
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
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
                        throw new Exception(L10NResourceMgr["square_border_error"] + " 1 px.");
                    }

                    _borderWidth = x;

                    if (_savingEnabled)
                    {
                        regenerate_squares();
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private async void Bsquare_color1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ColorPickerDialog colorDialog1 = new ColorPickerDialog() { Color = _color1 };

                var dr = await colorDialog1.ShowDialog<bool>(this);

                if (dr)
                {
                    if (colorDialog1.Color == null)
                        throw new Exception("No color selected");

                    Bsquare_color1.Background = new SolidColorBrush((colorDialog1.Color));
                    _color1 = colorDialog1.Color;

                    _squareColor1Uint = colorDialog1.Color.ToUInt32();

                    regenerate_squares();

                    if (_savingEnabled)
                    {
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private async void Bsquare_color2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ColorPickerDialog colorDialog2 = new ColorPickerDialog() { Color = _color2 };

                var dr = await colorDialog2.ShowDialog<bool>(this);

                if (dr)
                {
                    if (colorDialog2.Color == null)
                        throw new Exception("No color selected");

                    Bsquare_color2.Background = new SolidColorBrush(colorDialog2.Color);
                    _color2 = colorDialog2.Color;

                    _squareColor2Uint = colorDialog2.Color.ToUInt32();

                    regenerate_squares();

                    if (_savingEnabled)
                    {
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
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
                        throw new Exception(L10NResourceMgr["min_square_size_too_low_error"] + " 1%.");
                    }
                    else if (x > 100)
                    {
                        TBmin_square_size.Text = "100";
                        throw new Exception(L10NResourceMgr["min_square_size_too_high_error"] + " 100%.");
                    }

                    _minSquareSizePercents = x;

                    if (_minSquareSizePercents < lowest_min_square_size_percents)
                    {
                        _minSquareSizePercents = lowest_min_square_size_percents;
                    }

                    if (_savingEnabled)
                    {
                        save_settings();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
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
                    throw new Exception(L10NResourceMgr["screen_size_error2"].ToString());
                }
                else
                {
                    int d = int.Parse(TBscreen_size.Text);
                    if (d < 1)
                        throw new Exception(L10NResourceMgr["screen_size_error1"] + ".");

                    int x = Screens.Primary.Bounds.Width;
                    int y = Screens.Primary.Bounds.Height;

                    TBLscreen_resolution.Text += x + "x" + y;

                    double b = Math.Sqrt(Math.Pow(d, 2) / (Math.Pow(x, 2) / Math.Pow(y, 2) + 1));
                    double a = b * x / y;

                    double area = a * b;
                    double pixelSizeMm = area / (x * y) * Math.Pow(25.4, 2);

                    TBsquare_size.Text = Math.Round(50 * 0.0771 / pixelSizeMm).ToString();
                    TBsquare_border.Text = Math.Round(2 * 0.06939 / pixelSizeMm).ToString();

                    regenerate_squares();
                    save_settings();
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString(), ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                var result = await box.ShowAsync();
            }
        }

        private void save_settings()
        {
            if (!File.Exists(_settingsPath))
            {
                if (!Directory.Exists(_appFolderPath))
                    Directory.CreateDirectory(_appFolderPath);

                File.Copy(_defaultSettingsPath, _settingsPath);

            }
            // Load the JSON file
            string json = File.ReadAllText(_settingsPath);

            // Parse JSON as JsonNode
            JsonNode root = JsonNode.Parse(json);

            if (root == null)
            {
                Console.WriteLine("Failed to load JSON.");
                // throw Exception h;
            }
            foreach (ILogical control in Wmain.GetLogicalDescendants())
            {
                if (control is CheckBox cb)
                    root[cb.Name] = cb.IsChecked.ToString();

                else if (control is TextBox tb)
                {
                    if (tb.Name == TBscreen_size.Name && tb.Text == "")
                        root[tb.Name] = "0";
                    else if (tb.Name == Bsquare_color1.Name)
                        root["square_color1_uint"] = _squareColor1Uint.ToString();
                    else if (tb.Name == Bsquare_color2.Name)
                        root["square_color2_uint"] = _squareColor2Uint.ToString();
                    else
                        root[tb.Name] = tb.Text;
                }
            }
            root["lang"] = _lang.ToString();

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_settingsPath, root.ToJsonString(options));

        }

        private async void load_settings()
        {

            try
            {
                if (!File.Exists(_settingsPath))
                {
                    if (!Directory.Exists(_appFolderPath))
                        Directory.CreateDirectory(_appFolderPath);

                    File.Copy(_defaultSettingsPath, _settingsPath);
                }

                // Load the JSON file
                string json = File.ReadAllText(_settingsPath);

                // Parse JSON as JsonNode
                JsonNode root = JsonNode.Parse(json);

                foreach (ILogical control in Wmain.GetLogicalDescendants())
                {
                    if (control is CheckBox cb)
                        // cb.IsChecked = bool.Parse(ReadAppSetting(root,cb.Name));
                        cb.IsChecked = bool.Parse(root[cb.Name].ToString());
                    else if (control is TextBox tb)
                        tb.Text = root[tb.Name].ToString();
                    else if (control is Button btn)
                    {
                        if (btn.Name == "Bsquare_color1")
                            _squareColor1Uint = uint.Parse(root["Bsquare_color1"].ToString());
                        else if (btn.Name == "Bsquare_color2")
                            _squareColor2Uint = uint.Parse(root["Bsquare_color2"].ToString());
                    }
                }

                Bsquare_color1.Background = new SolidColorBrush(Color.FromUInt32(_squareColor1Uint));
                _color1 = Avalonia.Media.Color.FromUInt32(_squareColor1Uint);

                Bsquare_color2.Background = new SolidColorBrush(Color.FromUInt32(_squareColor2Uint));
                _color2 = Avalonia.Media.Color.FromUInt32(_squareColor2Uint);

                Enum.TryParse(root["lang"].ToString(), out _lang);

            }
            catch (Exception ex)
            {
                _loadingError = true;
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString(), ex.Message + L10NResourceMgr["loading_error_msg"],
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

        private async void update_app_if_necessary()
        {
            try
            {
                string content;
                MyWebClient wc = new MyWebClient();
                content = wc.DownloadString(url_latest_version);

                _latestVersion = content.Replace("\r\n", "").Trim();
            }
            catch (WebException we)
            {
                _latestVersion = "unknown";
            }

            bool updateAvailable = false;

            if (_latestVersion != "unknown" &&
                int.Parse(_latestVersion.Replace(".", "")) > int.Parse(prog_version.Replace(".", "")))
            {
                updateAvailable = true;
            }

            if ((bool)CHBcheck_for_updates.IsChecked && updateAvailable)
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

        private void CreateSettingsFile()
        {
            File.Copy(Path.Combine(_appFolderPath, "defaults.json"), _settingsPath);

        }
    }


}
