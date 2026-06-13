using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;
using Videoficha.Features.Kiosk.ViewModels;
using Videoficha.Infrastructure.Services;
using Windows.Media.Playback;
using Windows.Media.Core;
using System.Diagnostics;

namespace Videoficha.Features.Kiosk.Views
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;

        [DllImport("kernel32.dll")]
        private static extern uint SetThreadExecutionState(uint esFlags);
        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001;
        private const uint ES_DISPLAY_REQUIRED = 0x00000002;

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const byte VK_ESCAPE = 0x1B;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        private static int GetIdleTimeInSeconds()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                return (int)((uint)Environment.TickCount - lastInputInfo.dwTime) / 1000;
            }
            return 0;
        }

        private readonly MainViewModel _viewModel;
        private int _adminClickCount = 0;
        private DispatcherTimer _adminClickTimer;
        private DispatcherTimer _inactivityTimer;
        private Windows.Foundation.Point _lastMousePosition;
        private Window? _returnWindow;
        private bool _isSystemGeneratingInput;

        // Native Media Players
        private MediaPlayer? _backgroundPlayer;
        private MediaPlayer? _mainPlayer;
        private MediaPlayer? _promoPlayer;

        private readonly string BackgroundVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Samples", "background-generic.mp4");
        private readonly string DefaultVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Samples", "landing-generic.mp4");
        private readonly string PromoVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Samples", "promo-generic.mp4");

        public MainWindow()
        {
            // 1. Prioridad de Proceso Alta para Kiosko
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            InitializeComponent();
            
            // Configurar ventana modo Kiosko (fullscreen + topmost)
            ConfigureKioskWindow();
            
            // 2. Inicializar Native Media Players
            _backgroundPlayer = new MediaPlayer();
            _mainPlayer = new MediaPlayer();
            _promoPlayer = new MediaPlayer();

            _backgroundPlayer.IsLoopingEnabled = true;
            _backgroundPlayer.IsMuted = true;

            _mainPlayer.IsLoopingEnabled = true;
            _mainPlayer.IsMuted = true;

            _promoPlayer.IsLoopingEnabled = true;
            _promoPlayer.IsMuted = true;

            // Asignar MediaPlayers a los MediaPlayerElements
            backgroundView.SetMediaPlayer(_backgroundPlayer);
            videoView.SetMediaPlayer(_mainPlayer);
            promoView.SetMediaPlayer(_promoPlayer);

            var systemProvider = new SystemProvider();
            var configService = new ConfigService();
            _viewModel = new MainViewModel(systemProvider, configService);
            
            // En WinUI 3 DataContext se asigna al elemento visual raíz
            if (this.Content is FrameworkElement rootElement)
            {
                rootElement.DataContext = _viewModel;
                rootElement.Loaded += MainWindow_Loaded;
            }

            this.Closed += MainWindow_Closed;

            // Admin Timer
            _adminClickTimer = new DispatcherTimer();
            _adminClickTimer.Interval = TimeSpan.FromSeconds(2);
            _adminClickTimer.Tick += (s, e) => { _adminClickCount = 0; _adminClickTimer.Stop(); };

            // Timer para inactividad
            _inactivityTimer = new DispatcherTimer();
            _inactivityTimer.Interval = TimeSpan.FromSeconds(1);
            _inactivityTimer.Tick += InactivityTimer_Tick;
            _inactivityTimer.Start();
        }

        private void ConfigureKioskWindow()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            
            // Quitar título y bordes, establecer pantalla completa
            appWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen);
            
            // Forzar Topmost
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
            
            PlayKioskVideo(_backgroundPlayer, BackgroundVideoPath, isMuted: true);

            await _viewModel.InitializeAsync();

            string videoToPlay = (!string.IsNullOrEmpty(_viewModel.Settings.SelectedVideoPath) && File.Exists(_viewModel.Settings.SelectedVideoPath))
                                 ? _viewModel.Settings.SelectedVideoPath : DefaultVideoPath;
            
            PlayKioskVideo(_mainPlayer, videoToPlay, isMuted: true);
        }

        private void PlayKioskVideo(MediaPlayer? player, string path, bool isMuted = false)
        {
            if (player == null || string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            try
            {
                player.IsMuted = isMuted;
                var oldSource = player.Source as IDisposable;
                
                var source = MediaSource.CreateFromUri(new Uri(path));
                player.Source = source;
                player.Play();

                oldSource?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error MediaPlayer: {ex.Message}");
            }
        }

        private string GetCurrentMainVideoPath()
        {
            if (!string.IsNullOrEmpty(_viewModel.Settings.SelectedVideoPath) && File.Exists(_viewModel.Settings.SelectedVideoPath))
                return _viewModel.Settings.SelectedVideoPath;
            return DefaultVideoPath;
        }

        private string GetCurrentPromoVideoPath()
        {
            string promoPath = _viewModel.Settings.InactivityVideoPath;
            if (string.IsNullOrEmpty(promoPath) || !File.Exists(promoPath))
                return PromoVideoPath;
            return promoPath;
        }

        private void PlayPromoVideo()
        {
            try
            {
                string promoPath = GetCurrentPromoVideoPath();
                if (File.Exists(promoPath))
                {
                    _mainPlayer?.Pause();
                    
                    PromoGrid.Visibility = Visibility.Visible;
                    MainContentGrid.Visibility = Visibility.Collapsed;
                    
                    PlayKioskVideo(_promoPlayer, promoPath, isMuted: true);
                }
            }
            catch { }
        }

        private void StopPromoVideo()
        {
            _promoPlayer?.Pause();
            
            PromoGrid.Visibility = Visibility.Collapsed;
            MainContentGrid.Visibility = Visibility.Visible;
            
            PlayKioskVideo(_mainPlayer, GetCurrentMainVideoPath(), isMuted: true);
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isSystemGeneratingInput) return;

            var currentPoint = e.GetCurrentPoint(this.Content);
            var currentPos = currentPoint.Position;
            
            double diffX = currentPos.X - _lastMousePosition.X;
            double diffY = currentPos.Y - _lastMousePosition.Y;
            
            if (Math.Abs(diffX) < 5 && Math.Abs(diffY) < 5) return;
            
            _lastMousePosition = currentPos;

            ResetInactivity();
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (_isSystemGeneratingInput) return;
            ResetInactivity();
        }

        private void ResetInactivity()
        {
            if (PromoGrid.Visibility == Visibility.Visible)
            {
                StopPromoVideo();
            }
            
            _inactivityTimer.Stop();
            _inactivityTimer.Start();
        }

        private void InactivityTimer_Tick(object? sender, object e)
        {
            int idleTime = GetIdleTimeInSeconds();
            int threshold = 30;

            if (idleTime >= threshold)
            {
                _isSystemGeneratingInput = true;
                
                try
                {
                    keybd_event(VK_ESCAPE, 0, 0, 0);
                    keybd_event(VK_ESCAPE, 0, KEYEVENTF_KEYUP, 0);

                    var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                    
                    ShowWindow(hWnd, SW_RESTORE);
                    SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                    
                    this.Activate();

                    if (_returnWindow != null)
                    {
                        _returnWindow.Close();
                        _returnWindow = null;
                    }

                    if (PromoGrid.Visibility != Visibility.Visible)
                    {
                        PlayPromoVideo();
                    }
                }
                finally
                {
                    Task.Delay(100).ContinueWith(_ => _isSystemGeneratingInput = false);
                }
            }
        }

        private void AdminTrigger_Click(object sender, RoutedEventArgs e)
        {
            _adminClickCount++;
            _adminClickTimer.Stop();
            _adminClickTimer.Start();

            if (_adminClickCount >= 4)
            {
                _adminClickCount = 0;
                _adminClickTimer.Stop();
                OpenConfig();
            }
        }

        private void Explore_Click(object sender, RoutedEventArgs e)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            ShowWindow(hWnd, SW_MINIMIZE);
            
            _returnWindow?.Close();
            _returnWindow = new ReturnWindow(this);
            _returnWindow.Activate();
        }

        private async void OpenConfig()
        {
            try
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                FileSelectionWindow configWindow = new FileSelectionWindow(hWnd);
                configWindow.XamlRoot = this.Content.XamlRoot;
                var result = await configWindow.ShowAsync();
                
                if (result == ContentDialogResult.Primary)
                {
                    _viewModel.ReloadSettings();
                    PlayKioskVideo(_mainPlayer, GetCurrentMainVideoPath(), isMuted: true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al recargar la configuración: {ex.Message}");
            }
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            backgroundView.SetMediaPlayer(null);
            videoView.SetMediaPlayer(null);
            promoView.SetMediaPlayer(null);

            _backgroundPlayer?.Dispose();
            _mainPlayer?.Dispose();
            _promoPlayer?.Dispose();
        }
    }
}
