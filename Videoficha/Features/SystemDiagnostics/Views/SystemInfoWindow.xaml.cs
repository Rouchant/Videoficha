using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Videoficha.Features.SystemDiagnostics.ViewModels;
using Videoficha.Infrastructure.Services;

namespace Videoficha.Features.SystemDiagnostics.Views
{
    public partial class SystemInfoWindow : ContentDialog
    {
        private readonly SystemInfoViewModel _viewModel;
        private DispatcherTimer? inactivityTimer;
        private const int InactivityThreshold = 300000; // 5 minutes

        public SystemInfoWindow()
        {
            InitializeComponent();
            
            _viewModel = new SystemInfoViewModel(new ConfigService());
            this.DataContext = _viewModel;

            InitializeTimer();
        }

        private void InitializeTimer()
        {
            inactivityTimer = new DispatcherTimer();
            inactivityTimer.Interval = TimeSpan.FromMilliseconds(InactivityThreshold);
            inactivityTimer.Tick += InactivityTimer_Tick;

            this.PointerMoved += ResetInactivityTimer;
            this.KeyDown += ResetInactivityTimer;

            inactivityTimer.Start();
        }

        private void ResetInactivityTimer(object sender, RoutedEventArgs e)
        {
            inactivityTimer?.Stop();
            inactivityTimer?.Start();
        }

        private void InactivityTimer_Tick(object? sender, object e)
        {
            this.Hide();
        }
    }
}
