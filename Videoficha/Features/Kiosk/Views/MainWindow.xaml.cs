using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Videoficha.Features.Kiosk.ViewModels;
using Videoficha.Features.SystemDiagnostics.Views;
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

        public MainWindow()
        {
            InitializeComponent();
            
            // Manual Dependency Injection for now
            var systemProvider = new SystemProvider();
            var configService = new ConfigService();
            _viewModel = new MainViewModel(systemProvider, configService);
            DataContext = _viewModel;

            this.Topmost = false;
            this.KeyDown += Window_KeyDown;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            SystemEvents.PowerModeChanged += OnPowerModeChanged;
        }

        private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
            
            await _viewModel.InitializeAsync();

            if (!string.IsNullOrEmpty(_viewModel.Settings.SelectedVideoPath) && File.Exists(_viewModel.Settings.SelectedVideoPath))
            {
                PlaySelectedVideo(_viewModel.Settings.SelectedVideoPath);
            }
            else
            {
                SelectFiles();
            }
        }

        private void Window_KeyDown(object? sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.S)
                {
                    ShowFileSelectionWindow();
                }
                else if (e.Key == Key.I)
                {
                    _ = ShowSystemInfoEditWindow();
                }
            }
        }

        private async System.Threading.Tasks.Task ShowSystemInfoEditWindow()
        {
            var systemInfoEditWindow = new SystemInfoEditWindow(_viewModel.SystemSpec.ToList(), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "systemInfo.txt"));
            systemInfoEditWindow.Owner = this;
            if (systemInfoEditWindow.ShowDialog() == true)
            {
                await _viewModel.InitializeAsync(); 
            }
        }

        private void ShowFileSelectionWindow()
        {
            FileSelectionWindow fileSelectionWindow = new FileSelectionWindow { Owner = this };
            if (fileSelectionWindow.ShowDialog() == true)
            {
                ProcessSelectedFiles(fileSelectionWindow);
            }
        }

        private void SelectFiles()
        {
            FileSelectionWindow fileSelectionWindow = new FileSelectionWindow { Owner = this };
            if (fileSelectionWindow.ShowDialog() == true)
            {
                ProcessSelectedFiles(fileSelectionWindow);
            }
            else
            {
                PlayDefaultVideo();
                ShowDefaultPDF();
            }
        }

        private void ProcessSelectedFiles(FileSelectionWindow fileSelectionWindow)
        {
            if (!string.IsNullOrEmpty(fileSelectionWindow.VideoFilePath))
            {
                _viewModel.Settings.SelectedVideoPath = fileSelectionWindow.VideoFilePath;
                _viewModel.SaveSettings();
                PlaySelectedVideo(fileSelectionWindow.VideoFilePath);
            }

            if (!string.IsNullOrEmpty(fileSelectionWindow.OtherFilePath))
            {
                _viewModel.Settings.SelectedPdfPath = fileSelectionWindow.OtherFilePath;
                _viewModel.SaveSettings();
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
            string defaultVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Samples", "HP.wmv");
            if (File.Exists(defaultVideoPath))
            {
                videoPlayer.Source = new Uri(defaultVideoPath);
                videoPlayer.Play();
            }
        }

        private void ShowDefaultPDF()
        {
            string defaultPDFPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Samples", "sample.pdf");
            if (File.Exists(defaultPDFPath))
            {
                var leerFichaWindow = new LeerFicha(defaultPDFPath) { Owner = this };
                leerFichaWindow.ShowDialog();
            }
        }

        private void OnMediaEnded(object? sender, RoutedEventArgs e)
        {
            videoPlayer.Position = TimeSpan.Zero;
            videoPlayer.Play();
        }

        private void ViewFichaButton_Click(object? sender, RoutedEventArgs e)
        {
            string pdfPath = _viewModel.Settings.SelectedPdfPath;
            if (string.IsNullOrEmpty(pdfPath))
            {
                ShowDefaultPDF();
            }
            else
            {
                var leerFichaWindow = new LeerFicha(pdfPath) { Owner = this };
                leerFichaWindow.ShowDialog();
            }
        }

        private void ViewInfoButton_Click(object? sender, RoutedEventArgs e)
        {
            var systemInfoWindow = new SystemInfoWindow { Owner = this };
            systemInfoWindow.ShowDialog();
        }

        private void OnPowerModeChanged(object? sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume && !string.IsNullOrEmpty(_viewModel.Settings.SelectedVideoPath))
            {
                videoPlayer.Stop();
                videoPlayer.Source = new Uri(_viewModel.Settings.SelectedVideoPath);
                videoPlayer.Play();
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        }
    }
}
