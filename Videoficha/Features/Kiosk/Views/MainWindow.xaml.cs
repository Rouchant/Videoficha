using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;
using Videoficha.Features.Kiosk.ViewModels;
using Videoficha.Infrastructure.Services;

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

        private readonly string BackgroundVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Samples", "background-generic.mp4");
        private readonly string DefaultVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Samples", "landing-generic.mp4");
        private readonly string PromoVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Samples", "promo-generic.mp4");

        public MainWindow()
        {
            InitializeComponent();
            
            var systemProvider = new SystemProvider();
            var configService = new ConfigService();
            _viewModel = new MainViewModel(systemProvider, configService);
            DataContext = _viewModel;

            promoVideo.MediaFailed += (s, e) => {
                // Si falla el video personalizado, intentar el genérico
                if (promoVideo.Source?.LocalPath != PromoVideoPath)
                {
                    promoVideo.Source = new Uri(PromoVideoPath);
                    promoVideo.Play();
                }
            };

            this.Loaded += MainWindow_Loaded;

            // Admin Timer
            _adminClickTimer = new DispatcherTimer();
            _adminClickTimer.Interval = TimeSpan.FromSeconds(2);
            _adminClickTimer.Tick += (s, e) => { _adminClickCount = 0; _adminClickTimer.Stop(); };

            // Timer para inactividad (Chequeo global cada segundo)
            _inactivityTimer = new DispatcherTimer();
            _inactivityTimer.Interval = TimeSpan.FromSeconds(1);
            _inactivityTimer.Tick += InactivityTimer_Tick;
            _inactivityTimer.Start();
        }

        private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
            
            PlayBackgroundVideo();

            await _viewModel.InitializeAsync();

            if (!string.IsNullOrEmpty(_viewModel.Settings.SelectedVideoPath) && File.Exists(_viewModel.Settings.SelectedVideoPath))
            {
                PlaySelectedVideo(_viewModel.Settings.SelectedVideoPath);
            }
            else
            {
                PlayDefaultVideo();
            }
        }

        private void PlayBackgroundVideo()
        {
            try
            {
                // El video de fondo (backgroundVideo) es solo estético detrás de todo.
                if (File.Exists(BackgroundVideoPath))
                {
                    backgroundVideo.Source = new Uri(Path.GetFullPath(BackgroundVideoPath));
                    backgroundVideo.Play();
                }
            }
            catch { }
        }

        private void PlayPromoVideo()
        {
            try
            {
                string promoPath = _viewModel.Settings.InactivityVideoPath;
                if (string.IsNullOrEmpty(promoPath) || !File.Exists(promoPath))
                {
                    promoPath = PromoVideoPath; // Fallback al genérico
                }

                if (File.Exists(promoPath) && IsVideoFile(promoPath))
                {
                    // 1. Detener el video de la ficha para liberar recursos
                    videoPlayer.Stop();
                    
                    // 2. Mostrar el contenedor
                    PromoGrid.Visibility = Visibility.Visible;
                    MainContentGrid.Visibility = Visibility.Collapsed;
                    
                    // 3. Forzar recarga limpia
                    promoVideo.Source = null;
                    promoVideo.Source = new Uri(Path.GetFullPath(promoPath));
                    promoVideo.Volume = 1.0;
                    
                    // 4. Iniciar reproducción
                    promoVideo.Position = TimeSpan.Zero;
                    promoVideo.Play();
                }
            }
            catch { }
        }

        private void StopPromoVideo()
        {
            promoVideo.Stop();
            promoVideo.Source = null; // Liberar archivo
            
            PromoGrid.Visibility = Visibility.Collapsed;
            MainContentGrid.Visibility = Visibility.Visible;
            
            // Reanudar el video de la ficha
            PlayDefaultVideo();
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

        private void OnBackgroundMediaEnded(object sender, RoutedEventArgs e)
        {
            backgroundVideo.Position = TimeSpan.Zero;
            backgroundVideo.Play();
        }

        private void OnPromoMediaEnded(object sender, RoutedEventArgs e)
        {
            promoVideo.Position = TimeSpan.Zero;
            promoVideo.Play();
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
                    if (!string.IsNullOrEmpty(configWindow.VideoFilePath))
                    {
                        PlaySelectedVideo(configWindow.VideoFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al recargar la configuración: {ex.Message}", "Error de Configuración", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PlaySelectedVideo(string videoPath)
        {
            try
            {
                if (videoPlayer != null && !string.IsNullOrEmpty(videoPath) && File.Exists(videoPath) && IsVideoFile(videoPath))
                {
                    videoPlayer.Source = new Uri(Path.GetFullPath(videoPath));
                    videoPlayer.Play();
                }
            }
            catch { }
        }

        private void PlayDefaultVideo()
        {
            try
            {
                if (File.Exists(DefaultVideoPath) && IsVideoFile(DefaultVideoPath))
                {
                    videoPlayer.Source = new Uri(Path.GetFullPath(DefaultVideoPath));
                    videoPlayer.Play();
                }
            }
            catch { }
        }

        private bool IsVideoFile(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            return ext == ".mp4" || ext == ".wmv" || ext == ".avi" || ext == ".mov" || ext == ".mkv";
        }

        private void OnMediaEnded(object? sender, RoutedEventArgs e)
        {
            videoPlayer.Position = TimeSpan.Zero;
            videoPlayer.Play();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Limpieza
        }
    }
}
