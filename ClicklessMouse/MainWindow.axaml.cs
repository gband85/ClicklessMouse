using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;
using WindowState = Avalonia.Controls.WindowState;
using Avalonia;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Egorozh.ColorPicker.Dialog;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using WindowsInput;
using WindowsInput.Native;
using Color = Avalonia.Media.Color;

namespace ClicklessMouse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //square toggles
        private bool _slEnabled;
        private bool _srEnabled;
        private bool _sldEnabled;
        private bool _slhEnabled;
        private bool _srhEnabled;
        private bool _screenPanning;

        //default values
        private const int default_cursor_idle_time_ms = 200;
        private const int default_cursor_time_in_square_ms = 100;
        private const int default_time_to_start_mouse_movement_ms = 700;
        private const int default_size = 50;
        private const int default_border_width = 2;
        private const uint default_color1_uint = 4278190335;
        private const uint default_color2_uint = 4294967040;
        private const int default_min_square_size_percents = 60;

        //minimum acceptable values
        private const int lowest_cursor_idle_time_ms = 100;
        private const int lowest_cursor_time_in_square_ms = 10;
        private const int lowest_time_to_start_mouse_movement_ms = 300;
        private const int lowest_size = 10;
        private const int lowest_border_width = 1;
        private const int lowest_min_square_size_percents = 10;

        //after click is performed it gives user more time to start moving mouse without squares blocking top movement path
        private const int additional_cursor_idle_time = 300;

        //how often is cursor position checked (10ms recommended). Changing this requires changing default and lowest: cursor_idle_time_ms, time_to_start_mouse_movement_ms, and cursor_time_in_square_ms
        private const int loop_time_ms = 10;

        //idle time before squares appear                
        private int _cursorIdleTimeMs;

        private int _loopsToShowSquaresAfterCursorIdle;

        //time to start mouse movement after squares appear, before they disappear (700 default, lowest reasonable 500)
        private int _timeToStartMouseMovementMs;

        private int _loopsToStartMouseMovement;

        //cursor hover time in square needed to perform a click
        private int _cursorTimeInSquareMs;

        private int _size;
        private int _borderWidth;
        private string _name;
        private Color _color1 = Color.FromUInt32(default_color1_uint); //square color 1
        private Color _color2 = Color.FromUInt32(default_color2_uint); //square color 2
        private uint _squareColor1Uint;
        private uint _squareColor2Uint;

        //how much square size can be decreased if it would be covered by left or right screen edge                               
        private int _minSquareSizePercents = default_min_square_size_percents;

        //----------------------------------

        private const string prog_name = "Clickless Mouse";
        private const string prog_version = "3.2.0";

        private const string url_latest_version =
            "https://raw.githubusercontent.com/gband85/ClicklessMouse/rebuild/other/latest_version.txt";

        private const string url_homepage = "github.com/gband85/ClicklessMouse";
        private string _latestVersion = "";
        private const string copyright_text = "Copyright © 2025-2026 Garrett Anderson. All rights reserved.";
        private string _settingsFilename = "appsettings.json";
        private string _defaultSettingsFilename = "defaults.json";
        private Square _sl, _sr, _sld, _slh, _srh;
        private DateTime _lastClickTime;
        private CancellationTokenSource _cts1, _cts2;
        private Thread _thRmouseMonitor, _thRsquaresMonitor, _thRmouseMonitor2;
        private int _displacement;

        private bool _savingEnabled = true;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private string _appFolderPath;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private string _settingsPath;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private string _defaultSettingsPath;

        private UiLanguage _lang = UiLanguage.En;
        private bool _loadingError;
        private Process _prc;

        private InputSimulator _sim = new InputSimulator();

        public L10NResourceMgr L10NResourceMgr
            => L10NResourceMgr.Instance;

        private TrayIcon _trayIcon;
        private TrayIcons _trayIcons;

        public ReactiveCommand<Unit, Unit> trayIcon_ClickCommand;

        public MainWindow()
        {
            is_program_already_running();

            // prc = Process.GetCurrentProcess();
            // prc.PriorityClass = ProcessPriorityClass.High;
            // Thread.CurrentThread.Priority = ThreadPriority.Highest;

            InitializeComponent();
            DataContext = this;
            WindowMain.PropertyChanged += WindowMain_StateChanged;

#if _WINDOWS
            _appFolderPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), prog_name);
            _defaultSettingsPath = Path.Combine(AppContext.BaseDirectory, _defaultSettingsFilename);

#elif _LINUX
            _appFolderPath = Path.Combine("/home", Environment.UserName, ".config",
                prog_name.Replace(" ", String.Empty));
            _defaultSettingsPath = Path.Combine(AppContext.BaseDirectory, _defaultSettingsFilename);

#endif
            trayIcon_ClickCommand = ReactiveCommand.Create(trayIcon_Click);
            _trayIcon = new TrayIcon
            {
                Icon = new WindowIcon(
                    AssetLoader.Open(new Uri("avares://ClicklessMouse/Assets/icons/clicklessmouse.ico"))),
                Command = trayIcon_ClickCommand,
                IsVisible = false,
                ToolTipText = "Clickless Mouse"
            };
            _trayIcons = new TrayIcons();
            _trayIcons.Add(_trayIcon);
            TrayIcon.SetIcons(Application.Current, _trayIcons);

            _settingsPath = Path.Combine(_appFolderPath, _settingsFilename);

            WindowMain.Title = prog_name + " " + prog_version;

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


            _thRmouseMonitor = new Thread(monitor_mouse)
            {
                Priority = ThreadPriority.Highest
            };
            _thRmouseMonitor.Start();
        }

        public static TrayIcon? GetPrimaryTrayIcon()
        {
            var app = Application.Current;

            if (app is null)
                return null;

            return TrayIcon.GetIcons(app)?.FirstOrDefault();
        }

        private async void is_program_already_running()
        {
            Process[] arr = Process.GetProcesses();
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
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString() ?? "!!!!",
                    prog_name + "is already running.",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
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
            _color1 = Color.FromUInt32(_squareColor1Uint);

            Bsquare_color2.Background = new SolidColorBrush(Color.FromUInt32(_squareColor2Uint));
            _color2 = Color.FromUInt32(_squareColor2Uint);

            TBmin_square_size.Text = default_min_square_size_percents.ToString();

            TBscreen_size.Text = "";
            if (Screens.Primary != null)
            {
                int x = Screens.Primary.Bounds.Width;
                int y = Screens.Primary.Bounds.Height;
                TBLscreen_resolution.Text = x + "x" + y;
            }
        }

        private void regenerate_squares()
        {
            if (_slEnabled)
                Dispatcher.Post(() => create_square(ref _sl));
            if (_srEnabled)
                Dispatcher.Post(() => create_square(ref _sr));
            if (_sldEnabled)
                Dispatcher.Post(() => create_square(ref _sld));
            if (_slhEnabled)
                Dispatcher.Post(() => create_square(ref _slh));
            if (_srhEnabled)
                Dispatcher.Post(() => create_square(ref _srh));
        }

        public int X, Y;
        private int _maxX;
        private int _maxY;
        public bool SquaresVisible;
        private int _showZone;

        private void monitor_mouse2(CancellationToken token)
        {
            //Thread.Sleep(5000); //debug only

            int x1, y1;
            bool pressedUp, pressedLeft, pressedDown, pressedRight;
            pressedUp = pressedLeft = pressedDown = pressedRight = false;
            int[] mouseCoords;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                //user may change screen resolution so max_x and max_y should be updated
                if (Screens.Primary != null)
                {
                    _maxX = Screens.Primary.Bounds.Width - 1;
                    _maxY = Screens.Primary.Bounds.Height - 1;
                }

                mouseCoords = GetCursorPosition();
                x1 = mouseCoords[0];
                y1 = mouseCoords[1];

                if (x1 == 0) //if (x1 == 0 && pressed_left == false) would be a mistake - we need
                    //continuous pressing as well as holding
                {
                    key_down(VirtualKeyCode.LEFT);
                    pressedLeft = true;
                }
                else if (pressedLeft)
                {
                    key_up(VirtualKeyCode.LEFT);
                    pressedLeft = false;
                }

                if (x1 == _maxX)
                {
                    key_down(VirtualKeyCode.RIGHT);
                    pressedRight = true;
                }
                else if (x1 != _maxX && pressedRight)
                {
                    key_up(VirtualKeyCode.RIGHT);
                    pressedRight = false;
                }

                if (y1 == 0)
                {
                    key_down(VirtualKeyCode.UP);
                    pressedUp = true;
                }
                else if (pressedUp)
                {
                    key_up(VirtualKeyCode.UP);
                    pressedUp = false;
                }

                if (y1 == _maxY)
                {
                    key_down(VirtualKeyCode.DOWN);
                    pressedDown = true;
                }
                else if (y1 != _maxY && pressedDown)
                {
                    key_up(VirtualKeyCode.DOWN);
                    pressedDown = false;
                }

                real_sleep(50);
            }
        }

        private void calculate_squares_start_positions()
        {
            _displacement = _size / 2;
            _showZone = _size + _displacement;

            if (_slEnabled)
            {
                _sl.StartX = X - _showZone;
                _sl.StartY = Y - _showZone;
                _sl.EndX = _sl.StartX + _size;
                _sl.EndY = _sl.StartY + _size;
            }

            if (_srEnabled)
            {
                _sr.StartX = X + _displacement;
                _sr.StartY = Y - _showZone;
                _sr.EndX = _sr.StartX + _size;
                _sr.EndY = _sr.StartY + _size;
            }

            if (_sldEnabled)
            {
                _sld.StartX = X - _displacement;
                _sld.StartY = Y - 2 * _size;
                _sld.EndX = _sld.StartX + _size;
                _sld.EndY = _sld.StartY + _size;
            }

            if (_slhEnabled)
            {
                _slh.StartX = X - 2 * _size;
                _slh.StartY = Y - _displacement;
                _slh.EndX = _slh.StartX + _size;
                _slh.EndY = _slh.StartY + _size;
            }

            if (_srhEnabled)
            {
                _srh.StartX = X + _size;
                _srh.StartY = Y - _displacement;
                _srh.EndX = _srh.StartX + _size;
                _srh.EndY = _srh.StartY + _size;
            }
        }

        private int _bannedX = -1;
        private int _bannedY = -1;
        private int _previousSize;

        private void monitor_mouse()
        {
            int i = 0;

            while (true)
            {
                int[] mouseCoords = GetCursorPosition();
                int x1 = mouseCoords[0];
                int y1 = mouseCoords[1];
                Thread.Sleep(loop_time_ms);
                mouseCoords = GetCursorPosition();
                int x2 = mouseCoords[0];
                int y2 = mouseCoords[1];


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

                    TimeSpan timeElapsedSinceLastClick = DateTime.Now - _lastClickTime;

                    if (timeElapsedSinceLastClick.TotalMilliseconds >
                        _cursorIdleTimeMs + additional_cursor_idle_time)
                    {
                        int originalSize = _size;
                        int minimumSize =
                            (int)Math.Round((double)originalSize * _minSquareSizePercents / 100);

                        calculate_squares_start_positions();

                        if (Screens.Primary != null)
                        {
                            int screenWidth = Screens.Primary.Bounds.Width;

                            //if SLH is visible when at minimum_size and 80% or more of SLH 
                            //is out of left screen edge
                            if (_slhEnabled && X > minimumSize && _slh.StartX <= -1 * _size * 0.8)
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
                            else if (_slEnabled && X > minimumSize / 2 && _sl.StartX <= -1 * _size * 0.8)
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
                                                 && _srh.StartX >= (screenWidth - 1) - _size * 0.2)
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
                                                && _sr.StartX >= (screenWidth - 1) - _size * 0.2)
                            {
                                //decrease square size so at least 25% is visible, but square size >= minimum_size
                                _size = (int)(((screenWidth - 1) - X) / 0.75);

                                if (_size < minimumSize)
                                {
                                    _size = minimumSize;
                                }
                            }
                        }

                        if (originalSize != _size)
                        {
                            calculate_squares_start_positions();
                            regenerate_squares();
                        }
                        else if (_previousSize != _size)
                        {
                            regenerate_squares();
                        }

                        //if top screen edge would cover squares show them below mouse cursor instead
                        if (_sldEnabled)
                        {
                            if (_sld.StartY < -1 * _size * 0.75)
                            {
                                _sld.StartY = Y + _size;
                                _sld.EndY = _sld.StartY + _size;
                            }
                        }

                        if (_slEnabled)
                        {
                            if (_sl.StartY < -1 * _size * 0.75)
                            {
                                _sl.StartY = Y + _displacement;
                                _sl.EndY = _sl.StartY + _size;
                            }
                        }

                        if (_srEnabled)
                        {
                            if (_sr.StartY < -1 * _size * 0.75)
                            {
                                _sr.StartY = Y + _displacement;
                                _sr.EndY = _sr.StartY + _size;
                            }
                        }


                        bool mi_file_open = false;
                        bool mi_restore_open = false;
                        bool mi_language_open = false;
                        bool mi_help_open = false;
                        bool is_this_focused = false;
                        bool is_instructions_focused = false;

                        Dispatcher.UIThread.Post(() => { mi_file_open = MIfile.IsSubMenuOpen; });
                        Dispatcher.UIThread.Post(() => { mi_restore_open = MIrestore.IsSubMenuOpen; });
                        Dispatcher.UIThread.Post(() => { mi_language_open = MIlanguage.IsSubMenuOpen; });
                        Dispatcher.UIThread.Post(() => { mi_help_open = MIhelp.IsSubMenuOpen; });

                        Dispatcher.UIThread.Post(() => { is_this_focused = this.IsFocused; });
                        // Dispatcher.UIThread.Invoke(
                        //     new Action(() => { is_instructions_focused = Wmanual.IsActive; }));

                        if (_slEnabled)
                            show_square(_sl, true);
                        if (_srEnabled)
                            show_square(_sr, true);
                        if (_sldEnabled)
                            show_square(_sld, true);
                        if (_slhEnabled)
                            show_square(_slh, true);
                        if (_srhEnabled)
                            show_square(_srh, true);

                        SquaresVisible = true;

//reopen submenu that was closed because squares appeared
                        if (mi_file_open)
                            Dispatcher.UIThread.Post(() => { MIfile.Open(); });
                        if (mi_restore_open)
                            Dispatcher.UIThread.Post(() => { MIrestore.IsSubMenuOpen = mi_restore_open; });
                        if (mi_language_open)
                            Dispatcher.UIThread.Post(() => { MIlanguage.IsSubMenuOpen = mi_language_open; });
                        if (mi_help_open)
                            Dispatcher.UIThread.Post(() => { MIhelp.IsSubMenuOpen = mi_help_open; });

                        //give back stolen focus (by squares) to a Window if it
                        //was focused before they appeared
                        if (is_this_focused)
                        {
                            Dispatcher.UIThread.Post(() => { this.Focus(); });
                        }
                        // else if (is_instructions_focused)
                        // {
                        //     Dispatcher.UIThread.Invoke(
                        //         new Action(() => { Wmanual.Focus(); }));
                        // }

                        _cts1 = new CancellationTokenSource();
                        _thRsquaresMonitor = new Thread(() => monitor_squares(_cts1.Token))
                        {
                            Priority = ThreadPriority.Highest
                        };
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
                        show_square(_sl, false);
                    if (_srEnabled)
                        show_square(_sr, false);
                    if (_sldEnabled)
                        show_square(_sld, false);
                    if (_slhEnabled)
                        show_square(_slh, false);
                    if (_srhEnabled)
                        show_square(_srh, false);
                    i = 0;
                    _bannedX = X;
                    _bannedY = Y;
                }
            }
        }

        private void monitor_squares(CancellationToken token)
        {
            int iSl = 0, iSr = 0, iSld = 0, iSlh = 0, iSrh = 0;
            int iMax = _cursorTimeInSquareMs / loop_time_ms;
            int posX, posY;
            int[] mouseCoords;

            while (iSl < iMax && iSr < iMax && iSld < iMax
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
                    if (is_cursor_in_square(posX, posY, _sl))
                    {
                        iSl++;
                    }
                    else iSl = 0;
                }

                if (_srEnabled)
                {
                    if (is_cursor_in_square(posX, posY, _sr))
                    {
                        iSr++;
                    }
                    else iSr = 0;
                }

                if (_sldEnabled)
                {
                    if (is_cursor_in_square(posX, posY, _sld))
                    {
                        iSld++;
                    }
                    else iSld = 0;
                }

                if (_slhEnabled)
                {
                    if (is_cursor_in_square(posX, posY, _slh))
                    {
                        iSlh++;
                    }
                    else iSlh = 0;
                }

                if (_srhEnabled)
                {
                    if (is_cursor_in_square(posX, posY, _srh))
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
                    show_square(_sl, false);
                if (_srEnabled)
                    show_square(_sr, false);
                if (_sldEnabled)
                    show_square(_sld, false);
                if (_slhEnabled)
                    show_square(_slh, false);
                if (_srhEnabled)
                    show_square(_srh, false);
                _lastClickTime = DateTime.Now;
                SquaresVisible = false;
            }
            else if (iSr >= iMax)
            {
                RmbClick(X, Y, 100);
                if (_slEnabled)
                    show_square(_sl, false);
                if (_srEnabled)
                    show_square(_sr, false);
                if (_sldEnabled)
                    show_square(_sld, false);
                if (_slhEnabled)
                    show_square(_slh, false);
                if (_srhEnabled)
                    show_square(_srh, false);
                _lastClickTime = DateTime.Now;
                SquaresVisible = false;
            }
            else if (iSld >= iMax)
            {
                DlmbClick(X, Y, 100);
                if (_slEnabled)
                    show_square(_sl, false);
                if (_srEnabled)
                    show_square(_sr, false);
                if (_sldEnabled)
                    show_square(_sld, false);
                if (_slhEnabled)
                    show_square(_slh, false);
                if (_srhEnabled)
                    show_square(_srh, false);
                _lastClickTime = DateTime.Now;
                SquaresVisible = false;
            }

            if (iSlh >= iMax)
            {
                LmbHold(X, Y, 100);
                if (_slEnabled)
                    show_square(_sl, false);
                if (_srEnabled)
                    show_square(_sr, false);
                if (_sldEnabled)
                    show_square(_sld, false);
                if (_slhEnabled)
                    show_square(_slh, false);
                if (_srhEnabled)
                    show_square(_srh, false);
                _lastClickTime = DateTime.Now;
                SquaresVisible = false;
            }
            else if (iSrh >= iMax)
            {
                RmbHold(X, Y, 100);
                if (_slEnabled)
                    show_square(_sl, false);
                if (_srEnabled)
                    show_square(_sr, false);
                if (_sldEnabled)
                    show_square(_sld, false);
                if (_slhEnabled)
                    show_square(_slh, false);
                if (_srhEnabled)
                    show_square(_srh, false);
                _lastClickTime = DateTime.Now;
                SquaresVisible = false;
            }
        }


        private void mouse_move_detected()
        {
            if (SquaresVisible)
            {
                int[] mouseCoords = GetCursorPosition();
                int x1 = mouseCoords[0];
                int y1 = mouseCoords[1];

                if (is_cursor_outside_zone(x1, y1))
                {
                    if (_slEnabled)
                        show_square(_sl, false);
                    if (_srEnabled)
                        show_square(_sr, false);
                    if (_sldEnabled)
                        show_square(_sld, false);
                    if (_slhEnabled)
                        show_square(_slh, false);
                    if (_srhEnabled)
                        show_square(_srh, false);
                    SquaresVisible = false;
                }
            }
        }

        private bool is_cursor_in_square(int x1, int y1, Square square)
        {
            if (square != null)
            {
                if (x1 >= square.StartX && x1 <= square.EndX
                                        && y1 >= square.StartY && y1 <= square.EndY)
                {
                    return true;
                }
            }

            return false;
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

            return false;
        }

        private void real_sleep(int time)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            do
            {
                Thread.Sleep(10);
            } while (stopwatch.ElapsedMilliseconds < time);

            stopwatch.Stop();
        }

        private void show_square(Square square, bool show)
        {
            if (square == null)
                return;
            if (show)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    square.Position = new PixelPoint(square.StartX, square.StartY);
                    square.Show();
                });
            }
            else Dispatcher.UIThread.Post(() => square.Hide());
        }

        private void create_square(ref Square square)
        {
            if (square != null)
            {
                if (square.IsAttachedToVisualTree())
                {
                    square.Close();
                }
            }

            square = new Square(_name, _size, _borderWidth, _color1, _color2)
            {
                Topmost = true,
                Height = _size,
                Width = _size
            };
            square.Show();

            square.Hide();
        }

        void destroy_square(Square square)
        {
            if (square != null)
            {
                if (square.IsAttachedToVisualTree())
                {
                    square.Close();
                }
            }
        }

        //----------------------------------------------------------------------------------

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private void WindowMain_StateChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.ToString() == "WindowState" && WindowMain.WindowState == WindowState.Minimized)
            {
                if (CHBminimize_to_tray.IsChecked == true)
                {
                    WindowMain.Hide();
                    WindowMain.ShowInTaskbar = false;
                    GetPrimaryTrayIcon().IsVisible = true;
                }
            }
        }

        private void WindowMain_Loaded(object? sender, RoutedEventArgs e)
        {
            if (CHBstart_minimized.IsChecked == true)
            {
                WindowMain.WindowState = WindowState.Minimized;
            }
        }

        private void trayIcon_Click()
        {
            WindowMain.ShowInTaskbar = true;
            WindowMain.SetValue(ShowInTaskbarProperty, true);
            WindowMain.Show();
            WindowMain.Focus();

            GetPrimaryTrayIcon().IsVisible = false;
        }

        private void WindowMain_Closing(object sender, WindowClosingEventArgs e)
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
            _color1 = Color.FromUInt32(_squareColor1Uint);

            Bsquare_color2.Background = new SolidColorBrush(Color.FromUInt32(_squareColor2Uint));
            _color2 = Color.FromUInt32(_squareColor2Uint);

            regenerate_squares();

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
            WindowManual windowManual = new WindowManual();
            Uri assetUri;
            Stream stream;
            StreamReader reader;

            if (_lang == UiLanguage.En)
            {
                assetUri = new Uri("avares://ClicklessMouse/Assets/1en.md");
                stream = AssetLoader.Open(assetUri);
                reader = new StreamReader(stream);
                windowManual.RTBinstructions.Markdown = reader.ReadToEnd();
            }

            else if (_lang == UiLanguage.Pl)
            {
                assetUri = new Uri("avares://ClicklessMouse/Assets/1pl.md");
                stream = AssetLoader.Open(assetUri);
                reader = new StreamReader(stream);
                windowManual.RTBinstructions.Markdown = reader.ReadToEnd();
            }

            windowManual.DataContext = this;
            windowManual.ShowDialog(this);
        }

        private async void MIabout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MyWebClient wc = new MyWebClient();
                string content = wc.DownloadString(url_latest_version);

                _latestVersion = content.Replace("\r\n", "").Trim();
            }
            catch (WebException we)
            {
                _latestVersion = "unknown";
            }

            try
            {
                WindowAbout windowAbout = new WindowAbout
                {
                    Lprogram_name =
                    {
                        Content = prog_name
                    },
                    Llatest_version =
                    {
                        Content = "Latest version: " + _latestVersion
                    },
                    Linstalled_version =
                    {
                        Content = "Installed version: " + prog_version
                    },
                    HBhomepage =
                    {
                        Content = url_homepage,
                        NavigateUri = new Uri("https://" + url_homepage)
                    },
                    Lcopyright =
                    {
                        Content = copyright_text
                    }
                };

                windowAbout.ShowDialog(this);
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString() ?? "!!!!",
                    ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowAsync();
            }
        }

        private void CHBLMB_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_savingEnabled)
            {
                show_square(_sl, false);
                show_square(_sr, false);
                show_square(_sld, false);
                show_square(_slh, false);
                show_square(_srh, false);
            }

            if (CHBLMB.IsChecked == true)
            {
                _slEnabled = true;
                _name = "LC";
                create_square(ref _sl);
            }
            else
            {
                _slEnabled = false;
                destroy_square(_sl);
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
                show_square(_sl, false);
                show_square(_sr, false);
                show_square(_sld, false);
                show_square(_slh, false);
                show_square(_srh, false);
            }

            if (CHBRMB.IsChecked == true)
            {
                _srEnabled = true;
                _name = "RC";
                create_square(ref _sr);
            }
            else
            {
                _srEnabled = false;
                destroy_square(_sr);
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
                show_square(_sl, false);
                show_square(_sr, false);
                show_square(_sld, false);
                show_square(_slh, false);
                show_square(_srh, false);
            }

            if (CHBdoubleLMB.IsChecked == true)
            {
                _sldEnabled = true;
                _name = "LD";
                create_square(ref _sld);
            }
            else
            {
                _sldEnabled = false;
                destroy_square(_sld);
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
                show_square(_sl, false);
                show_square(_sr, false);
                show_square(_sld, false);
                show_square(_slh, false);
                show_square(_srh, false);
            }

            if (CHBholdLMB.IsChecked == true)
            {
                _slhEnabled = true;
                _name = "LH";
                create_square(ref _slh);
            }
            else
            {
                _slhEnabled = false;
                destroy_square(_slh);
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
                show_square(_sl, false);
                show_square(_sr, false);
                show_square(_sld, false);
                show_square(_slh, false);
                show_square(_srh, false);
            }

            if (CHBholdRMB.IsChecked == true)
            {
                _srhEnabled = true;
                _name = "RH";
                create_square(ref _srh);
            }
            else
            {
                _srhEnabled = false;
                destroy_square(_srh);
            }

            if (_savingEnabled)
            {
                save_settings();
            }
        }

        private void CHBscreen_panning_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CHBscreen_panning.IsChecked == true)
            {
                _cts2 = new CancellationTokenSource();
                _thRmouseMonitor2 = new Thread(() => monitor_mouse2(_cts2.Token))
                {
                    Priority = ThreadPriority.Highest
                };
                _thRmouseMonitor2.Start();
                _screenPanning = true;
            }
            else if (CHBscreen_panning.IsChecked == false)
            {
                _screenPanning = false;
                _cts2.Cancel();
                _cts2.Dispose();
            }

            if (_savingEnabled)
            {
                save_settings();
            }
        }

        private void CHBcheck_for_updates_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CHBcheck_for_updates.IsChecked == true)
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
                if (TBcursor_idle_before_squares_appear.Text is { Length: > 0 })
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
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString() ?? "!!!!",
                    ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowAsync();
            }
        }

        private async void TBtime_to_start_mouse_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TBtime_to_start_mouse.Text is { Length: > 0 })
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
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString() ?? "!!!!",
                    ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowAsync();
            }
        }

        private async void TBcursor_time_in_square_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TBcursor_time_in_square.Text is { Length: > 0 })
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
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString() ?? "!!!!",
                    ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowAsync();
            }
        }

        private void CHBrun_at_startup_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_savingEnabled)
            {
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
                        File.Copy(
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clicklessmouse.desktop"),
                            Path.Combine("/home", Environment.UserName, ".config/autostart", "clicklessmouse.desktop"));
                    }
#endif
                }
#if _WINDOWS
                else if (rkApp.GetValue(prog_name) != null)
                {
                    rkApp.DeleteValue(prog_name, false);
                }
#elif _LINUX
                else if (File.Exists(Path.Combine("/home", Environment.UserName, ".config/autostart",
                             "clicklessmouse.desktop")))
                {
                    File.Delete(Path.Combine("/home", Environment.UserName, ".config/autostart",
                        "clicklessmouse.desktop"));
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
                if (TBsquare_size.Text is { Length: > 0 })
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
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString() ?? "!!!!",
                    ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowAsync();
            }
        }

        private async void TBsquare_border_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TBsquare_border.Text is { Length: > 0 })
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
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString() ?? "!!!!",
                    ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowAsync();
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
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString() ?? "!!!!",
                    ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowAsync();
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
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString() ?? "!!!!",
                    ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowAsync();
            }
        }

        private async void TBmin_square_size_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TBmin_square_size.Text is { Length: > 0 })
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
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString() ?? "!!!!",
                    ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowAsync();
            }
        }

        private async void Click_Bset_recommended_square(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TBscreen_size.Text is { Length: 0 })
                {
                    throw new Exception(L10NResourceMgr["screen_size_error2"].ToString());
                }
                else
                {
                    int d = int.Parse(TBscreen_size.Text ?? "15");
                    if (d < 1)
                        throw new Exception(L10NResourceMgr["screen_size_error1"] + ".");

                    if (Screens.Primary != null)
                    {
                        int x = Screens.Primary.Bounds.Width;
                        int y = Screens.Primary.Bounds.Height;

                        TBLscreen_resolution.Text = x + "x" + y;

                        double b = Math.Sqrt(Math.Pow(d, 2) / (Math.Pow(x, 2) / Math.Pow(y, 2) + 1));
                        double a = b * x / y;

                        double area = a * b;
                        double pixelSizeMm = area / (x * y) * Math.Pow(25.4, 2);

                        TBsquare_size.Text = Math.Round(50 * 0.0771 / pixelSizeMm).ToString();
                        TBsquare_border.Text = Math.Round(2 * 0.06939 / pixelSizeMm).ToString();
                    }

                    regenerate_squares();
                    save_settings();
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString() ?? "!!!!",
                    ex.Message, ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowAsync();
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

            foreach (ILogical control in WindowMain.GetLogicalDescendants())
            {
                if (control is CheckBox cb)
                {
                    Debug.Assert(cb.Name != null);
                    root?[cb.Name] = cb.IsChecked.ToString();
                }

                else if (control is TextBox tb)
                {
                    if (tb.Name == TBscreen_size.Name && tb.Text == "")
                    {
                        Debug.Assert(tb.Name != null);
                        root?[tb.Name] = "15";
                    }
                    else
                    {
                        Debug.Assert(tb.Name != null);
                        root?[tb.Name] = tb.Text;
                    }
                }
                else if (control is Button btn)
                {
                    if (btn.Name == Bsquare_color1.Name)
                        root?["square_color1_uint"] = _squareColor1Uint.ToString();
                    else if (btn.Name == Bsquare_color2.Name)
                        root?["square_color2_uint"] = _squareColor2Uint.ToString();
                }
            }

            root?["lang"] = _lang.ToString();

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_settingsPath, root?.ToJsonString(options));
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

                foreach (ILogical control in WindowMain.GetLogicalDescendants())
                {
                    if (control is CheckBox cb)
                    {
                        Debug.Assert(cb.Name != null);
                        cb.IsChecked = bool.Parse(root?[cb.Name]?.ToString() ?? "false");
                    }
                    else if (control is TextBox tb)
                    {
                        Debug.Assert(tb.Name != null);
                        tb.Text = root?[tb.Name]?.ToString();
                    }
                    else if (control is Button btn)
                    {
                        if (btn.Name == "Bsquare_color1")
                            _squareColor1Uint =
                                uint.Parse(root?["Bsquare_color1"]?.ToString() ?? default_color1_uint.ToString());
                        else if (btn.Name == "Bsquare_color2")
                            _squareColor2Uint =
                                uint.Parse(root?["Bsquare_color2"]?.ToString() ?? default_color2_uint.ToString());
                    }
                }

                Bsquare_color1.Background = new SolidColorBrush(Color.FromUInt32(_squareColor1Uint));
                _color1 = Color.FromUInt32(_squareColor1Uint);

                Bsquare_color2.Background = new SolidColorBrush(Color.FromUInt32(_squareColor2Uint));
                _color2 = Color.FromUInt32(_squareColor2Uint);

                Enum.TryParse(root?["lang"]?.ToString(), out _lang);
            }
            catch (Exception ex)
            {
                _loadingError = true;
                var box = MessageBoxManager.GetMessageBoxStandard(L10NResourceMgr["error_title"].ToString() ?? "!!!!",
                    ex.Message + L10NResourceMgr["loading_error_msg"],
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowAsync();
            }
        }

        private async void update_app_if_necessary()
        {
            try
            {
                MyWebClient wc = new MyWebClient();
                string content = wc.DownloadString(url_latest_version);

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

            if (CHBcheck_for_updates.IsChecked == true && updateAvailable)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("New Version Available", "A new program version" +
                    " is available. Do you want to download it now?", ButtonEnum.YesNo,
                    MsBox.Avalonia.Enums.Icon.Question);
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
    }
}