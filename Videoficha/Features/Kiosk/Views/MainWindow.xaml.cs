using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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

        private readonly MainViewModel _viewModel;
        private int _adminClickCount = 0;
        private DispatcherTimer _adminClickTimer;
        private DispatcherTimer _inactivityTimer;

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

            this.Loaded += MainWindow_Loaded;

            // Admin Timer
            _adminClickTimer = new DispatcherTimer();
            _adminClickTimer.Interval = TimeSpan.FromSeconds(2);
            _adminClickTimer.Tick += (s, e) => { _adminClickCount = 0; _adminClickTimer.Stop(); };

            // Inactivity Timer (30 segundos por defecto)
            _inactivityTimer = new DispatcherTimer();
            _inactivityTimer.Interval = TimeSpan.FromSeconds(30);
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
            if (File.Exists(BackgroundVideoPath))
            {
                backgroundVideo.Source = new Uri(BackgroundVideoPath);
                backgroundVideo.Play();
            }
        }

        private void PlayPromoVideo()
        {
            if (File.Exists(PromoVideoPath))
            {
                promoVideo.Source = new Uri(PromoVideoPath);
                promoVideo.Play();
                PromoGrid.Visibility = Visibility.Visible;
                MainContentGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void StopPromoVideo()
        {
            promoVideo.Stop();
            PromoGrid.Visibility = Visibility.Collapsed;
            MainContentGrid.Visibility = Visibility.Visible;
        }

        private void OnUserActivity(object sender, EventArgs e)
        {
            _inactivityTimer.Stop();
            _inactivityTimer.Start();

            if (PromoGrid.Visibility == Visibility.Visible)
            {
                StopPromoVideo();
            }
        }

        private void InactivityTimer_Tick(object? sender, EventArgs e)
        {
            _inactivityTimer.Stop();
            PlayPromoVideo();
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
            var returnButton = new ReturnWindow(this);
            returnButton.Show();
        }

        private void OpenConfig()
        {
            FileSelectionWindow configWindow = new FileSelectionWindow { Owner = this };
            if (configWindow.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(configWindow.VideoFilePath))
                {
                    _viewModel.Settings.SelectedVideoPath = configWindow.VideoFilePath;
                    _viewModel.SaveSettings();
                    PlaySelectedVideo(configWindow.VideoFilePath);
                }
            }
        }

        private void PlaySelectedVideo(string videoPath)
        {
            if (videoPlayer != null && !string.IsNullOrEmpty(videoPath))
            {
                videoPlayer.Source = new Uri(videoPath);
                videoPlayer.Play();
            }
        }

        private void PlayDefaultVideo()
        {
            if (File.Exists(DefaultVideoPath))
            {
                videoPlayer.Source = new Uri(DefaultVideoPath);
                videoPlayer.Play();
            }
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
