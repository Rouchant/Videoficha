using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;
using Videoficha.Features.Kiosk.ViewModels;
using Videoficha.Infrastructure.Services;
using LibVLCSharp.Shared;
using System.Diagnostics;

namespace Videoficha.Features.Kiosk.Views
{
    public partial class MainWindow : Window
    {
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
        private Point _lastMousePosition;
        private Window? _returnWindow;
        private bool _isSystemGeneratingInput;

        // LibVLC Objects
        private LibVLC? _libVLC;
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
            
            // 2. Inicializar LibVLC
            Core.Initialize();
            _libVLC = new LibVLC("--no-osd", "--quiet");

            _backgroundPlayer = new MediaPlayer(_libVLC);
            _mainPlayer = new MediaPlayer(_libVLC);
            _promoPlayer = new MediaPlayer(_libVLC);

            // Asignar MediaPlayers a los VideoViews
            backgroundView.MediaPlayer = _backgroundPlayer;
            videoView.MediaPlayer = _mainPlayer;
            promoView.MediaPlayer = _promoPlayer;

            // Looping
            _backgroundPlayer.EndReached += (s, e) => ThreadPool_LoopVideo(_backgroundPlayer, BackgroundVideoPath);
            _mainPlayer.EndReached += (s, e) => ThreadPool_LoopVideo(_mainPlayer, GetCurrentMainVideoPath());
            _promoPlayer.EndReached += (s, e) => ThreadPool_LoopVideo(_promoPlayer, GetCurrentPromoVideoPath());

            var systemProvider = new SystemProvider();
            var configService = new ConfigService();
            _viewModel = new MainViewModel(systemProvider, configService);
            DataContext = _viewModel;

            this.Loaded += MainWindow_Loaded;

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

        private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
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
                // Limpiar media anterior para evitar fugas de memoria
                var oldMedia = player.Media;
                
                using (var media = new Media(_libVLC, path, FromType.FromPath))
                {
                    // 1. Optimizaciones de Kiosko
                    media.AddOption(":file-caching=150"); 
                    media.AddOption(":hwdec=auto"); // Decodificación por hardware
                    if (isMuted) media.AddOption(":no-audio");

                    player.Play(media);
                }

                if (oldMedia != null) oldMedia.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error VLC: {ex.Message}");
            }
        }

        private void ThreadPool_LoopVideo(MediaPlayer? player, string path)
        {
            // LibVLC EndReached ocurre en un thread distinto
            Task.Run(() => {
                if (player != null && File.Exists(path))
                {
                    PlayKioskVideo(player, path, isMuted: player == _backgroundPlayer || player == _mainPlayer);
                }
            });
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
                    _mainPlayer?.Stop();
                    
                    PromoGrid.Visibility = Visibility.Visible;
                    MainContentGrid.Visibility = Visibility.Collapsed;
                    
                    PlayKioskVideo(_promoPlayer, promoPath, isMuted: true);
                }
            }
            catch { }
        }

        private void StopPromoVideo()
        {
            _promoPlayer?.Stop();
            
            PromoGrid.Visibility = Visibility.Collapsed;
            MainContentGrid.Visibility = Visibility.Visible;
            
            PlayKioskVideo(_mainPlayer, GetCurrentMainVideoPath(), isMuted: true);
        }

        private void OnUserActivity(object sender, EventArgs e)
        {
            if (_isSystemGeneratingInput) return;

            if (e is MouseEventArgs mouseArgs)
            {
                Point currentPos = mouseArgs.GetPosition(this);
                Vector diff = currentPos - _lastMousePosition;
                
                // Umbral de 5 píxeles solicitado por el usuario
                if (Math.Abs(diff.X) < 5 && Math.Abs(diff.Y) < 5) return;
                
                _lastMousePosition = currentPos;
            }

            // Si hay CUALQUIER actividad (ratón > 5px o Teclado)
            if (PromoGrid.Visibility == Visibility.Visible)
            {
                StopPromoVideo();
            }
            
            _inactivityTimer.Stop();
            _inactivityTimer.Start();
        }

        private void InactivityTimer_Tick(object? sender, EventArgs e)
        {
            // Verificamos la inactividad global de Windows (independiente de si la app está minimizada)
            int idleTime = GetIdleTimeInSeconds();
            int threshold = 30; // Volvemos a los 30 segundos estándar

            if (idleTime >= threshold)
            {
                // Bloqueamos la detección de actividad para que el ESC no nos quite el video
                _isSystemGeneratingInput = true;
                
                try
                {
                    // Simular pulsación de ESC para cerrar Menú Inicio o cualquier popup del sistema
                    keybd_event(VK_ESCAPE, 0, 0, 0); // Down
                    keybd_event(VK_ESCAPE, 0, KEYEVENTF_KEYUP, 0); // Up

                    // Si la ventana no está visiblemente al frente, forzarla
                    if (this.WindowState == WindowState.Minimized)
                    {
                        this.WindowState = WindowState.Maximized;
                    }

                    // Fuerza bruta para ponerse encima del Menú Inicio y todo lo demás
                    var helper = new System.Windows.Interop.WindowInteropHelper(this);
                    SetWindowPos(helper.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                    
                    this.Activate();
                    this.Focus();

                    if (_returnWindow != null)
                    {
                        _returnWindow.Close();
                        _returnWindow = null;
                    }

                    // Si no se está reproduciendo ya el video de promoción, iniciarlo
                    if (PromoGrid.Visibility != Visibility.Visible)
                    {
                        PlayPromoVideo();
                    }
                }
                finally
                {
                    // Pequeño retardo para asegurar que el evento de teclado se procese antes de liberar el flag
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
            this.WindowState = WindowState.Minimized;
            
            _returnWindow?.Close(); // Cerrar si ya existe
            _returnWindow = new ReturnWindow(this);
            _returnWindow.Show();
        }

        private void OpenConfig()
        {
            try
            {
                FileSelectionWindow configWindow = new FileSelectionWindow { Owner = this };
                if (configWindow.ShowDialog() == true)
                {
                    _viewModel.ReloadSettings();
                    PlayKioskVideo(_mainPlayer, GetCurrentMainVideoPath(), isMuted: true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al recargar la configuración: {ex.Message}", "Error de Configuración", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _backgroundPlayer?.Dispose();
            _mainPlayer?.Dispose();
            _promoPlayer?.Dispose();
            _libVLC?.Dispose();
        }
    }
}
