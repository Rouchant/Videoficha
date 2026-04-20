using System;
using System.Windows;
using System.Windows.Threading;
using Videoficha.Features.SystemDiagnostics.ViewModels;
using Videoficha.Infrastructure.Services;

namespace Videoficha.Features.SystemDiagnostics.Views
{
    public partial class SystemInfoWindow : Window
    {
        private readonly SystemInfoViewModel _viewModel;
        private DispatcherTimer? inactivityTimer;
        private const int InactivityThreshold = 300000; // 5 minutes

        public SystemInfoWindow()
        {
            InitializeComponent();
            
            _viewModel = new SystemInfoViewModel(new ConfigService());
            DataContext = _viewModel;

            InitializeTimer();
        }

        private void InitializeTimer()
        {
            inactivityTimer = new DispatcherTimer();
            inactivityTimer.Interval = TimeSpan.FromMilliseconds(InactivityThreshold);
            inactivityTimer.Tick += InactivityTimer_Tick;

            this.MouseMove += ResetInactivityTimer;
            this.KeyDown += ResetInactivityTimer;

            inactivityTimer.Start();
        }

        private void ResetInactivityTimer(object? sender, EventArgs e)
        {
            inactivityTimer?.Stop();
            inactivityTimer?.Start();
        }

        private void InactivityTimer_Tick(object? sender, EventArgs e)
        {
            this.Close();
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
